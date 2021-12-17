using FundTransfer.Domain.Enums;

namespace FundTransfer.Domain.Commands
{
    public class TransferCommand
    {
        public Guid TransactionId { get; set; }
        public TransferStatusEnum TransferStatus { get; set; }
        public string AccountOrigin { get; set; }
        public string AccountDestination { get; set; }
        public float? Value { get; set; }

        public TransferCommand() { }

        public TransferCommand(Guid transactionId, TransferStatusEnum transferStatus, string accountOrigin,
            string accountDestination, float? value)
        {
            TransactionId = transactionId;
            TransferStatus = transferStatus;
            AccountOrigin = accountOrigin;
            AccountDestination = accountDestination;
            Value = value;
        }
    }
}