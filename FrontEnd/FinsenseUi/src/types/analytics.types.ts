import type { Category } from './common.types'
import type { Transaction, TransactionDto } from './upload.types'

export interface CategoryBreakdownDto {
  category: string | null
  amount: number
  percentage: number
}

export interface SpendingSummaryDto {
  totalIncome: number
  totalRefunds: number
  totalSpending: number
  netSavings: number
  byCategory: CategoryBreakdownDto[] | null
  topTransactions: TransactionDto[] | null
}

// Client-side strict types
export interface CategoryBreakdown {
  category: Category
  amount: number
  percentage: number
}

export interface SpendingSummary {
  totalIncome: number
  totalRefunds: number
  totalSpending: number
  netSavings: number
  byCategory: CategoryBreakdown[]
  topTransactions: Transaction[]
}
