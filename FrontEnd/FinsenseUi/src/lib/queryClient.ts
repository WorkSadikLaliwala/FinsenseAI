import { QueryClient } from '@tanstack/react-query'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,        // 5 minutes
      gcTime: 10 * 60 * 1000,          // 10 minutes cache
      retry: 1,                        // retry once on failure
      refetchOnWindowFocus: false      // don't refetch when tab regains focus
    },
    mutations: {
      retry: 0                         // never retry mutations
    }
  }
})

export const QueryKeys = {
  analytics: (sessionId: string) => ['analytics', sessionId] as const,
  goals: (userId: string)        => ['goals', userId] as const,
  uploadHistory: ()              => ['upload-history'] as const,
  predictor: (sessionId: string) => ['predictor', sessionId] as const,
  chatHistory: (sessionId: string) => ['chat', sessionId] as const
}
