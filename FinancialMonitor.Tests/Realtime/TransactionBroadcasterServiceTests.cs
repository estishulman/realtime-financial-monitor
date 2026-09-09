using System.Threading.Channels;
using System.Diagnostics;
using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Infrastructure.Realtime;
using FinancialMonitor.Api.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinancialMonitor.Tests.Realtime;

public sealed class TransactionBroadcasterServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ReadsChannelAndBroadcastsTransaction()
    {
        var channel = Channel.CreateUnbounded<Transaction>();
        var transaction = CreateTransaction();
        var processedTransaction = transaction with { Status = TransactionStatus.Failed };
        var processor = new Mock<ITransactionProcessor>();
        processor.Setup(item => item.ProcessAsync(transaction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(processedTransaction);
        var repository = new Mock<ITransactionRepository>();
        var clientProxy = new Mock<IClientProxy>();
        var hubClients = new Mock<IHubClients>();
        hubClients.Setup(clients => clients.All).Returns(clientProxy.Object);
        var hubContext = new Mock<IHubContext<TransactionHub>>();
        hubContext.SetupGet(context => context.Clients).Returns(hubClients.Object);
        using var service = new TransactionBroadcasterService(
            channel,
            processor.Object,
            repository.Object,
            hubContext.Object,
            NullLogger<TransactionBroadcasterService>.Instance);
        using var cancellationSource = new CancellationTokenSource();

        var execution = service.StartAsync(cancellationSource.Token);
        await channel.Writer.WriteAsync(transaction);

        var stopwatch = Stopwatch.StartNew();
        while (clientProxy.Invocations.Count == 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(1))
        {
            await Task.Yield();
        }

        while (clientProxy.Invocations.Count < 2 && stopwatch.Elapsed < TimeSpan.FromSeconds(1))
        {
            await Task.Yield();
        }

        Assert.Equal(2, clientProxy.Invocations.Count);
        var initialBroadcast = clientProxy.Invocations[0];
        Assert.Equal("ReceiveTransaction", initialBroadcast.Arguments[0]);
        Assert.Equal(transaction, Assert.IsType<object[]>(initialBroadcast.Arguments[1])[0]);

        var finalBroadcast = clientProxy.Invocations[1];
        Assert.Equal("ReceiveTransaction", finalBroadcast.Arguments[0]);
        Assert.Equal(processedTransaction, Assert.IsType<object[]>(finalBroadcast.Arguments[1])[0]);
        repository.Verify(item => item.UpdateAsync(processedTransaction, It.IsAny<CancellationToken>()), Times.Once);

        await cancellationSource.CancelAsync();
        channel.Writer.TryComplete();
        await service.StopAsync(CancellationToken.None);
        await execution;
    }

    private static Transaction CreateTransaction() => new(
        Guid.NewGuid().ToString(),
        100,
        "USD",
        TransactionStatus.Completed,
        DateTime.UtcNow);
}