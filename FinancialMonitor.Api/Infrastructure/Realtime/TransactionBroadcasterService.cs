using System.Threading.Channels;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FinancialMonitor.Api.Infrastructure.Realtime;

public sealed class TransactionBroadcasterService(
    Channel<Transaction> transactionChannel,
    ITransactionProcessor transactionProcessor,
    ITransactionRepository transactionRepository,
    IHubContext<TransactionHub> hubContext,
    ILogger<TransactionBroadcasterService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var transaction in transactionChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await hubContext.Clients.All.SendAsync(
                    "ReceiveTransaction",
                    transaction,
                    stoppingToken);
                logger.LogInformation(
                    "Broadcasted transaction {TransactionId} with status {Status}.",
                    transaction.TransactionId,
                    transaction.Status);

                var processedTransaction = await transactionProcessor.ProcessAsync(transaction, stoppingToken);
                await transactionRepository.UpdateAsync(processedTransaction, stoppingToken);

                await hubContext.Clients.All.SendAsync(
                    "ReceiveTransaction",
                    processedTransaction,
                    stoppingToken);
                logger.LogInformation(
                    "Broadcasted transaction {TransactionId} with final status {Status}.",
                    processedTransaction.TransactionId,
                    processedTransaction.Status);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to broadcast transaction {TransactionId}.", transaction.TransactionId);
            }
        }
    }
}