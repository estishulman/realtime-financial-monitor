using System.Threading.Channels;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FinancialMonitor.Tests.Api;

public sealed class TransactionsControllerTests
{
    [Fact]
    public async Task Create_ValidTransaction_SavesAndEnqueuesTransaction()
    {
        var transaction = new Transaction(
            Guid.NewGuid().ToString(),
            100,
            "USD",
            TransactionStatus.Pending,
            DateTime.UtcNow);
        var transactionService = new Mock<ITransactionService>();
        transactionService
            .Setup(service => service.CreateAsync(transaction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TransactionOperationResult.Success(transaction));
        var repository = new Mock<ITransactionRepository>();
        var channel = Channel.CreateUnbounded<Transaction>();
        var controller = new TransactionsController(
            transactionService.Object,
            repository.Object,
            channel);

        var result = await controller.Create(transaction, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(transaction, created.Value);
        Assert.True(channel.Reader.TryRead(out var queued));
        Assert.Equal(transaction, queued);
    }
}