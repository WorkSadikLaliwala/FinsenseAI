import { api, extractData } from '../../../lib/axios'
import type { ApiResponse } from '../../../types/common.types'
import type {
  GoalRequest,
  GoalResponseTyped,
  GoalDto,
  UpdateGoalRequest
} from '../../../types/goal.types'

export const goalsApi = {
  createGoal: async (data: GoalRequest): Promise<GoalResponseTyped> => {
    const res = await api.post<ApiResponse<GoalResponseTyped>>(
      '/goals', data
    )
    return extractData(res)
  },

  getGoals: async (): Promise<GoalDto[]> => {
    const res = await api.get<ApiResponse<GoalDto[]>>('/goals')
    return extractData(res)
  },

  deleteGoal: async (id: number): Promise<void> => {
    await api.delete(`/goals/${id}`)
  },

  updateGoal: async (id: number, data: UpdateGoalRequest): Promise<GoalDto> => {
    const res = await api.put<ApiResponse<GoalDto>>(`/goals/${id}`, data)
    return extractData(res)
  }
}
