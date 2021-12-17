using FundTransfer.Domain.Entities;

namespace FundTransfer.Domain.Repositories
{
    public interface ITransferRepository
    {
        Task AddAsync(Transfer transfer, CancellationToken cancellationToken);
        Task<Transfer> GetAsync(Guid transactionId, CancellationToken cancellationToken);
    }
}