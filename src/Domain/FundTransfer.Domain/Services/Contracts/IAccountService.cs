namespace FundTransfer.Domain.Services.Contracts
{
    public interface IAccountService
    {
        Task<bool> CheckAccountAsync(string account, CancellationToken cancellationToken, Guid transactionId = default);
        Task<float?> CheckBalanceAsync(string account, CancellationToken cancellationToken, Guid transactionId = default);
        Task<bool> TransferAsync(
            string accountOrigin, string accountDestination, float value, out bool errorOccurred, CancellationToken cancellationToken, 
            Guid transactionId = default);
        Task<bool> Reversal(
            string account, float value, TransferTypeEnum transferType, CancellationToken cancellationToken, Guid transactionId = default);
    }
}