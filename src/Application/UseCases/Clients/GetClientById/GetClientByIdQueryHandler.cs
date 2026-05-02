using Application.Abstractions;
using Domain;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;

namespace Application.UseCases.Clients.GetClientById;

internal sealed class GetClientByIdQueryHandler(IClientRepository repository)
    : IQueryHandler<GetClientByIdQuery, ClientResponse>
{
    public async Task<Result<ClientResponse>> Handle(
        GetClientByIdQuery request,
        CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(request.ClientId, cancellationToken);

        if (client is null)
            return Result.Failure<ClientResponse>(new NotFoundError(nameof(Client), request.ClientId));

        return Result.Success(new ClientResponse(client.Id, client.FullName, client.Email));
    }
}