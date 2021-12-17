using FundTransfer.Domain.AcessoAccount.Enums;

namespace FundTransfer.Domain.AcessoAccount.Dtos
{
    internal class Transfer
    {
        public string AccountNumber { get; set; }
        public float Value { get; set; }
        public TypeEnum Type { get; set; }

        public Transfer() { }

        public Transfer(string accountNumber, float value, TypeEnum type)
        {
            AccountNumber = accountNumber;
            Value = value;
            Type = type;
        }
    }
}