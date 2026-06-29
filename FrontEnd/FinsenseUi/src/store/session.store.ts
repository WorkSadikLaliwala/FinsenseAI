import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface SessionStore {
  sessionId: string | null
  isGuest: boolean
  guestMessageCount: number
  emiCheckCount: number
  setSession: (sessionId: string, isGuest: boolean) => void
  clearSession: () => void
  incrementGuestMessageCount: () => void
  incrementEmiCheckCount: () => void
}

export const useSessionStore = create<SessionStore>()(
  persist(
    (set) => ({
      sessionId: null,
      isGuest: true,
      guestMessageCount: 0,
      emiCheckCount: 0,

      setSession: (sessionId, isGuest) =>
        set({ sessionId, isGuest }),

      clearSession: () =>
        set({ sessionId: null, isGuest: true, guestMessageCount: 0, emiCheckCount: 0 }),

      incrementGuestMessageCount: () =>
        set(state => ({
          guestMessageCount: state.guestMessageCount + 1
        })),

      incrementEmiCheckCount: () =>
        set(state => ({
          emiCheckCount: state.emiCheckCount + 1
        }))
    }),
    {
      name: 'finsense-session'
    }
  )
)
