namespace Mizan.Domain.Finance;

public sealed class IdempotencyConflictException : Exception
{
    public IdempotencyConflictException(string message) : base(message) { }
}
