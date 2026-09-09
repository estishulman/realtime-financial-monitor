import { useMemo, useState } from 'react'
import type { TransactionStatus } from '../models/transaction'
import { HubConnectionState } from '@microsoft/signalr'
import { useTransactionRealtime } from '../contexts/useTransactionRealtime'

type Filter = 'All' | TransactionStatus

export function MonitorPage() {
  const [filter, setFilter] = useState<Filter>('All')
  const { transactions, connectionState, error } = useTransactionRealtime()
  const isConnected = connectionState === HubConnectionState.Connected

  const visibleTransactions = useMemo(() => filter === 'All' ? transactions : transactions.filter((transaction) => transaction.status === filter), [filter, transactions])
  return <section className="page-grid">
    <div className="page-heading"><div className="heading-row"><div><p className="eyebrow">REAL-TIME OPERATIONS</p><h2>Transaction monitor</h2></div><span className={`connection-pill ${isConnected ? 'online' : 'offline'}`}><span />{isConnected ? 'Live connection' : 'Offline'}</span></div><p>Every accepted transaction appears here as the backend broadcasts it.</p></div>
    {error && <p className="error-message" role="alert">{error}</p>}
    <div className="toolbar"><span>{visibleTransactions.length} visible transactions</span><div className="filter-group" aria-label="Filter transactions">{(['All', 'Pending', 'Completed', 'Failed'] as Filter[]).map((option) => <button className={filter === option ? 'filter active' : 'filter'} key={option} onClick={() => setFilter(option)} type="button">{option}</button>)}</div></div>
    <div className="transaction-table-wrap"><table><thead><tr><th>Transaction</th><th>Amount</th><th>Currency</th><th>Status</th><th>Timestamp</th></tr></thead><tbody>{visibleTransactions.map((transaction) => <tr key={transaction.transactionId}><td className="transaction-id">{transaction.transactionId}</td><td>{transaction.amount.toFixed(2)}</td><td>{transaction.currency}</td><td><span className={`status-badge ${transaction.status.toLowerCase()}`}>{transaction.status}</span></td><td>{new Date(transaction.timestamp).toLocaleString()}</td></tr>)}</tbody></table>{visibleTransactions.length === 0 && <div className="empty-state">No transactions match this filter.</div>}</div>
  </section>
}