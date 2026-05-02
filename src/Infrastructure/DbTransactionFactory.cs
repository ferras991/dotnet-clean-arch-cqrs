using Application.Abstractions;
using Infrastructure.Persistence;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure;

internal sealed class DbTransactionFactory(AppDbContext context) : IDbTransactionFactory
{
    public async Task<IDbTransaction> BeginAsync(CancellationToken cancellationToken)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return transaction.GetDbTransaction();
    }
}