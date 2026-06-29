import type { ChatRole } from './common.types'

export interface ChatHistoryItemDto {
  role: string | null
  content: string | null
}

export interface ChatRequestDto {
  message: string | null
  sessionId: string
  chatHistory: ChatHistoryItemDto[] | null
  guestMessageCount: number
}

export interface ChatResponseDto {
  reply: string | null
  isGuest: boolean
  messagesRemaining: number
  limitReached: boolean
}

// Client side representation
export interface ChatHistoryItem {
  role: ChatRole
  content: string
}

export interface ChatMessage {
  id: string           // local UUID for React key
  role: ChatRole
  content: string
  timestamp: Date
  isStreaming?: boolean
}

export interface ChatState {
  messages: ChatMessage[]
  isStreaming: boolean
  guestMessageCount: number
  messagesRemaining: number
  limitReached: boolean
}
