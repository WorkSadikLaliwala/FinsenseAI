import { useUploadHistory } from '../hooks/useUpload'
import { useSessionStore } from '../../../store/session.store'
import { useNavigate } from 'react-router-dom'
import { FileSpreadsheet, Play, AlertCircle } from 'lucide-react'
import { formatDate } from '../../../utils/date'

export function UploadHistory() {
  const navigate = useNavigate()
  const { data: history, isLoading, error } = useUploadHistory()
  const { sessionId: activeSessionId, setSession } = useSessionStore()

  const handleSelectSession = (sessionId: string) => {
    setSession(sessionId, false)
    navigate('/dashboard')
  }

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map(n => (
          <div key={n} className="h-16 bg-slate-900/50 border border-slate-800 rounded-xl animate-pulse"></div>
        ))}
      </div>
    )
  }

  if (error) {
    return (
      <div className="p-4 bg-rose-500/10 border border-rose-500/20 text-rose-400 rounded-xl text-xs flex items-center gap-2">
        <AlertCircle className="w-4.5 h-4.5" />
        <span>Failed to load upload history.</span>
      </div>
    )
  }

  if (!history || history.length === 0) {
    return (
      <div className="text-center py-8 border border-slate-800 rounded-2xl bg-slate-900/10">
        <p className="text-slate-500 text-xs">No upload history found.</p>
        <p className="text-slate-600 text-[10px] mt-1">Upload a CSV to get started.</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {history.map((item) => {
        const isActive = item.sessionId === activeSessionId

        return (
          <div
            key={item.sessionId}
            className={`p-4 rounded-xl border flex items-center justify-between transition-all duration-200
              ${isActive
                ? 'bg-emerald-500/5 border-emerald-500/30'
                : 'bg-slate-900/30 border-slate-850 hover:bg-slate-900/50'
              }
            `}
          >
            <div className="flex items-center gap-3 min-w-0">
              <div className={`w-9 h-9 rounded-lg flex items-center justify-center border flex-shrink-0
                ${isActive
                  ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                  : 'bg-slate-800 border-slate-700 text-slate-400'
                }
              `}>
                <FileSpreadsheet className="w-4.5 h-4.5" />
              </div>
              <div className="min-w-0">
                <h5 className="text-white font-medium text-xs truncate">
                  {item.fileName || 'Unnamed Statement'}
                </h5>
                <div className="flex items-center gap-2.5 mt-1 text-[10px] text-slate-500">
                  <span>{formatDate(item.uploadedAt)}</span>
                  <span className="w-1 h-1 rounded-full bg-slate-700"></span>
                  <span>{item.transactionCount} transactions</span>
                </div>
              </div>
            </div>

            <button
              onClick={() => handleSelectSession(item.sessionId)}
              disabled={isActive}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold flex items-center gap-1.5 transition-all
                ${isActive
                  ? 'bg-emerald-600/10 text-emerald-400 border border-emerald-500/20'
                  : 'bg-slate-800 hover:bg-slate-700 text-white border border-slate-700'
                }
              `}
            >
              <Play className="w-3 h-3 fill-current" />
              <span>{isActive ? 'Active' : 'Load'}</span>
            </button>
          </div>
        )
      })}
    </div>
  )
}
