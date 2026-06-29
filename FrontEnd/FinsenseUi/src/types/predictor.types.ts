import type { Status } from './common.types'

export interface PredictorRequest {
  sessionId: string
  currentDate: string // ISO string
}

export interface PredictorResponse {
  projectedBalance: number
  totalSpentSoFar: number
  projectedFutureSpend: number
  dailyAverage: number
  daysRemaining: number
  monthlyIncome: number
  status: string | null
  aiCommentary: string | null
  topSpendingCategory: string | null
}

export interface PredictorResponseTyped extends Omit<PredictorResponse, 'status'> {
  status: Status | null
}
