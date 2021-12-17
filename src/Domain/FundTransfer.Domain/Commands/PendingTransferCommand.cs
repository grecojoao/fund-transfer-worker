using FundTransfer.Domain.Services;

namespace FundTransfer.Domain.Commands
{
    public class PendingTransferCommand
    {
        public Guid TransactionId { get; set; }
        public string Account { get; set; }
        public float Value { get; set; }
        public TransferTypeEnum TransferType { get; set; }

        public PendingTransferCommand() { }

        public PendingTransferCommand(Guid transactionId, string account, float value, TransferTypeEnum transferType)
        {
            TransactionId = transactionId;
            Account = account;
            Value = value;
            TransferType = transferType;
        }
    }
}