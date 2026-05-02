using Domain.Entities;
using System.Data;

namespace Domain.Repositories;

public interface IClientRepository
{
    Task InsertAsync(Client client, CancellationToken cancellationToken, IDbTransaction? transaction = null);
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
}