import { api } from '../../../lib/axios'

export const chatApi = {
  deleteChatHistory: async (sessionId: string): Promise<void> => {
    await api.delete(`/chat/history/${sessionId}`)
  }
}
