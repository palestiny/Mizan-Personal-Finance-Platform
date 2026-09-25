namespace Mizan.Application.Capture;

public sealed class DeterministicCaptureContextResolver : ICaptureContextResolver
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<Guid>> _accounts;

    public DeterministicCaptureContextResolver(IReadOnlyDictionary<string, Guid> accounts)
        : this(accounts.ToDictionary(x => x.Key, x => (IReadOnlyList<Guid>)new[] { x.Value }, StringComparer.OrdinalIgnoreCase))
    {
    }

    public DeterministicCaptureContextResolver(IReadOnlyDictionary<string, IReadOnlyList<Guid>> accounts)
    {
        _accounts = accounts;
    }

    public CaptureResolution Resolve(CaptureInterpretation interpretation)
    {
        var missing = interpretation.MissingFields?.ToList() ?? [];
        var ambiguities = interpretation.Ambiguities?.ToList() ?? [];

        Guid? accountId = null;
        if (string.IsNullOrWhiteSpace(interpretation.AccountReference))
        {
            if (!missing.Contains("Account", StringComparer.OrdinalIgnoreCase))
                missing.Add("Account");
        }
        else if (!_accounts.TryGetValue(interpretation.AccountReference, out var matches) || matches.Count == 0)
        {
            if (!missing.Contains("Account", StringComparer.OrdinalIgnoreCase))
                missing.Add("Account");
        }
        else if (matches.Count > 1)
        {
            if (!ambiguities.Contains("Account", StringComparer.OrdinalIgnoreCase))
                ambiguities.Add("Account");
        }
        else
        {
            accountId = matches[0];
        }

        return new CaptureResolution(accountId, null, missing, ambiguities);
    }
}
