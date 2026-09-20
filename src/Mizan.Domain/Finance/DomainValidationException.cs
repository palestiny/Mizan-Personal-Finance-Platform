namespace Mizan.Domain.Finance;

public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}
