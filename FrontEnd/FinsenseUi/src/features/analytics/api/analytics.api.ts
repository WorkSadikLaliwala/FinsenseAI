import { api, extractData } from '../../../lib/axios'
import type { ApiResponse } from '../../../types/common.types'
import type { SpendingSummaryDto } from '../../../types/analytics.types'

export const analyticsApi = {
  getSummary: async (sessionId: string): Promise<SpendingSummaryDto> => {
    const res = await api.get<ApiResponse<SpendingSummaryDto>>(
      `/analytics/${sessionId}`
    )
    return extractData(res)
  }
}
