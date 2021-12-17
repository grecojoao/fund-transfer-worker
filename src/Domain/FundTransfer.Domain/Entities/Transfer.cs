using FundTransfer.Domain.Enums;

namespace FundTransfer.Domain.Entities
{
    public class Transfer
    {
        public Guid TransactionId { get; set; }
        public TransferStatusEnum TransferStatus { get; set; }
        public string Message { get; set; }
        public string AccountOrigin { get; set; }
        public string AccountDestination { get; set; }
        public float? Value { get; set; }

        public Transfer() { }

        public Transfer(
            Guid transactionId, TransferStatusEnum transferStatus, string accountOrigin,
            string accountDestination, float? value, string message = "")
        {
            TransactionId = transactionId;
            TransferStatus = transferStatus;
            AccountOrigin = accountOrigin;
            AccountDestination = accountDestination;
            Value = value;
            Message = message;
        }

        public void Change(TransferStatusEnum transferStatus) =>
            TransferStatus = transferStatus;

        public void Change(TransferStatusEnum transferStatus, string message)
        {
            TransferStatus = transferStatus;
            Message = message;
        }
    }
}