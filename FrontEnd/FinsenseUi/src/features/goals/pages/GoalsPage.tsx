import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { GoalSchema, type GoalFormData } from '../schemas/goal.schema'
import { useGoals, useCreateGoal, useDeleteGoal, useUpdateGoal } from '../hooks/useGoals'
import { formatRupee, formatRupeeCompact } from '../../../utils/currency'
import { formatDate } from '../../../utils/date'
import {
  Target,
  Sparkles,
  Loader2,
  Trash2,
  Calendar,
  FileText,
  X,
  Plus,
  Coins,
  ShieldCheck,
  ShieldAlert,
  Edit2
} from 'lucide-react'

export function GoalsPage() {
  const todayString = new Date().toISOString().split('T')[0]
  const { data: goals, isLoading, error } = useGoals()
  const { mutate: createGoal, isPending: isCreating } = useCreateGoal()
  const { mutate: deleteGoal } = useDeleteGoal()
  const { mutate: updateGoal } = useUpdateGoal()

  // Modal / details states
  const [feasibilityReport, setFeasibilityReport] = useState<any>(null)
  const [editingGoal, setEditingGoal] = useState<any>(null)
  const [updatedSavings, setUpdatedSavings] = useState<number>(0)
  const [editSavingsError, setEditSavingsError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm({
    resolver: zodResolver(GoalSchema)
  })

  const onSubmit = (data: any) => {
    createGoal(data as GoalFormData, {
      onSuccess: (report) => {
        setFeasibilityReport({
          ...report,
          goalName: data.goalName,
          targetAmount: data.targetAmount
        })
        reset()
      }
    })
  }

  const handleUpdateSavings = (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingGoal) return

    if (isNaN(updatedSavings) || updatedSavings < 0) {
      setEditSavingsError('Savings cannot be negative')
      return
    }

    if (updatedSavings > editingGoal.targetAmount) {
      setEditSavingsError(`Savings cannot exceed target of ${formatRupee(editingGoal.targetAmount)}`)
      return
    }

    setEditSavingsError(null)

    updateGoal(
      {
        id: editingGoal.id,
        data: {
          currentSavings: updatedSavings
        }
      },
      {
        onSuccess: () => {
          setEditingGoal(null)
        }
      }
    )
  }

  // Get status details
  const getStatusDetails = (status: string | null) => {
    switch (status) {
      case 'On Track':
        return {
          bg: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20',
          icon: ShieldCheck,
          text: 'On Track'
        }
      case 'At Risk':
        return {
          bg: 'bg-amber-500/10 text-amber-400 border border-amber-500/20',
          icon: ShieldAlert,
          text: 'At Risk'
        }
      case 'Not Feasible':
        return {
          bg: 'bg-rose-500/10 text-rose-400 border border-rose-500/20',
          icon: ShieldAlert,
          text: 'Not Feasible'
        }
      default:
        return {
          bg: 'bg-slate-800 text-slate-400 border border-slate-700',
          icon: ShieldCheck,
          text: 'Evaluating'
        }
    }
  }

  return (
    <div className="py-10 px-6 max-w-6xl mx-auto space-y-8 relative">
      {/* Title */}
      <div>
        <h1 className="text-2xl font-bold text-white">Savings Goals Tracker</h1>
        <p className="text-slate-400 text-xs mt-0.5">Track, edit, and audit the financial feasibility of your future goals</p>
      </div>

      {/* Main Grid layout */}
      <div className="grid grid-cols-1 lg:grid-cols-5 gap-8 items-start">
        {/* Create Form (2/5) */}
        <div className="lg:col-span-2 bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-6">
          <h3 className="text-sm font-bold text-white flex items-center gap-2">
            <Plus className="w-4 h-4 text-emerald-400" />
            <span>Create Savings Goal</span>
          </h3>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {/* Goal Title */}
            <div>
              <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                Goal Name
              </label>
              <input
                type="text"
                placeholder="e.g. Dream House Deposit"
                className="w-full px-4 py-2.5 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none transition-all"
                {...register('goalName')}
              />
              {errors.goalName && <p className="mt-1 text-xs text-rose-400">{errors.goalName.message}</p>}
            </div>

            {/* Target Amount */}
            <div>
              <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                Target Amount (Rs.)
              </label>
              <input
                type="number"
                step="any"
                placeholder="1000000"
                className="w-full px-4 py-2.5 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none transition-all"
                {...register('targetAmount', { valueAsNumber: true })}
              />
              {errors.targetAmount && <p className="mt-1 text-xs text-rose-400">{errors.targetAmount.message}</p>}
            </div>

            {/* Target Date */}
            <div>
              <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                Target Date
              </label>
              <input
                type="date"
                min={todayString}
                className="w-full px-4 py-2.5 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-650 outline-none transition-all cursor-pointer"
                onClick={(e) => {
                  try {
                    e.currentTarget.showPicker();
                  } catch (err) {
                    console.warn('showPicker not supported', err);
                  }
                }}
                {...register('targetDate')}
              />
              {errors.targetDate && <p className="mt-1 text-xs text-rose-400">{errors.targetDate.message}</p>}
            </div>

            {/* Current Savings */}
            <div>
              <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                Initial Savings Saved (Rs.)
              </label>
              <input
                type="number"
                step="any"
                placeholder="50000"
                className="w-full px-4 py-2.5 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none transition-all"
                {...register('currentSavings', { valueAsNumber: true })}
              />
              {errors.currentSavings && <p className="mt-1 text-xs text-rose-400">{errors.currentSavings.message}</p>}
            </div>

            {/* Submit */}
            <button
              type="submit"
              disabled={isCreating}
              className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl font-semibold text-sm shadow-lg shadow-emerald-600/15 flex items-center justify-center gap-2 transition-all disabled:opacity-50"
            >
              {isCreating ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  <span>Creating & Auditing...</span>
                </>
              ) : (
                <span>Create & Analyze Goal</span>
              )}
            </button>
          </form>
        </div>

        {/* Goals Grid list (3/5) */}
        <div className="lg:col-span-3 space-y-6">
          {isLoading ? (
            <div className="space-y-4">
              {[1, 2].map(i => (
                <div key={i} className="h-40 bg-slate-900 border border-slate-800 rounded-2xl animate-pulse"></div>
              ))}
            </div>
          ) : error ? (
            <div className="p-4 bg-rose-500/10 border border-rose-500/20 text-rose-400 rounded-2xl text-xs">
              Failed to load savings goals list.
            </div>
          ) : !goals || goals.length === 0 ? (
            <div className="border border-slate-800 bg-slate-900/10 rounded-2xl p-10 text-center flex flex-col items-center justify-center min-h-[20rem]">
              <div className="w-12 h-12 bg-slate-900 border border-slate-800 rounded-xl flex items-center justify-center mb-4 text-slate-400">
                <Target className="w-6 h-6" />
              </div>
              <h3 className="text-white font-bold text-sm">No Savings Goals Set</h3>
              <p className="text-slate-500 text-xs mt-1.5 max-w-xs leading-relaxed">
                Add a goal in the left panel to begin auditing your budget allocations against targets.
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {goals.map((goal) => {
                const percent = Math.min((goal.currentSavings / goal.targetAmount) * 100, 100)
                const statusDetails = getStatusDetails(goal.status)

                return (
                  <div key={goal.id} className="bg-slate-900 border border-slate-800 rounded-2xl p-5 shadow-xl space-y-4 relative overflow-hidden group">
                    {/* Goal Header */}
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h4 className="text-white font-bold text-sm">{goal.goalName}</h4>
                        <div className="flex items-center gap-2 mt-1 text-[10px] text-slate-500">
                          <Calendar className="w-3 h-3" />
                          <span>Target: {formatDate(goal.targetDate)}</span>
                        </div>
                      </div>

                      {/* Status and Action Buttons */}
                      <div className="flex items-center gap-2">
                        {statusDetails && (
                          <div className={`px-2 py-0.5 rounded text-[9px] font-bold border flex items-center gap-1 ${statusDetails.bg}`}>
                            <statusDetails.icon className="w-3 h-3" />
                            <span>{statusDetails.text}</span>
                          </div>
                        )}
                        <button
                          onClick={() => {
                            setEditingGoal(goal)
                            setUpdatedSavings(goal.currentSavings)
                            setEditSavingsError(null)
                          }}
                          className="p-1.5 rounded-lg bg-slate-950 border border-slate-850 hover:border-slate-700 text-slate-400 hover:text-white transition-colors"
                          title="Update savings"
                        >
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => deleteGoal(goal.id)}
                          className="p-1.5 rounded-lg bg-rose-500/10 border border-rose-500/20 hover:bg-rose-500/20 text-rose-400 transition-colors"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>

                    {/* Savings Tracker Bar */}
                    <div className="space-y-2 pt-2">
                      <div className="flex justify-between text-[10px] font-semibold">
                        <span className="text-slate-500">Progress</span>
                        <span className="text-slate-300">
                          {formatRupeeCompact(goal.currentSavings)} / {formatRupeeCompact(goal.targetAmount)} ({percent.toFixed(0)}%)
                        </span>
                      </div>
                      <div className="w-full h-2 bg-slate-950 rounded-full overflow-hidden border border-slate-850">
                        <div
                          className="h-full bg-emerald-500 transition-all duration-500 rounded-full"
                          style={{ width: `${percent}%` }}
                        ></div>
                      </div>
                    </div>
                  </div>
                )
              })}
            </div>
          )}
        </div>
      </div>

      {/* Goal Feasibility report modal */}
      {feasibilityReport && (
        <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-800 rounded-2xl max-w-lg w-full p-6 shadow-2xl relative overflow-hidden space-y-6">
            <div className="absolute -top-12 -right-12 w-32 h-32 bg-emerald-500/5 rounded-full blur-2xl"></div>

            <button
              onClick={() => setFeasibilityReport(null)}
              className="absolute top-4 right-4 p-1 rounded-lg bg-slate-950 border border-slate-850 hover:bg-slate-800 text-slate-400 hover:text-white transition-colors"
            >
              <X className="w-4 h-4" />
            </button>

            {/* Header */}
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-emerald-500/10 rounded-xl flex items-center justify-center border border-emerald-500/20 text-emerald-400">
                <Sparkles className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-white font-bold text-sm">Goal Feasibility Audit</h3>
                <p className="text-[10px] text-slate-400 uppercase mt-0.5">Category audit for: "{feasibilityReport.goalName}"</p>
              </div>
            </div>

            {/* Metric Panel */}
            <div className="grid grid-cols-2 gap-4 p-4 rounded-xl bg-slate-950 border border-slate-850">
              <div className="space-y-1">
                <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Required Monthly saving</span>
                <span className="text-xs font-bold text-white">{formatRupee(feasibilityReport.requiredMonthlySaving)}</span>
              </div>
              <div className="space-y-1">
                <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Current Monthly Surplus</span>
                <span className="text-xs font-bold text-white">{formatRupee(feasibilityReport.currentMonthlySurplus)}</span>
              </div>
              <div className="space-y-1">
                <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Shortfall deficit</span>
                <span className={`text-xs font-bold ${feasibilityReport.shortfall > 0 ? 'text-rose-400' : 'text-emerald-400'}`}>
                  {formatRupee(feasibilityReport.shortfall)}
                </span>
              </div>
              <div className="space-y-1">
                <span className="text-slate-500 text-[9px] font-semibold uppercase tracking-wider block">Months remaining</span>
                <span className="text-xs font-bold text-white">{feasibilityReport.monthsRemaining} months</span>
              </div>
            </div>

            <div className="space-y-2 text-xs">
              <h5 className="font-bold text-white flex items-center justify-between gap-1.5 w-full">
                <span className="flex items-center gap-1.5">
                  <FileText className="w-4 h-4 text-emerald-400" />
                  <span>AI Feasibility Assessment</span>
                </span>
                <span className="px-1.5 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 text-[9px] font-semibold select-none animate-pulse">
                  Llama 3 (Groq API)
                </span>
              </h5>
              <p className="text-slate-300 leading-relaxed bg-slate-950/40 p-3 rounded-lg border border-slate-850">
                {feasibilityReport.reason}
              </p>
            </div>

            {/* Recommendation Tip */}
            <div className="p-3 bg-emerald-500/5 border border-emerald-500/10 rounded-xl flex gap-2">
              <Coins className="w-4.5 h-4.5 text-emerald-400 flex-shrink-0 mt-0.5" />
              <div className="space-y-1">
                <span className="text-[9px] font-semibold text-emerald-400 uppercase tracking-wider">AI Suggestion</span>
                <p className="text-[11px] text-slate-400 leading-relaxed">{feasibilityReport.tip}</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Edit current savings modal */}
      {editingGoal && (
        <div className="fixed inset-0 bg-slate-950/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-800 rounded-2xl max-w-sm w-full p-6 shadow-2xl relative space-y-4">
            <button
              onClick={() => {
                setEditingGoal(null)
                setEditSavingsError(null)
              }}
              className="absolute top-4 right-4 p-1 rounded-lg bg-slate-950 border border-slate-855 hover:bg-slate-800 text-slate-400 hover:text-white transition-colors"
            >
              <X className="w-4 h-4" />
            </button>

            <h3 className="text-white font-bold text-sm">Update Saved Savings</h3>
            <p className="text-xs text-slate-400">Modify the savings accumulated towards "{editingGoal.goalName}"</p>

            <form onSubmit={handleUpdateSavings} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Savings Balance (Rs.)
                </label>
                <input
                  type="number"
                  step="any"
                  value={updatedSavings}
                  onChange={(e) => setUpdatedSavings(Number(e.target.value))}
                  className={`w-full px-4 py-2.5 bg-slate-950 border rounded-xl text-sm text-white outline-none transition-all
                    ${editSavingsError ? 'border-rose-500 focus:border-rose-500/30' : 'border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10'}
                  `}
                />
                {editSavingsError && (
                  <p className="mt-1.5 text-[11px] text-rose-400">{editSavingsError}</p>
                )}
              </div>

              <button
                type="submit"
                className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl font-semibold text-sm shadow-lg shadow-emerald-600/15"
              >
                Save Updates
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
