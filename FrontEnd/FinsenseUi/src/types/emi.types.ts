import type { Status } from './common.types'

export interface EMIRequest {
  salary: number
  monthlyExpenses: number
  loanAmount: number
  tenureMonths: number
  interestRatePerAnnum: number
  sessionId?: string | null
}

export interface EMIResponse {
  emiAmount: number
  status: string | null
  reason: string | null
  recommendation: string | null
  safeEMILimit: number
  disposableIncome: number
  emiAsPercentOfSalary: number
}

export interface EMIResponseTyped extends Omit<EMIResponse, 'status'> {
  status: Status | null
}
