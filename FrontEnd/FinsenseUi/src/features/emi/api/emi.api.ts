import { api, extractData } from '../../../lib/axios'
import type { ApiResponse } from '../../../types/common.types'
import type { EMIRequest, EMIResponseTyped } from '../../../types/emi.types'

export const emiApi = {
  checkAffordability: async (data: EMIRequest): Promise<EMIResponseTyped> => {
    const res = await api.post<ApiResponse<EMIResponseTyped>>(
      '/emi/check', data
    )
    return extractData(res)
  }
}
