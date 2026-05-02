using Application.Abstractions;
using Domain;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;

namespace Application.UseCases.Clients.RegisterClient;

internal sealed class RegisterClientCommandHandler(IClientRepository repository)
    : ICommandHandler<RegisterClientCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        RegisterClientCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await repository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
            return Result.Failure<Guid>(new ConflictError("Client.EmailInUse", "Email is already in use."));

        var client = Client.Create(request.FirstName, request.LastName, request.Email);

        await repository.InsertAsync(client, cancellationToken);

        return Result.Success(client.Id);
    }
}