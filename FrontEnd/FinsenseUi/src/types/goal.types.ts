import type { GoalStatus } from './common.types'

export interface GoalRequest {
  goalName: string
  targetAmount: number
  targetDate: string
  currentSavings: number
  sessionId: string // required by Swagger DTO
}

export interface GoalResponse {
  status: string | null
  amountRemaining: number
  monthsRemaining: number
  requiredMonthlySaving: number
  currentMonthlySurplus: number
  shortfall: number
  reason: string | null
  tip: string | null
}

export interface GoalResponseTyped extends Omit<GoalResponse, 'status'> {
  status: GoalStatus | null
}

export interface GoalDto {
  id: number
  goalName: string | null
  targetAmount: number
  targetDate: string
  currentSavings: number
  status: string | null
}

export interface GoalItem extends Omit<GoalDto, 'status'> {
  status: GoalStatus | null
}

export interface UpdateGoalRequest {
  goalName?: string | null
  targetAmount?: number | null
  targetDate?: string | null
  currentSavings?: number | null
}
