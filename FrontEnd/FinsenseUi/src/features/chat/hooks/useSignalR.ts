import { useEffect, useRef, useState } from 'react'
import { getSignalRConnection, startConnection } from '../../../lib/signalr'
import { useSessionStore } from '../../../store/session.store'
import type { ChatMessage, ChatHistoryItem } from '../../../types/chat.types'
import * as signalR from '@microsoft/signalr'

interface UseSignalROptions {
  sessionId: string
  isGuest: boolean
  guestMessageCount: number
  onLimitReached: () => void
}

export function useSignalR({
  sessionId,
  isGuest,
  guestMessageCount,
  onLimitReached
}: UseSignalROptions) {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [isStreaming, setIsStreaming] = useState(false)
  const streamingMessageId = useRef<string | null>(null)
  const incrementGuestMessageCount = useSessionStore(state => state.incrementGuestMessageCount)

  useEffect(() => {
    let active = true

    const initConnection = async () => {
      try {
        const conn = getSignalRConnection()
        await startConnection()

        if (!active) return

        // Receive streaming chunks
        conn.on('ReceiveChunk', (chunk: string) => {
          if (!active) return
          setMessages(prev =>
            prev.map(msg =>
              msg.id === streamingMessageId.current
                ? { ...msg, content: msg.content + chunk }
                : msg
            )
          )
        })

        // Stream complete
        conn.on('ReceiveComplete', () => {
          if (!active) return
          setIsStreaming(false)
          streamingMessageId.current = null
          if (isGuest) {
            incrementGuestMessageCount()
          }
        })

        // Error from hub
        conn.on('ReceiveError', (error: string) => {
          if (!active) return
          setIsStreaming(false)
          console.error('SignalR Hub error:', error)
        })

      } catch (err) {
        console.error('SignalR init failed:', err)
      }
    }

    initConnection()

    return () => {
      active = false
      const conn = getSignalRConnection()
      conn.off('ReceiveChunk')
      conn.off('ReceiveComplete')
      conn.off('ReceiveError')
    }
  }, [isGuest, incrementGuestMessageCount])

  const sendMessage = async (text: string) => {
    // Guest Limit check (max 3 messages)
    if (isGuest && guestMessageCount >= 3) {
      onLimitReached()
      return
    }

    try {
      const conn = getSignalRConnection()
      if (conn.state !== signalR.HubConnectionState.Connected) {
        await startConnection()
      }

      // Add user message to UI immediately
      const userMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: 'user',
        content: text,
        timestamp: new Date()
      }

      // Add empty assistant message that will be filled by streaming
      const assistantId = crypto.randomUUID()
      const assistantMsg: ChatMessage = {
        id: assistantId,
        role: 'assistant',
        content: '',
        timestamp: new Date(),
        isStreaming: true
      }

      // Save references
      streamingMessageId.current = assistantId
      setIsStreaming(true)
      setMessages(prev => [...prev, userMsg, assistantMsg])

      // Map local history to API shape
      const chatHistory: ChatHistoryItem[] = messages.map(m => ({
        role: m.role,
        content: m.content
      }))

      // Send to SignalR Hub matching exact DTO signatures
      await conn.invoke(
        'SendMessage',
        sessionId,
        text,
        chatHistory,
        guestMessageCount
      )

    } catch (err) {
      console.error('Error invoking SendMessage:', err)
      setIsStreaming(false)
      streamingMessageId.current = null
    }
  }

  const clearLocalMessages = () => {
    setMessages([])
    setIsStreaming(false)
    streamingMessageId.current = null
  }

  return { messages, isStreaming, sendMessage, clearLocalMessages }
}
