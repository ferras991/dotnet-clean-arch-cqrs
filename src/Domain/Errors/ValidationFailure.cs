namespace Domain.Errors;

public sealed record ValidationFailure(string Field, string Message);