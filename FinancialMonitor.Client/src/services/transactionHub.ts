import { HubConnectionBuilder, HttpTransportType, HubConnectionState, LogLevel, type HubConnection } from '@microsoft/signalr'
import type { Transaction } from '../models/transaction'

const hubUrl = `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5058'}/hubs/transactions`

export function createTransactionHub(
  onTransaction: (transaction: Transaction) => void,
  onStateChanged: (state: HubConnectionState) => void,
): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(hubUrl, {
      transport: HttpTransportType.WebSockets,
      skipNegotiation: true,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  connection.on('ReceiveTransaction', (transaction: Transaction) => {
    console.info('[SignalR] ReceiveTransaction', {
      transactionId: transaction.transactionId,
      status: transaction.status,
      timestamp: transaction.timestamp,
    })
    onTransaction(transaction)
  })

  connection.onreconnecting((error) => {
    onStateChanged(HubConnectionState.Reconnecting)
    console.warn('[SignalR] Reconnecting', error)
  })

  connection.onreconnected((connectionId) => {
    onStateChanged(HubConnectionState.Connected)
    console.info('[SignalR] Reconnected', connectionId)
  })

  connection.onclose((error) => {
    onStateChanged(HubConnectionState.Disconnected)
    console.warn('[SignalR] Connection closed', error)
  })
  return connection
}