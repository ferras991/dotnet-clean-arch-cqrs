using Application.Abstractions;

namespace Application.UseCases.Clients.RegisterClient;

public sealed record RegisterClientCommand(
    string FirstName,
    string LastName,
    string Email) : ICommand<Guid>;