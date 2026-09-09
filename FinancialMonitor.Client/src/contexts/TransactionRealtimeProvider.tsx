import { useEffect, useState, type ReactNode } from 'react'
import { HubConnectionState } from '@microsoft/signalr'
import type { Transaction } from '../models/transaction'
import { getTransactions } from '../services/transactionApi'
import { createTransactionHub } from '../services/transactionHub'
import { TransactionRealtimeContext } from './transactionRealtimeContext'
import type { TransactionRealtimeContextValue } from './transactionRealtimeContext'

export function TransactionRealtimeProvider({ children }: { children: ReactNode }) {
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [connectionState, setConnectionState] = useState(HubConnectionState.Disconnected)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    let frameId: number | undefined
    const pendingTransactions = new Map<string, Transaction>()

    function flushTransactions() {
      frameId = undefined
      const incomingTransactions = [...pendingTransactions.values()]
      pendingTransactions.clear()
      if (incomingTransactions.length === 0) return

      setTransactions((current) => {
        const updated = new Map(current.map((transaction) => [transaction.transactionId, transaction]))
        for (const transaction of incomingTransactions) updated.set(transaction.transactionId, transaction)
        const incomingIds = new Set(incomingTransactions.map((transaction) => transaction.transactionId))
        return [...incomingTransactions, ...[...updated.values()].filter((transaction) => !incomingIds.has(transaction.transactionId))]
      })
    }

    const connection = createTransactionHub((transaction) => {
      pendingTransactions.set(transaction.transactionId, transaction)
      if (frameId === undefined) frameId = requestAnimationFrame(flushTransactions)
    }, setConnectionState)

    async function startRealtime() {
      try {
        await connection.start()
        if (active) setConnectionState(HubConnectionState.Connected)
        const stored = await getTransactions()
        if (active) {
          setTransactions((current) => {
            const merged = new Map(stored.map((transaction) => [transaction.transactionId, transaction]))
            current.forEach((transaction) => merged.set(transaction.transactionId, transaction))
            return [...merged.values()].sort((left, right) => right.timestamp.localeCompare(left.timestamp))
          })
        }
      } catch {
        if (active) setError('Live connection unavailable. Start the backend and refresh.')
      }
    }

    void startRealtime()
    return () => {
      active = false
      if (frameId !== undefined) cancelAnimationFrame(frameId)
      pendingTransactions.clear()
      void connection.stop()
    }
  }, [])

  const value: TransactionRealtimeContextValue = { transactions, connectionState, error }
  return <TransactionRealtimeContext.Provider value={value}>{children}</TransactionRealtimeContext.Provider>
}