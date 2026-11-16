using System.Text.Json;
using BehaAtel.Application.Features.Announcements.CreateAnnouncement;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Announcements;

public class CreateAnnouncementValidatorTests
{
    private readonly CreateAnnouncementValidator _validator = new();

    [Fact]
    public void Validate_ValidAnnouncement_IsValid()
    {
        var result = _validator.Validate(new CreateAnnouncementCommand("Объявление", "Текст объявления", true));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_CommandWithoutTelegramOption_DefaultsToSending()
    {
        var command = JsonSerializer.Deserialize<CreateAnnouncementCommand>(
            """{"title":"Объявление","body":"Текст","publishNow":true}""");

        command.Should().NotBeNull();
        command!.SendTelegram.Should().BeTrue();
    }

    [Theory]
    [InlineData("Но", "Текст")]
    [InlineData("", "Текст")]
    [InlineData("Заголовок", "")]
    public void Validate_InvalidAnnouncement_IsInvalid(string title, string body)
    {
        var result = _validator.Validate(new CreateAnnouncementCommand(title, body, false));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TooLongBody_IsInvalid()
    {
        var result = _validator.Validate(new CreateAnnouncementCommand(
            "Заголовок", new string('т', 5001), false));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateAnnouncementCommand.Body));
    }
}
