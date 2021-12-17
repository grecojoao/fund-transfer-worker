using FundTransfer.Infra.RabbitMq.Consumers;
using Raven.Client.Documents;
using Serilog;

namespace FundTransfer.Worker
{
    public class Worker : BackgroundService
    {
        private readonly TransferConsumer _transferConsumer;
        private readonly PendingTransferConsumer _pendingTransferConsumer;

        public Worker(TransferConsumer transferConsumer, PendingTransferConsumer pendingTransferConsumer, DocumentStore store)
        {
            _transferConsumer = transferConsumer;
            _pendingTransferConsumer = pendingTransferConsumer;
            store.Initialize();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("[Worker] - Initiated");
            try
            {
                await _transferConsumer.ReceiveAsync();
                await _pendingTransferConsumer.ReceiveAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "");
                throw;
            }            
        }
    }
}