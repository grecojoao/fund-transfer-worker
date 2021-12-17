using FlyGon.CQRS.Commands;
using FlyGon.CQRS.Handlers.Contracts;
using FundTransfer.Domain.Bus.Publishers;
using FundTransfer.Domain.Commands;
using FundTransfer.Domain.Dtos;
using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Enums;
using FundTransfer.Domain.Repositories;
using FundTransfer.Domain.Services;
using FundTransfer.Domain.Services.Contracts;
using Newtonsoft.Json;
using Serilog;

namespace FundTransfer.Domain.Handlers
{
    public sealed class FundTransferHandler :
        IGenericHandler<TransferCommand, CommandResult>,
        IGenericHandler<PendingTransferCommand, CommandResult>
    {
        private const string INVALID_ACCOUNT_NUMBER = "Invalid account number";
        private const string INSUFFICIENT_FUNDS = "Insufficient funds";
        private static readonly string ERROR_OCCURRED = "An error has occurred";
        private readonly IAccountService _accountService;
        private readonly ITransferRepository _transferRepository;
        private readonly IBusPublisher _busPublisher;

        public FundTransferHandler(IAccountService accountService, ITransferRepository transferRepository, IBusPublisher busPublisher)
        {
            _accountService = accountService;
            _transferRepository = transferRepository;
            _busPublisher = busPublisher;
        }

        public async Task<CommandResult> Handle(TransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = new Transfer(
                request.TransactionId, request.TransferStatus, request.AccountOrigin, request.AccountDestination, request.Value);
            transfer.Change(TransferStatusEnum.Processing);
            try
            {
                Guid transactionId = transfer.TransactionId;
                await _transferRepository.AddAsync(transfer, cancellationToken);
                Log.Debug($"[{transactionId}] - {JsonConvert.SerializeObject(transfer)}");
                if (await AccountNotFound(transfer, transactionId, cancellationToken))
                {
                    transfer.Change(TransferStatusEnum.Error, INVALID_ACCOUNT_NUMBER);
                    await _transferRepository.AddAsync(transfer, cancellationToken);
                    return new CommandResult(true, transfer.Message, transfer);
                }

                var balance = await _accountService.CheckBalanceAsync(transfer.AccountOrigin, cancellationToken, transactionId);
                if (HasNoBalance(transfer, balance))
                {
                    transfer.Change(TransferStatusEnum.Error, INSUFFICIENT_FUNDS);
                    await _transferRepository.AddAsync(transfer, cancellationToken);
                    return new CommandResult(true, transfer.Message, transfer);
                }

                var isTransferred = await _accountService.TransferAsync(
                    transfer.AccountOrigin, transfer.AccountDestination, (float)transfer.Value, out var errorOccurred, cancellationToken,
                    transactionId);
                if (OnlyOneTransaction(isTransferred, errorOccurred))
                {
                    var pendingTransfer = new PendingTransfer(
                        transfer.TransactionId, transfer.AccountDestination, (float)transfer.Value, TransferTypeEnum.Credit);
                    await _busPublisher.SendAsync(JsonConvert.SerializeObject(pendingTransfer));
                    Log.Warning($"[{transactionId}] - {JsonConvert.SerializeObject(pendingTransfer)}");
                    return new CommandResult(true, transfer.Message, transfer);
                }
                else if (NoTransaction(isTransferred, errorOccurred))
                {
                    transfer.Change(TransferStatusEnum.InQueue);
                    await _transferRepository.AddAsync(transfer, cancellationToken);
                    return new CommandResult(false, ERROR_OCCURRED, transfer);
                }

                transfer.Change(TransferStatusEnum.Confirmed);
                await _transferRepository.AddAsync(transfer, cancellationToken);
                return new CommandResult(true, transfer.Message, transfer);
            }
            catch (Exception)
            {
                transfer.Change(TransferStatusEnum.InQueue);
                await _transferRepository.AddAsync(transfer, cancellationToken);
                return new CommandResult(false, ERROR_OCCURRED, transfer);
            }
        }

        private async Task<bool> AccountNotFound(Transfer transfer, Guid transactionId, CancellationToken cancellationToken) =>
            !await _accountService.CheckAccountAsync(transfer.AccountOrigin, cancellationToken, transactionId) ||
            !await _accountService.CheckAccountAsync(transfer.AccountDestination, cancellationToken, transactionId);

        private static bool HasNoBalance(Transfer transfer, float? balance) =>
            transfer.Value > balance;

        private static bool NoTransaction(bool isTransferred, bool errorOccurred) =>
            !isTransferred && !errorOccurred;

        private static bool OnlyOneTransaction(bool isTransferred, bool errorOccurred) =>
            !isTransferred && errorOccurred;

        public async Task<CommandResult> Handle(PendingTransferCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var transactionId = request.TransactionId;
                var isTransferred = await _accountService.Reversal(
                    request.Account, request.Value, request.TransferType, cancellationToken, transactionId);
                if (!isTransferred)
                {
                    Log.Debug($"[{transactionId}] - {JsonConvert.SerializeObject(request)} -> isTransferred: {isTransferred}");
                    return new CommandResult(false, ERROR_OCCURRED);
                }

                var transfer = await _transferRepository.GetAsync(request.TransactionId, cancellationToken);
                transfer.Change(TransferStatusEnum.Confirmed);
                await _transferRepository.AddAsync(transfer, cancellationToken);
                Log.Debug($"[{transactionId}] - {JsonConvert.SerializeObject(request)} -> isTransferred: {isTransferred}");
                Log.Debug($"[{transactionId}] - {JsonConvert.SerializeObject(transfer)}");
                return new CommandResult(true, transfer.Message, transfer);
            }
            catch (Exception)
            {
                return new CommandResult(false, ERROR_OCCURRED);
            }
        }
    }
}