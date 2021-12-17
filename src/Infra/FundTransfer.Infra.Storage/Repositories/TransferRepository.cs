using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Repositories;
using FundTransfer.Infra.Storage.Context;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Infra.Storage.Repositories
{
    public class TransferRepository : Repository, ITransferRepository
    {
        private readonly DataContext dataContext;

        public TransferRepository(DataContext context) : base(context) =>
            dataContext = context;

        public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken) =>
            await dataContext.Transfers.AddAsync(transfer, cancellationToken);

        public async Task<Transfer> GetAsync(Guid transactionId, CancellationToken cancellationToken) =>
            await dataContext.Transfers.FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);

        public Task UpdateStatus(Transfer transfer)
        {
            dataContext.Entry(transfer).State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}