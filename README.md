# dotnet-clean-arch-cqrs

A production-ready template for building .NET 10 REST APIs using **Clean Architecture** and a **custom CQRS implementation** — no MediatR dependency.

---

## Why no MediatR?

MediatR moved to a paid model for commercial use. More importantly, the core pattern it implements — a dispatcher that resolves handlers and runs a behavior pipeline — is straightforward to own yourself:

- No black-box dependency
- Full control over pipeline behavior
- Easier to debug and extend
- Zero licensing concerns

---

## Architecture

```
src/
  Domain/          → Entities, value objects, repository interfaces, Result, errors
  Application/     → CQRS abstractions, dispatcher, pipeline behaviors, use cases, validators
  Infrastructure/  → EF Core (migrations + connection), Dapper (all queries), repository implementations
  Api/             → Controllers, route conventions, Result → HTTP mapping

tests/
  Unit/            → Handlers and validators in isolation (NSubstitute)
  Integration/     → Repository layer against a real DB (Testcontainers)
  E2E/             → Full HTTP stack against a real DB (WebApplicationFactory + Testcontainers)
```

### Dependency graph

```
Api → Application → Domain
Api → Infrastructure → Application → Domain
```

Api references Domain transitively — no direct `<ProjectReference>` to Domain in `Api.csproj`.

---

## CQRS Design

Commands and queries are plain records that implement marker interfaces:

```csharp
public sealed record RegisterClientCommand(
    string FirstName,
    string LastName,
    string Email) : ICommand<Guid>;

public sealed record GetClientByIdQuery(Guid ClientId) : IQuery<ClientResponse>;
```

Handlers are resolved and executed through a `Dispatcher`:

```csharp
var result = await dispatcher.Send(command, cancellationToken);
```

The dispatcher chains registered `IPipelineBehavior<,>` implementations before invoking the handler — same mental model as MediatR's pipeline, no external dependency.

### Pipeline (in order)

```
ValidationBehavior → LoggingBehavior → Handler
```

Adding a new behavior is one class + one `services.AddScoped` line in `DependencyInjection.cs`.

---

## Result Pattern

All use cases return `Result` or `Result<T>` — no exceptions for expected business failures:

```csharp
// handler
var emailExists = await repository.ExistsByEmailAsync(request.Email, cancellationToken);
if (emailExists)
    return Result.Failure<Guid>(new ConflictError("Client.EmailInUse", "Email is already in use."));

return Result.Success(client.Id);

// controller
var result = await dispatcher.Send(command, cancellationToken);
return result.ToActionResult();
```

`ToActionResult()` maps `DomainError.ErrorType` → HTTP status code in one place. Controllers stay thin.

### Error types

| Type | HTTP |
|---|---|
| `NotFoundError` | 404 |
| `ConflictError` | 409 |
| `ValidationError` | 422 |

Validation errors include field-level detail:

```json
{
  "title": "Validation.Failed",
  "detail": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "Email": ["'Email' is not a valid email address."],
    "FirstName": ["'First Name' must not be empty."]
  }
}
```

---

## Database Strategy

**EF Core** handles migrations and connection lifecycle only.  
**Dapper** handles all queries and writes via `context.Database.GetDbConnection()`.

This gives full SQL control with zero ORM overhead, while keeping EF Core's migration tooling.

Transactions are managed explicitly in the use case handler when multiple tables are involved:

```csharp
await using var transaction = await transactionFactory.BeginAsync(cancellationToken);
await clientRepository.InsertAsync(client, cancellationToken, transaction);
await cardRepository.InsertAsync(card, cancellationToken, transaction);
transaction.Commit();
```

Single-table operations need no transaction wrapper — each Dapper call is already atomic.

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for Testcontainers in tests)
- SQL Server (or use the connection string from `appsettings.Development.json`)

### Run

```bash
# apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# run the API
dotnet run --project src/Api
```

Swagger is available at `https://localhost:{port}/swagger` in Development.

### Test

```bash
# unit tests (no Docker required)
dotnet test tests/Unit

# integration tests (requires Docker)
dotnet test tests/Integration

# E2E tests (requires Docker)
dotnet test tests/E2E
```

---

## Stack

| Concern | Library |
|---|---|
| Framework | .NET 10 |
| ORM / Migrations | EF Core 10 |
| Queries / Writes | Dapper |
| Validation | FluentValidation |
| API docs | Swashbuckle |
| Unit tests | xUnit, NSubstitute, FluentAssertions |
| Integration / E2E | Testcontainers, WebApplicationFactory |
