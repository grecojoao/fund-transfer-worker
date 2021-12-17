namespace FundTransfer.Domain.Bus.Consumers
{
    public interface IBusConsumer
    {
        Task ReceiveAsync();
    }
}