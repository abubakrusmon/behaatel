using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Common.Services;
using BehaAtel.Infrastructure.Options;
using BehaAtel.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace BehaAtel.Application.UnitTests.Services;

public sealed class TelegramNotifierTests
{
    [Theory]
    [InlineData(TelegramAudienceKind.AllResidents, "Всем жильцам")]
    [InlineData(TelegramAudienceKind.Building, "Здание Корпус А")]
    [InlineData(TelegramAudienceKind.SelectedResidents, "Выбранным: 12 жильцов")]
    public void Format_IncludesAudience(TelegramAudienceKind kind, string expected)
    {
        var names = Enumerable.Range(1, 12).Select(index => $"Жилец {index}").ToArray();
        var text = TelegramMessageFormatter.Format(
            TelegramMessageKind.Notification,
            "Тема",
            "Текст",
            new TelegramAudience(kind, 12, "Корпус А", names),
            "Администратор",
            new DateTime(2026, 11, 9, 12, 30, 0, DateTimeKind.Utc));

        text.Should().Contain(expected);
        if (kind == TelegramAudienceKind.SelectedResidents)
        {
            text.Should().Contain("Жилец 10");
            text.Should().Contain("и ещё 2");
            text.Should().NotContain("Жилец 11");
        }
    }

    [Fact]
    public void Format_EscapesHtmlAndIncludesAuthorAndTime()
    {
        var text = TelegramMessageFormatter.Format(
            TelegramMessageKind.Announcement,
            "<тема> & \"важно\"",
            "A < B & C > D",
            new TelegramAudience(TelegramAudienceKind.AllResidents, 3),
            "Admin & Co",
            new DateTime(2026, 11, 9, 12, 30, 0, DateTimeKind.Utc));

        text.Should().Contain("&lt;тема&gt; &amp; &quot;важно&quot;");
        text.Should().Contain("A &lt; B &amp; C &gt; D");
        text.Should().Contain("Admin &amp; Co");
        text.Should().Contain("2026-11-09 12:30 UTC");
    }

    [Fact]
    public void Format_TruncatesBodyTo3500Characters()
    {
        var text = TelegramMessageFormatter.Format(
            TelegramMessageKind.Notification,
            "Тема",
            new string('x', 4000),
            new TelegramAudience(TelegramAudienceKind.AllResidents, 1),
            "Администратор",
            DateTime.UtcNow);

        var body = text[(text.IndexOf("<b>Текст:</b> ", StringComparison.Ordinal) + "<b>Текст:</b> ".Length)..];
        body.Should().HaveLength(3500);
    }

    [Fact]
    public void Enqueue_WhenTokenOrChatIdIsMissing_SkipsAndLogsWarning()
    {
        var channel = Channel.CreateUnbounded<TelegramQueuedMessage>();
        var logger = new RecordingLogger<TelegramMessageQueue>();
        var queue = new TelegramMessageQueue(
            channel,
            Options.Create(new TelegramSettings { ChatId = "owner-chat" }),
            logger);

        var status = queue.Enqueue(new TelegramNotificationJob("message"));

        status.Should().Be(TelegramQueueStatus.NotConfigured);
        channel.Reader.TryRead(out _).Should().BeFalse();
        logger.Entries.Should().Contain(entry => entry.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task TelegramApiClient_RetriesRateLimitAndThenSucceeds()
    {
        var requests = 0;
        var handler = new RecordingHandler(_ =>
        {
            requests++;
            return requests == 1
                ? new HttpResponseMessage((HttpStatusCode)429)
                {
                    Content = JsonContent("""{"ok":false,"parameters":{"retry_after":0}}""")
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent("""{"ok":true}""")
                };
        });
        using var client = CreateHttpClient(handler);
        var telegram = new TelegramApiClient(
            client,
            Options.Create(new TelegramSettings()),
            new RecordingLogger<TelegramApiClient>());

        var delivered = await telegram.SendAsync("test-chat", "message", CancellationToken.None);

        delivered.Should().BeTrue();
        requests.Should().Be(2);
        handler.Paths.Should().OnlyContain(path => path == "/bot/sendMessage");
    }

    [Fact]
    public async Task TelegramApiClient_StopsAfterFiveFailedAttempts()
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = JsonContent("""{"ok":false,"description":"unavailable"}""")
            });
        using var client = CreateHttpClient(handler);
        var telegram = new TelegramApiClient(
            client,
            Options.Create(new TelegramSettings()),
            new RecordingLogger<TelegramApiClient>());

        var delivered = await telegram.SendAsync("test-chat", "message", CancellationToken.None);

        delivered.Should().BeFalse();
        handler.Paths.Should().HaveCount(5);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.telegram.org/") };

    private static StringContent JsonContent(string content) =>
        new(content, Encoding.UTF8, "application/json");

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }

    private sealed class RecordingHandler(Func<string, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<string> Paths { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            json.RootElement.GetProperty("text").GetString().Should().Be("message");
            Paths.Add(request.RequestUri!.AbsolutePath);
            return responseFactory(json.RootElement.GetProperty("chat_id").GetString()!);
        }
    }
}
