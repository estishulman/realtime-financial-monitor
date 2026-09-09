import { useEffect, useMemo, useState } from 'react'
import type { Transaction, TransactionStatus } from '../models/transaction'
import { getTransactions } from '../services/transactionApi'
import { createTransactionHub } from '../services/transactionHub'

type Filter = 'All' | TransactionStatus

export function MonitorPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [filter, setFilter] = useState<Filter>('All')
  const [isConnected, setIsConnected] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    let frameId: number | undefined
    const pendingTransactions = new Map<string, Transaction>()

    function flushTransactions() {
      frameId = undefined
      if (pendingTransactions.size === 0) return

      const incomingTransactions = [...pendingTransactions.values()]
      pendingTransactions.clear()
      setTransactions((current) => {
        const updated = new Map(current.map((transaction) => [transaction.transactionId, transaction]))
        for (const transaction of incomingTransactions) {
          const previous = updated.get(transaction.transactionId)
          console.info(
            previous ? '[Monitor] Updating transaction row' : '[Monitor] Adding transaction row',
            transaction.transactionId,
            previous ? `${previous.status} -> ${transaction.status}` : transaction.status,
          )
          updated.set(transaction.transactionId, transaction)
        }

        const incomingIds = new Set(incomingTransactions.map((transaction) => transaction.transactionId))
        const existing = [...updated.values()].filter((transaction) => !incomingIds.has(transaction.transactionId))
        return [...incomingTransactions, ...existing]
      })
    }

    const hub = createTransactionHub((transaction) => {
      pendingTransactions.set(transaction.transactionId, transaction)
      if (frameId === undefined) frameId = requestAnimationFrame(flushTransactions)
    })
    async function connect() {
      try {
        const stored = await getTransactions()
        if (active) setTransactions([...stored].reverse())
        await hub.start()
        if (active) setIsConnected(true)
      } catch { if (active) setError('Live connection unavailable. Start the backend and refresh.') }
    }
    void connect()
    return () => {
      active = false
      if (frameId !== undefined) cancelAnimationFrame(frameId)
      pendingTransactions.clear()
      void hub.stop()
    }
  }, [])

  const visibleTransactions = useMemo(() => filter === 'All' ? transactions : transactions.filter((transaction) => transaction.status === filter), [filter, transactions])
  return <section className="page-grid">
    <div className="page-heading"><div className="heading-row"><div><p className="eyebrow">REAL-TIME OPERATIONS</p><h2>Transaction monitor</h2></div><span className={`connection-pill ${isConnected ? 'online' : 'offline'}`}><span />{isConnected ? 'Live connection' : 'Offline'}</span></div><p>Every accepted transaction appears here as the backend broadcasts it.</p></div>
    {error && <p className="error-message" role="alert">{error}</p>}
    <div className="toolbar"><span>{visibleTransactions.length} visible transactions</span><div className="filter-group" aria-label="Filter transactions">{(['All', 'Pending', 'Completed', 'Failed'] as Filter[]).map((option) => <button className={filter === option ? 'filter active' : 'filter'} key={option} onClick={() => setFilter(option)} type="button">{option}</button>)}</div></div>
    <div className="transaction-table-wrap"><table><thead><tr><th>Transaction</th><th>Amount</th><th>Currency</th><th>Status</th><th>Timestamp</th></tr></thead><tbody>{visibleTransactions.map((transaction) => <tr key={transaction.transactionId}><td className="transaction-id">{transaction.transactionId}</td><td>{transaction.amount.toFixed(2)}</td><td>{transaction.currency}</td><td><span className={`status-badge ${transaction.status.toLowerCase()}`}>{transaction.status}</span></td><td>{new Date(transaction.timestamp).toLocaleString()}</td></tr>)}</tbody></table>{visibleTransactions.length === 0 && <div className="empty-state">No transactions match this filter.</div>}</div>
  </section>
}