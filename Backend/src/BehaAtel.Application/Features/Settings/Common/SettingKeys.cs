using System.Globalization;

namespace BehaAtel.Application.Features.Settings.Common;

public static class SettingKeys
{
    public const string BillingDueDay = "Billing.DueDay";
    public const string BillingCurrency = "Billing.Currency";
    public const string BillingPenaltyPercent = "Billing.PenaltyPercent";
    public const string TwoFactorEmailAdmins = "Auth.TwoFactorEmail.Admins";
    public const string TwoFactorEmailResidents = "Auth.TwoFactorEmail.Residents";

    public const string DefaultDueDay = "10";
    public const string DefaultCurrency = "TJS";
    public const string DefaultPenaltyPercent = "0";
    public const string DefaultTwoFactorEmailAdmins = "false";
    public const string DefaultTwoFactorEmailResidents = "false";

    public static IReadOnlyList<string> All { get; } =
        [BillingDueDay, BillingCurrency, BillingPenaltyPercent, TwoFactorEmailAdmins, TwoFactorEmailResidents];

    public static bool IsKnown(string? key) => key is
        BillingDueDay or BillingCurrency or BillingPenaltyPercent or TwoFactorEmailAdmins or TwoFactorEmailResidents;

    public static bool IsValidValue(string? key, string? value)
    {
        if (value is null)
        {
            return false;
        }

        return key switch
        {
            BillingDueDay => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var dueDay)
                             && dueDay is >= 1 and <= 28,
            BillingCurrency => value.Length == 3 && value.All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z'),
            BillingPenaltyPercent => decimal.TryParse(
                                         value,
                                         NumberStyles.AllowDecimalPoint,
                                         CultureInfo.InvariantCulture,
                                         out var penaltyPercent)
                                     && penaltyPercent is >= 0 and <= 100,
            TwoFactorEmailAdmins or TwoFactorEmailResidents => bool.TryParse(value, out _),
            _ => false
        };
    }

    public static string DefaultValue(string key) => key switch
    {
        BillingDueDay => DefaultDueDay,
        BillingCurrency => DefaultCurrency,
        BillingPenaltyPercent => DefaultPenaltyPercent,
        TwoFactorEmailAdmins => DefaultTwoFactorEmailAdmins,
        TwoFactorEmailResidents => DefaultTwoFactorEmailResidents,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Неизвестный ключ настройки.")
    };
}
