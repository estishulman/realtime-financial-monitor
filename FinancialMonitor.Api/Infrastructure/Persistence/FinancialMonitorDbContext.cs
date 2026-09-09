using FinancialMonitor.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialMonitor.Api.Infrastructure.Persistence;

public sealed class FinancialMonitorDbContext(DbContextOptions<FinancialMonitorDbContext> options)
    : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(transaction => transaction.TransactionId);
            entity.Property(transaction => transaction.Amount).HasPrecision(18, 2);
            entity.Property(transaction => transaction.Currency).HasMaxLength(3).IsRequired();
            entity.Property(transaction => transaction.Status).HasConversion<string>().IsRequired();
            entity.Property(transaction => transaction.Timestamp).IsRequired();
        });
    }
}