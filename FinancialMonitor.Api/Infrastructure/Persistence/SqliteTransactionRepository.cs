using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialMonitor.Api.Infrastructure.Persistence;

public sealed class EfTransactionRepository(
    IDbContextFactory<FinancialMonitorDbContext> contextFactory) : ITransactionRepository
{
    public async Task<Transaction> AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Transactions.Update(transaction);
        await context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Transactions
            .AsNoTracking()
            .OrderByDescending(transaction => transaction.Timestamp)
            .ToArrayAsync(cancellationToken);
    }
}