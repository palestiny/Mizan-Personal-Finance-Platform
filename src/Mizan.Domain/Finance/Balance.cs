namespace Mizan.Domain.Finance;

public static class Balance
{
    public static Money Derive(Account account, IEnumerable<FinancialEffect> effects)
    {
        var applicable = effects
            .Where(e => e.AccountId == account.Id)
            .OrderBy(e => e.EffectiveAt)
            .ThenBy(e => e.RecordedAt)
            .ThenBy(e => e.Order)
            .ThenBy(e => e.Id);

        var total = account.OpeningBalance.MinorUnits;
        foreach (var effect in applicable)
        {
            if (!string.Equals(effect.Amount.Currency, account.Currency, StringComparison.Ordinal))
                throw new DomainValidationException("Effect currency must match account currency.");

            total = checked(total + effect.SignedMinorUnits);
        }

        return Money.FromMinorUnits(total, account.Currency);
    }

    public static Money Rebuild(Account account, IEnumerable<FinancialEffect> effects) =>
        Derive(account, effects);
}
