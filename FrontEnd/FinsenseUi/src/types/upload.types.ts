import type { SpendingSummary } from './analytics.types'
import type { Category, TransactionType } from './common.types'


export interface TransactionDto {
  id: number
  date: string
  description: string | null
  amount: number
  type: string | null
  category: string | null
}

export interface Transaction {
  id: number
  date: string
  description: string
  amount: number
  type: TransactionType
  category: Category
}

export interface UploadResponse {
  sessionId: string
  isGuest: boolean
  transactions: TransactionDto[] | null
  summary: SpendingSummary
}

export interface UploadHistoryItem {
  sessionId: string
  userId: string | null
  fileName: string | null
  uploadedAt: string
  transactionCount: number
}
