import { useMutation } from '@tanstack/react-query'
import { emiApi } from '../api/emi.api'
import type { EMIFormData } from '../schemas/emi.schema'
import { useSessionStore } from '../../../store/session.store'

export function useEMICheck() {
  const { sessionId, isGuest, incrementEmiCheckCount } = useSessionStore()

  return useMutation({
    mutationFn: (data: EMIFormData) =>
      emiApi.checkAffordability({
        ...data,
        sessionId: sessionId ?? undefined
      }),
    onSuccess: () => {
      if (isGuest) {
        incrementEmiCheckCount()
      }
    }
  })
}
