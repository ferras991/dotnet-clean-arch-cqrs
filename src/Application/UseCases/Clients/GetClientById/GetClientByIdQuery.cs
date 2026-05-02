using Application.Abstractions;

namespace Application.UseCases.Clients.GetClientById;

public sealed record GetClientByIdQuery(Guid ClientId) : IQuery<ClientResponse>;

public sealed record ClientResponse(Guid Id, string FullName, string Email);