namespace BehaAtel.Application.Features.Auth.Common;

public static class EmailTemplates
{
    public static (string Subject, string Body) Verification(string code) =>
        ("Подтверждение почты — BehaAtel", CodeMessage("Подтвердите адрес электронной почты", code));

    public static (string Subject, string Body) PasswordReset(string code) =>
        ("Восстановление пароля — BehaAtel", CodeMessage("Код для сброса пароля", code));

    public static (string Subject, string Body) LoginCode(string code) =>
        ("Код входа — BehaAtel", CodeMessage("Подтвердите вход в BehaAtel", code));

    public static (string Subject, string Body) Approved() =>
        ("Заявка одобрена — BehaAtel", "<p>Ваша заявка одобрена. Теперь вы можете войти в BehaAtel.</p>");

    public static (string Subject, string Body) Rejected(string? reason) =>
        ("Заявка отклонена — BehaAtel",
            $"<p>К сожалению, ваша заявка отклонена.</p>{(string.IsNullOrWhiteSpace(reason) ? string.Empty : $"<p>Причина: {System.Net.WebUtility.HtmlEncode(reason)}</p>")}");

    private static string CodeMessage(string title, string code) =>
        $"<h2>{title}</h2><p>Ваш код:</p><p style=\"font-size:28px;font-weight:bold;letter-spacing:8px\">{code}</p><p>Код действует 15 минут. Не сообщайте его другим.</p>";
}
