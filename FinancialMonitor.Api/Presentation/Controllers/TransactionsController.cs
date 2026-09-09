using System.Threading.Channels;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FinancialMonitor.Api.Presentation.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(
    ITransactionService transactionService,
    ITransactionRepository transactionRepository,
    Channel<Transaction> transactionChannel) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Transaction>> Create(
        [FromBody] Transaction transaction,
        CancellationToken cancellationToken)
    {
        var result = await transactionService.CreateAsync(transaction, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        var savedTransaction = result.Transaction!;
        if (!transactionChannel.Writer.TryWrite(savedTransaction))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "Transaction broadcast queue is unavailable.");
        }

        return Created($"/api/transactions/{savedTransaction.TransactionId}", savedTransaction);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<Transaction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<Transaction>>> GetAll(
        CancellationToken cancellationToken)
    {
        var transactions = await transactionRepository.GetAllAsync(cancellationToken);
        return Ok(transactions);
    }
}