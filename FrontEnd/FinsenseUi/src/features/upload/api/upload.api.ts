import { api, extractData } from '../../../lib/axios'
import type { ApiResponse } from '../../../types/common.types'
import type { UploadResponse, UploadHistoryItem } from '../../../types/upload.types'

export const uploadApi = {
  uploadCsv: async (file: File): Promise<UploadResponse> => {
    const formData = new FormData()
    formData.append('file', file)

    const res = await api.post<ApiResponse<UploadResponse>>(
      '/upload',
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' }
      }
    )
    return extractData(res)
  },

  getUploadHistory: async (): Promise<UploadHistoryItem[]> => {
    const res = await api.get<ApiResponse<UploadHistoryItem[]>>(
      '/upload/history'
    )
    return extractData(res)
  }
}
