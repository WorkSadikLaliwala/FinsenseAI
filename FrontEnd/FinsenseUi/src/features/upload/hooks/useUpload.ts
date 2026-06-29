import { useMutation, useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { uploadApi } from '../api/upload.api'
import { useSessionStore } from '../../../store/session.store'
import { useAuthStore } from '../../../store/auth.store'
import { queryClient, QueryKeys } from '../../../lib/queryClient'

export function useUpload() {
  const navigate = useNavigate()
  const setSession = useSessionStore(state => state.setSession)

  return useMutation({
    mutationFn: (file: File) => uploadApi.uploadCsv(file),

    onSuccess: (response) => {
      // Save session details to Zustand store
      setSession(response.sessionId, response.isGuest)

      // Pre-populate the analytics cache using key pattern
      queryClient.setQueryData(
        QueryKeys.analytics(response.sessionId),
        response.summary
      )

      // Invalidate history cache if authenticated
      queryClient.invalidateQueries({
        queryKey: QueryKeys.uploadHistory()
      })

      navigate('/dashboard')
    }
  })
}

export function useUploadHistory() {
  const isAuthenticated = useAuthStore(state => state.isAuthenticated)

  return useQuery({
    queryKey: QueryKeys.uploadHistory(),
    queryFn: uploadApi.getUploadHistory,
    enabled: isAuthenticated
  })
}
