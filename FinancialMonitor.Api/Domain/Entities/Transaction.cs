namespace FinancialMonitor.Api.Domain.Entities;

public sealed record Transaction(
    string TransactionId,
    decimal Amount,
    string Currency,
    TransactionStatus Status,
    DateTime Timestamp);