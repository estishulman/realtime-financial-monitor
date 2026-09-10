using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinancialMonitor.Tests.Storage;

public sealed class CachedTransactionRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_CacheMiss_LoadsFromRepositoryAndPopulatesCache()
    {
        var transactions = new[] { CreateTransaction() };
        var repository = new Mock<ITransactionRepository>();
        repository.Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);
        var cache = new Mock<ITransactionCache>();
        cache.Setup(item => item.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Transaction>?)null);
        var sut = CreateSut(repository, cache);

        var result = await sut.GetAllAsync();

        Assert.Equal(transactions, result);
        repository.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(item => item.SetAsync(transactions, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_CacheHit_DoesNotReadRepository()
    {
        var transactions = new[] { CreateTransaction() };
        var repository = new Mock<ITransactionRepository>();
        var cache = new Mock<ITransactionCache>();
        cache.Setup(item => item.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);
        var sut = CreateSut(repository, cache);

        var result = await sut.GetAllAsync();

        Assert.Equal(transactions, result);
        repository.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_InvalidatesCacheAfterPersistence()
    {
        var transaction = CreateTransaction();
        var repository = new Mock<ITransactionRepository>();
        repository.Setup(item => item.AddAsync(transaction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        var cache = new Mock<ITransactionCache>();
        var sut = CreateSut(repository, cache);

        await sut.AddAsync(transaction);

        cache.Verify(item => item.RemoveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CachedTransactionRepository CreateSut(
        Mock<ITransactionRepository> repository,
        Mock<ITransactionCache> cache) =>
        new(
            repository.Object,
            cache.Object,
            NullLogger<CachedTransactionRepository>.Instance);

    private static Transaction CreateTransaction() => new(
        Guid.NewGuid().ToString(),
        100,
        "USD",
        TransactionStatus.Pending,
        DateTime.UtcNow);
}