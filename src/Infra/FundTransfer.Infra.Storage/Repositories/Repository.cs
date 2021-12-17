using FundTransfer.Domain.Repositories;
using FundTransfer.Infra.Storage.Context;

namespace FundTransfer.Infra.Storage.Repositories
{
    public abstract class Repository : ITransaction
    {
        private readonly DataContext dataContext;

        protected Repository(DataContext context) =>
            dataContext = context;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken) =>
            await dataContext.Database.BeginTransactionAsync(cancellationToken);

        public async Task CommitAsync(CancellationToken cancellationToken) =>
            await dataContext.Database.CommitTransactionAsync(cancellationToken);

        public async Task RollBackAsync(CancellationToken cancellationToken) =>
            await dataContext.Database.RollbackTransactionAsync(cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
            await dataContext.SaveChangesAsync(cancellationToken);
    }
}