namespace Domain.Errors;

public sealed class ConflictError(string code, string description)
    : DomainError(code, description, ErrorType.Conflict);