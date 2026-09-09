import type { Transaction } from '../models/transaction'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5058'

export async function getTransactions(): Promise<Transaction[]> {
  const response = await fetch(`${apiBaseUrl}/api/transactions`)
  if (!response.ok) throw new Error('Could not load transactions.')
  return response.json() as Promise<Transaction[]>
}

export async function createTransaction(transaction: Transaction): Promise<Transaction> {
  const response = await fetch(`${apiBaseUrl}/api/transactions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(transaction),
  })
  if (!response.ok) throw new Error('Could not create transaction.')
  return response.json() as Promise<Transaction>
}

export async function runLoadTest(): Promise<{ accepted: number; rejected: number }> {
  const transactions = Array.from({ length: 100 }, (_, index): Transaction => ({
    transactionId: crypto.randomUUID(),
    amount: index % 2 === 0 ? 1500 : 10001,
    currency: 'USD',
    status: 'Pending',
    timestamp: new Date().toISOString(),
  }))

  const responses = await Promise.all(
    transactions.map((transaction) => createTransaction(transaction).then(
      () => true,
      () => false,
    )),
  )

  return {
    accepted: responses.filter(Boolean).length,
    rejected: responses.filter((accepted) => !accepted).length,
  }
}