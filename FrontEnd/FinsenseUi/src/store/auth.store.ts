import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthUser, AuthState } from '../types/auth.types'

interface AuthStore extends AuthState {
  setAuth: (user: AuthUser, token: string, refreshToken?: string | null) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (user, token, refreshToken = null) =>
        set({ user, token, refreshToken, isAuthenticated: true }),

      clearAuth: () =>
        set({ user: null, token: null, refreshToken: null, isAuthenticated: false })
    }),
    {
      name: 'finsense-auth' // localStorage key
    }
  )
)
