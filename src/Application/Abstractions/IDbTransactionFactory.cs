using System.Data;

namespace Application.Abstractions;

public interface IDbTransactionFactory
{
    Task<IDbTransaction> BeginAsync(CancellationToken cancellationToken);
}