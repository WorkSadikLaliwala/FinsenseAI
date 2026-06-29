import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { authApi } from '../api/auth.api'
import { useAuthStore } from '../../../store/auth.store'
import type { LoginFormData } from '../schemas/auth.schema'

export function useLogin() {
  const navigate = useNavigate()
  const setAuth = useAuthStore(state => state.setAuth)

  return useMutation({
    mutationFn: (data: LoginFormData) => authApi.login(data),

    onSuccess: (response) => {
      if (response.token) {
        setAuth(
          {
            fullName: response.fullName ?? 'User',
            email: response.email ?? '',
            expiresAt: response.expiresAt
          },
          response.token,
          response.refreshToken
        )
        navigate('/dashboard')
      }
    },

    onError: (error: Error) => {
      console.error('Login failed:', error.message)
    }
  })
}
