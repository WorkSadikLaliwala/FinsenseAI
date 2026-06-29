import { useQuery } from '@tanstack/react-query'
import { predictorApi } from '../api/predictor.api'
import { useSessionStore } from '../../../store/session.store'
import { QueryKeys } from '../../../lib/queryClient'

export function usePredictor() {
  const sessionId = useSessionStore(state => state.sessionId)

  return useQuery({
    queryKey: QueryKeys.predictor(sessionId ?? ''),
    queryFn: () =>
      predictorApi.predictMonthEnd({
        sessionId: sessionId!,
        currentDate: new Date().toISOString() // ISO timestamp required
      }),
    enabled: !!sessionId
  })
}
