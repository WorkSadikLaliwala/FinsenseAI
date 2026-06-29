import { usePredictor } from '../hooks/usePredictor'
import { useSessionStore } from '../../../store/session.store'
import { formatRupee } from '../../../utils/currency'
import { Link } from 'react-router-dom'
import {
  Sparkles,
  TrendingUp,
  AlertTriangle,
  ArrowRight,
  Calendar,
  AlertCircle,
  ShieldCheck,
  ShieldAlert
} from 'lucide-react'

export function PredictorPage() {
  const { sessionId } = useSessionStore()
  const { data: result, isLoading, error } = usePredictor()

  // 1. Loading State
  if (isLoading) {
    return (
      <div className="p-8 max-w-5xl mx-auto space-y-6">
        <div className="h-8 w-48 bg-slate-900 border border-slate-800 rounded-lg animate-pulse mb-6"></div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="md:col-span-2 h-80 bg-slate-900 border border-slate-800 rounded-xl animate-pulse"></div>
          <div className="h-80 bg-slate-900 border border-slate-800 rounded-xl animate-pulse"></div>
        </div>
      </div>
    )
  }

  // 2. No CSV Session
  if (!sessionId) {
    return (
      <div className="min-h-[calc(100vh-8rem)] flex items-center justify-center px-4">
        <div className="text-center max-w-md space-y-6">
          <div className="w-16 h-16 bg-slate-900 border border-slate-800 rounded-2xl flex items-center justify-center mx-auto text-emerald-400">
            <TrendingUp className="w-8 h-8" />
          </div>
          <div>
            <h2 className="text-xl font-bold text-white mb-2">Predictor Awaiting Statement</h2>
            <p className="text-slate-400 text-sm leading-relaxed">
              Upload your bank statement CSV file to enable the month-end cash flow forecaster and AI commentary.
            </p>
          </div>
          <Link
            to="/upload"
            className="inline-flex items-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl font-semibold text-sm shadow-lg shadow-emerald-600/15 transition-all"
          >
            <span>Upload Statement</span>
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    )
  }

  // 3. Error
  if (error) {
    return (
      <div className="p-8 max-w-md mx-auto text-center space-y-4 mt-20">
        <AlertCircle className="w-12 h-12 text-rose-500 mx-auto" />
        <h3 className="text-white font-bold text-lg">Forecast Evaluation Error</h3>
        <p className="text-slate-400 text-sm">{error.message || 'Error occurred while loading predictor data.'}</p>
        <Link to="/upload" className="inline-flex items-center gap-2 px-4 py-2 bg-slate-800 hover:bg-slate-700 text-white border border-slate-700 rounded-xl text-sm font-semibold">
          <span>Upload CSV</span>
        </Link>
      </div>
    )
  }

  // Determine status color configurations
  const getStatusConfig = (status: string | null) => {
    switch (status) {
      case 'Safe':
        return {
          bg: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20',
          text: 'Safe Outlook',
          icon: ShieldCheck
        }
      case 'Risky':
        return {
          bg: 'bg-amber-500/10 text-amber-400 border border-amber-500/20',
          text: 'Risky Outflow',
          icon: ShieldAlert
        }
      case 'Danger':
        return {
          bg: 'bg-rose-500/10 text-rose-400 border border-rose-500/20',
          text: 'Danger Deficit',
          icon: ShieldAlert
        }
      default:
        return {
          bg: 'bg-slate-800 text-slate-400 border border-slate-700',
          text: 'Neutral Status',
          icon: AlertCircle
        }
    }
  }

  const statusConfig = result ? getStatusConfig(result.status) : null

  return (
    <div className="py-10 px-6 max-w-5xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-white">Month-End Predictor</h1>
          <p className="text-slate-400 text-xs mt-0.5">AI-driven cash flow projection and deficit alarms</p>
        </div>

        {statusConfig && (
          <div className={`px-4 py-2 border rounded-xl flex items-center gap-2 text-xs font-bold ${statusConfig.bg}`}>
            <statusConfig.icon className="w-4 h-4" />
            <span>{statusConfig.text}</span>
          </div>
        )}
      </div>

      {result && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 items-start">
          {/* Main projection panel (2/3) */}
          <div className="lg:col-span-2 space-y-6">
            <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-6">
              <h3 className="text-sm font-bold text-white">Cash Flow Balance Projection</h3>

              {/* Gauge and metrics wrapper */}
              <div className="flex flex-col sm:flex-row items-center justify-around gap-6 py-4">
                {/* Circular Indicator */}
                <div className="relative w-40 h-40 flex items-center justify-center">
                  {/* Outer circle track */}
                  <svg className="w-full h-full transform -rotate-90">
                    <circle
                      cx="80"
                      cy="80"
                      r="68"
                      stroke="#1e293b"
                      strokeWidth="10"
                      fill="transparent"
                    />
                    <circle
                      cx="80"
                      cy="80"
                      r="68"
                      stroke={
                        result.status === 'Safe'
                          ? '#10b981'
                          : result.status === 'Risky'
                          ? '#f59e0b'
                          : '#f43f5e'
                      }
                      strokeWidth="10"
                      fill="transparent"
                      strokeDasharray={427}
                      strokeDashoffset={
                        result.projectedBalance > 0
                          ? 427 - Math.min((result.projectedBalance / Math.max(result.monthlyIncome, 1)) * 427, 427)
                          : 427 // deficit
                      }
                      className="transition-all duration-1000 ease-out"
                    />
                  </svg>
                  <div className="absolute flex flex-col items-center justify-center text-center">
                    <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Projected Bal</span>
                    <span className={`text-base font-extrabold mt-0.5
                      ${result.projectedBalance < 0 ? 'text-rose-400' : 'text-white'}
                    `}>
                      {formatRupee(result.projectedBalance)}
                    </span>
                  </div>
                </div>

                {/* Status metrics side */}
                <div className="space-y-4 min-w-[12rem]">
                  <div className="space-y-1">
                    <span className="text-slate-500 text-[10px] font-semibold uppercase tracking-wider">Income Recv</span>
                    <p className="text-sm font-bold text-white">{formatRupee(result.monthlyIncome)}</p>
                  </div>
                  <div className="space-y-1">
                    <span className="text-slate-500 text-[10px] font-semibold uppercase tracking-wider">Spent So Far</span>
                    <p className="text-sm font-bold text-slate-300">{formatRupee(result.totalSpentSoFar)}</p>
                  </div>
                  <div className="space-y-1">
                    <span className="text-slate-500 text-[10px] font-semibold uppercase tracking-wider">Forecast Outflow</span>
                    <p className="text-sm font-bold text-slate-300">{formatRupee(result.projectedFutureSpend)}</p>
                  </div>
                </div>
              </div>

              {/* Statistics Grid */}
              <div className="grid grid-cols-3 gap-4 pt-6 border-t border-slate-805">
                <div className="bg-slate-950/40 p-3.5 rounded-xl border border-slate-850/60 text-center">
                  <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Daily Avg</span>
                  <span className="text-xs font-bold text-white block mt-1">{formatRupee(result.dailyAverage)}</span>
                </div>
                <div className="bg-slate-950/40 p-3.5 rounded-xl border border-slate-850/60 text-center">
                  <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Days Left</span>
                  <span className="text-xs font-bold text-white block mt-1 flex items-center justify-center gap-1">
                    <Calendar className="w-3.5 h-3.5 text-slate-500" />
                    <span>{result.daysRemaining}</span>
                  </span>
                </div>
                <div className="bg-slate-950/40 p-3.5 rounded-xl border border-slate-850/60 text-center">
                  <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Top Category</span>
                  <span className="text-xs font-bold text-emerald-400 block mt-1 truncate" title={result.topSpendingCategory ?? ''}>
                    {result.topSpendingCategory || 'Others'}
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* AI Commentary Panel (1/3) */}
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
            <h4 className="text-sm font-bold text-white flex items-center gap-2">
              <Sparkles className="w-4.5 h-4.5 text-emerald-400" />
              <span>AI Cashflow Advisory</span>
            </h4>

            <div className="p-4 bg-slate-950 border border-slate-850 rounded-xl">
              <p className="text-xs text-slate-300 leading-relaxed">
                {result.aiCommentary}
              </p>
            </div>

            {result.status === 'Danger' && (
              <div className="p-4 bg-rose-500/5 border border-rose-500/10 rounded-xl flex items-start gap-2.5">
                <AlertTriangle className="w-4.5 h-4.5 text-rose-400 flex-shrink-0 mt-0.5" />
                <div className="space-y-1">
                  <span className="text-[10px] font-semibold text-rose-400 uppercase tracking-wider block">Deficit Warning</span>
                  <p className="text-[11px] text-slate-400 leading-relaxed">
                    Based on your average spending pace of {formatRupee(result.dailyAverage)}/day, your month-end balance is predicted to run into a deficit. Reduce non-essential expenses immediately.
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
