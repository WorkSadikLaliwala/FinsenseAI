import { useQuery } from '@tanstack/react-query'
import { analyticsApi } from '../api/analytics.api'
import { useSessionStore } from '../../../store/session.store'
import { QueryKeys } from '../../../lib/queryClient'
import type { SpendingSummary, CategoryBreakdown } from '../../../types/analytics.types'
import type { Transaction } from '../../../types/upload.types'
import type { Category, TransactionType } from '../../../types/common.types'

// Map the API DTO schemas to our strictly-typed frontend domain models
function mapSummary(dto: any): SpendingSummary {
  const byCategory: CategoryBreakdown[] = (dto.byCategory ?? []).map((c: any) => ({
    category: (c.category ?? 'Others') as Category,
    amount: c.amount ?? 0,
    percentage: c.percentage ?? 0
  }))

  const topTransactions: Transaction[] = (dto.topTransactions ?? []).map((t: any) => ({
    id: t.id,
    date: t.date,
    description: t.description ?? '',
    amount: t.amount ?? 0,
    type: (t.type === 'Credit' ? 'Credit' : 'Debit') as TransactionType,
    category: (t.category ?? 'Others') as Category
  }))

  return {
    totalIncome: dto.totalIncome ?? 0,
    totalSpending: dto.totalSpending ?? 0,
    netSavings: dto.netSavings ?? 0,
    totalRefunds: dto.totalRefunds ?? 0,
    byCategory,
    topTransactions
  }
}

export function useAnalytics() {
  const sessionId = useSessionStore(state => state.sessionId)

  return useQuery({
    queryKey: QueryKeys.analytics(sessionId ?? ''),
    queryFn: async () => {
      const dto = await analyticsApi.getSummary(sessionId!)
      return mapSummary(dto)
    },
    enabled: !!sessionId
  })
}
