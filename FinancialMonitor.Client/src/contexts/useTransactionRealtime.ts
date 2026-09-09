import { useContext } from 'react'
import { TransactionRealtimeContext } from './transactionRealtimeContext'

export function useTransactionRealtime() {
  const context = useContext(TransactionRealtimeContext)
  if (!context) throw new Error('useTransactionRealtime must be used inside TransactionRealtimeProvider')
  return context
}