namespace FundTransfer.Domain.Bus.Publishers
{
    public interface IBusPublisher
    {
        Task SendAsync(string message);
    }
}