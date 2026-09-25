using System.Globalization;
using System.Text.RegularExpressions;

namespace Mizan.Application.Capture;

public sealed class DeterministicCaptureInterpreter : ICaptureInterpreter
{
    private static readonly Regex AmountRegex = new(@"(?<amount>\d+(?:[.,]\d+)?)\s*(?:جنيه|ج|egp|pound)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public CaptureInterpretation Interpret(CaptureInput input)
    {
        if (string.IsNullOrWhiteSpace(input.RawText))
            throw new ArgumentException("Raw capture text is required.", nameof(input));

        var text = input.RawText.Trim();
        var operation = text.Contains("دخل", StringComparison.OrdinalIgnoreCase)
            ? CaptureOperationType.Income
            : CaptureOperationType.PersonalExpense;

        var amount = ParseAmount(text);
        var currency = amount.HasValue ? "EGP" : null;
        var accountReference = FindReference(text, "البنك", "المحفظة", "الكاش", "نقدي");

        var missing = new List<string>();
        if (!amount.HasValue) missing.Add("Amount");
        if (accountReference is null) missing.Add("Account");

        return new CaptureInterpretation(
            operation,
            amount,
            currency,
            null,
            accountReference,
            null,
            missing.Count == 0 ? null : missing,
            null);
    }

    private static long? ParseAmount(string text)
    {
        var match = AmountRegex.Match(text);
        if (!match.Success) return null;

        var normalized = match.Groups["amount"].Value.Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            return null;

        return checked((long)(amount * 100m));
    }

    private static string? FindReference(string text, params string[] references) =>
        references.FirstOrDefault(reference => text.Contains(reference, StringComparison.OrdinalIgnoreCase));
}
