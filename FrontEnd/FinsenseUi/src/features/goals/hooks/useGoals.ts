import { useQuery, useMutation } from '@tanstack/react-query'
import { goalsApi } from '../api/goals.api'
import { queryClient, QueryKeys } from '../../../lib/queryClient'
import { useAuthStore } from '../../../store/auth.store'
import { useSessionStore } from '../../../store/session.store'
import type { GoalItem, GoalRequest, UpdateGoalRequest } from '../../../types/goal.types'
import type { GoalStatus } from '../../../types/common.types'

export function useGoals() {
  const user = useAuthStore(state => state.user)

  return useQuery({
    queryKey: QueryKeys.goals(user?.email ?? ''),
    queryFn: async () => {
      const dtos = await goalsApi.getGoals()
      // Map statuses strictly
      const items: GoalItem[] = dtos.map(dto => ({
        ...dto,
        status: (dto.status ?? 'On Track') as GoalStatus
      }))
      return items
    },
    enabled: !!user
  })
}

export function useCreateGoal() {
  const user = useAuthStore(state => state.user)
  const { sessionId } = useSessionStore()

  return useMutation({
    mutationFn: (data: Omit<GoalRequest, 'sessionId'>) =>
      goalsApi.createGoal({
        ...data,
        sessionId: sessionId ?? '00000000-0000-0000-0000-000000000000'
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QueryKeys.goals(user?.email ?? '')
      })
    }
  })
}

export function useDeleteGoal() {
  const user = useAuthStore(state => state.user)

  return useMutation({
    mutationFn: (id: number) => goalsApi.deleteGoal(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QueryKeys.goals(user?.email ?? '')
      })
    }
  })
}

export function useUpdateGoal() {
  const user = useAuthStore(state => state.user)

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateGoalRequest }) =>
      goalsApi.updateGoal(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QueryKeys.goals(user?.email ?? '')
      })
    }
  })
}
