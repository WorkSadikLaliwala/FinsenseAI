import { api, extractData } from '../../../lib/axios'
import type { ApiResponse } from '../../../types/common.types'
import type {
  PredictorRequest,
  PredictorResponseTyped
} from '../../../types/predictor.types'

export const predictorApi = {
  predictMonthEnd: async (
    data: PredictorRequest
  ): Promise<PredictorResponseTyped> => {
    const res = await api.post<ApiResponse<PredictorResponseTyped>>(
      '/predictor/month-end', data
    )
    return extractData(res)
  }
}
