import { createContext } from 'react'
import { HubConnectionState } from '@microsoft/signalr'
import type { Transaction } from '../models/transaction'

export type TransactionRealtimeContextValue = {
  transactions: Transaction[]
  connectionState: HubConnectionState
  error: string
}

export const TransactionRealtimeContext = createContext<TransactionRealtimeContextValue | undefined>(undefined)