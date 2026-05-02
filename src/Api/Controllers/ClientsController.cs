using Api.Extensions;
using Application.Abstractions;
using Application.UseCases.Clients.GetClientById;
using Application.UseCases.Clients.RegisterClient;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class ClientsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetClientByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterClientCommand command,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(command, cancellationToken);

        return result.IsFailure
            ? result.ToActionResult()
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, null);
    }
}