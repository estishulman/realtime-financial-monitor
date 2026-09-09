using System.Collections.Concurrent;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Domain.Entities;

namespace FinancialMonitor.Api.Infrastructure.Persistence;

public sealed class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<string, Transaction> transactions = new();

    public Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        transactions[transaction.TransactionId] = transaction;
        return Task.FromResult(transaction);
    }

    public Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        transactions[transaction.TransactionId] = transaction;
        return Task.FromResult(transaction);
    }

    public Task<IReadOnlyCollection<Transaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Transaction> snapshot = transactions.Values.ToArray();
        return Task.FromResult(snapshot);
    }
}