import axios from 'axios'
import { useAuthStore } from '../store/auth.store'
import type { ApiResponse } from '../types/common.types'

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api/v1'

export const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000   // 30 seconds
})

// Request interceptor -- attach JWT token automatically
api.interceptors.request.use(
  (config) => {
    console.log(`[API Request] ${config.method?.toUpperCase()} ${config.url}`, config.data ? { payload: config.data } : '')
    const token = useAuthStore.getState().token
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    console.error('[API Request Error]', error)
    return Promise.reject(error)
  }
)

// Response interceptor -- handle 401 globally
api.interceptors.response.use(
  (response) => {
    console.log(`[API Response] ${response.status} ${response.config.url}`)
    return response
  },
  (error) => {
    console.error(`[API Response Error] ${error.response?.status ?? 'Network Error'} ${error.config?.url}:`, error.response?.data ?? error.message)
    if (error.response?.status === 401) {
      console.warn('[API Auth] 401 Unauthorized received. Clearing local session and redirecting to login.')
      useAuthStore.getState().clearAuth()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// Helper -- extract data from ApiResponse wrapper
export function extractData<T>(response: { data: ApiResponse<T> }): T {
  if (!response.data.success) {
    throw new Error(response.data.error ?? response.data.message ?? 'Something went wrong')
  }
  return response.data.data
}
