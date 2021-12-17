using FundTransfer.Domain.Services;

namespace FundTransfer.Domain.Dtos
{
    public class PendingTransfer
    {
        public Guid TransactionId { get; set; }
        public string Account { get; set; }
        public float Value { get; set; }
        public TransferTypeEnum TransferType { get; set; }

        public PendingTransfer() { }

        public PendingTransfer(Guid transactionId, string account, float value, TransferTypeEnum transferType)
        {
            TransactionId = transactionId;
            Account = account;
            Value = value;
            TransferType = transferType;
        }
    }
}