using System.Text.Json.Serialization;

namespace FinancialMonitor.Api.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionStatus
{
    Pending,
    Completed,
    Failed
}