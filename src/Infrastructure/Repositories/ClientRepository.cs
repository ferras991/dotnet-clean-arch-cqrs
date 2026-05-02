using Dapper;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories;

internal sealed class ClientRepository(AppDbContext context) : IClientRepository
{
    public async Task InsertAsync(Client client, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        const string sql = """
                           INSERT INTO clients (id, first_name, last_name, email, created_at)
                           VALUES (@Id, @FirstName, @LastName, @Email, @CreatedAt)
                           """;

        var connection = context.Database.GetDbConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(sql, client, transaction, cancellationToken: cancellationToken));
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT
                               id         AS Id,
                               first_name AS FirstName,
                               last_name  AS LastName,
                               email      AS Email,
                               created_at AS CreatedAt
                           FROM clients
                           WHERE id = @Id
                           """;

        var connection = context.Database.GetDbConnection();

        return await connection.QuerySingleOrDefaultAsync<Client>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT CAST(CASE WHEN EXISTS (
                               SELECT 1 FROM clients WHERE email = @Email
                           ) THEN 1 ELSE 0 END AS BIT)
                           """;

        var connection = context.Database.GetDbConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
    }
}