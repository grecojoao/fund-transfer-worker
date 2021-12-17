using FundTransfer.Domain.AcessoAccount.Dtos;
using FundTransfer.Domain.AcessoAccount.Enums;
using FundTransfer.Domain.Services;
using FundTransfer.Domain.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using System.Text;

namespace FundTransfer.Domain.AcessoAccount
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _configuration;
        private readonly string _urlDefault = "https://acessoaccount.herokuapp.com";

        public AccountService(IConfiguration configuration) =>
            _configuration = configuration;

        private string GetUrl()
        {
            string url = null;
            if (_configuration["WebServices:AccountApi"] != null)
                url = _configuration["WebServices:AccountApi"];
            if (url == null)
                url = _urlDefault;
            return url;
        }

        public async Task<bool> CheckAccountAsync(string account, CancellationToken cancellationToken, Guid transactionId = default)
        {
            try
            {
                var url = GetUrl();
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url + $"/api/Account/{account}", cancellationToken);
                var statusCode = response.StatusCode;
                Log.Debug($"[{transactionId}] - Check Account: {account} -> StatusCode: {statusCode}");
                if (statusCode == HttpStatusCode.OK)
                    return true;
                else if (statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                    return false;
                else if (statusCode == HttpStatusCode.InternalServerError)
                    throw new Exception("Application offline");
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<float?> CheckBalanceAsync(string account, CancellationToken cancellationToken, Guid transactionId = default)
        {
            try
            {
                var url = GetUrl();
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url + $"/api/Account/{account}", cancellationToken);
                var statusCode = response.StatusCode;
                Log.Debug($"[{transactionId}] - Balance Account: {account} -> StatusCode: {statusCode}");
                if (statusCode == HttpStatusCode.OK)
                {
                    var accountResponse = JsonConvert.DeserializeObject<Account>(await response.Content.ReadAsStringAsync(cancellationToken));
                    return accountResponse.Balance;
                }
                else
                    throw new Exception("Application offline");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> TransferAsync(
            string accountOrigin, string accountDestination, float value, out bool errorOccurred, CancellationToken cancellationToken, 
            Guid transactionId = default)
        {
            try
            {
                errorOccurred = false;
                var transferDebit = new Transfer(accountOrigin, value, TypeEnum.Debit);
                var isTransferred = TransferAsync(transferDebit, cancellationToken).Result;
                if (!isTransferred)
                {
                    Log.Debug($"[{transactionId}] - Transfer - AccountOrigin: {accountOrigin}, AccountDestination: {accountDestination}, Value: {value} -> [false, false]");
                    return Task.FromResult(false);
                }

                var transferCredit = new Transfer(accountDestination, value, TypeEnum.Credit);
                isTransferred = TransferAsync(transferCredit, cancellationToken).Result;
                if (!isTransferred)
                {
                    var count = 0;
                    while (!isTransferred || count <= 2)
                    {
                        isTransferred = TransferAsync(transferCredit, cancellationToken).Result;
                        count++;
                    }
                    errorOccurred = !isTransferred;
                    if (errorOccurred)
                        Log.Warning($"[{transactionId}] - Transfer - AccountOrigin: {accountOrigin}, AccountDestination: {accountDestination}, Value: {value} -> [true, {isTransferred}]");
                    else
                        Log.Debug($"[{transactionId}] - Transfer - AccountOrigin: {accountOrigin}, AccountDestination: {accountDestination}, Value: {value} -> [true, {isTransferred}]");
                    return errorOccurred ? Task.FromResult(false) : Task.FromResult(true);
                }
                Log.Debug($"[{transactionId}] - Transfer - AccountOrigin: {accountOrigin}, AccountDestination: {accountDestination}, Value: {value} -> [true, {isTransferred}]");
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> TransferAsync(Transfer transfer, CancellationToken cancellationToken)
        {
            var url = GetUrl();
            using var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(transfer), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url + "/api/Account", content, cancellationToken);
            var statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                return true;
            else
                return false;
        }

        public async Task<bool> Reversal(
            string account, float value, TransferTypeEnum transferType, CancellationToken cancellationToken, Guid transactionId = default)
        {
            var transfer = transferType == TransferTypeEnum.Credit ?
                new Transfer(account, value, TypeEnum.Credit) :
                new Transfer(account, value, TypeEnum.Debit);

            var isTransferred = await TransferAsync(transfer, cancellationToken);
            Log.Debug($"[{transactionId}] - Transfer reversal - Account: {account}, Value: {value} -> {isTransferred}");
            return isTransferred;
        }
    }
}