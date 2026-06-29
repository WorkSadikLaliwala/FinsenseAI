export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T
  error: string | null
  statusCode: number
}

export type Status = 'Safe' | 'Risky' | 'Danger'
export type GoalStatus = 'On Track' | 'At Risk' | 'Not Feasible'
export type TransactionType = 'Debit' | 'Credit'
export type ChatRole = 'user' | 'assistant'

export type Category =
  | 'Food'
  | 'Transport'
  | 'EMI'
  | 'Entertainment'
  | 'Utilities'
  | 'Shopping'
  | 'Health'
  | 'Education'
  | 'Salary'
  | 'Refund'
  | 'Others'
