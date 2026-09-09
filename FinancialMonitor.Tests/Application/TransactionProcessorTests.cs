using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Tests.Application;

public sealed class TransactionProcessorTests
{
    [Theory]
    [InlineData(100, TransactionStatus.Completed)]
    [InlineData(10_000, TransactionStatus.Completed)]
    [InlineData(10_001, TransactionStatus.Failed)]
    public async Task ProcessAsync_ResolvesTransactionStatus(decimal amount, TransactionStatus expectedStatus)
    {
        var processor = new TransactionProcessor();
        var transaction = new Transaction(
            Guid.NewGuid().ToString(),
            amount,
            "USD",
            TransactionStatus.Pending,
            DateTime.UtcNow);

        var result = await processor.ProcessAsync(transaction);

        Assert.Equal(expectedStatus, result.Status);
    }
}