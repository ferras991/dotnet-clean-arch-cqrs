namespace Domain.Errors;

public abstract class DomainError(string code, string description, ErrorType errorType)
{
    public string Code { get; } = code;
    public string Description { get; } = description;
    public ErrorType ErrorType { get; } = errorType;
    public IReadOnlyDictionary<string, object?>? Metadata { get; protected init; }

    public override string ToString() => $"{Code}: {Description}";
}