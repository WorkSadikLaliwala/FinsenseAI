import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '../store/auth.store'

const HUB_URL = import.meta.env.VITE_HUB_URL ?? 'http://localhost:5000/hubs/chat'

let connection: signalR.HubConnection | null = null

export function getSignalRConnection(): signalR.HubConnection {
  if (connection) return connection

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      transport: signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
      accessTokenFactory: () =>
        useAuthStore.getState().token ?? ''
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  return connection
}

export async function startConnection(): Promise<void> {
  const conn = getSignalRConnection()
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    console.log(`[SignalR] Attempting to connect to Hub: ${HUB_URL}`)
    try {
      await conn.start()
      console.log('[SignalR] Connected successfully. Connection State:', conn.state)
    } catch (err) {
      console.error('[SignalR] Failed to start connection:', err)
      throw err;
    }
  } else {
    console.log('[SignalR] Connection state is already:', conn.state)
  }
}

export async function stopConnection(): Promise<void> {
  if (connection) {
    console.log('[SignalR] Stopping connection...')
    await connection.stop()
    connection = null
    console.log('[SignalR] Connection stopped.')
  }
}
