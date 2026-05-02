namespace Domain.Errors;

public sealed class NotFoundError(string entity, object id)
    : DomainError($"{entity}.NotFound", $"{entity} with id '{id}' was not found.", ErrorType.NotFound);