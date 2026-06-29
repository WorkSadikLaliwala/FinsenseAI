import { api, extractData } from '../../../lib/axios'
import type { ApiResponse } from '../../../types/common.types'
import type {
  RegisterRequest,
  LoginRequest,
  AuthResponse
} from '../../../types/auth.types'

export const authApi = {
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const res = await api.post<ApiResponse<AuthResponse>>(
      '/auth/register', data
    )
    return extractData(res)
  },

  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const res = await api.post<ApiResponse<AuthResponse>>(
      '/auth/login', data
    )
    return extractData(res)
  }
}
