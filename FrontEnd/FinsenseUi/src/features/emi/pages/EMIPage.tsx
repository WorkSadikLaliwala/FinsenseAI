import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { EMISchema, type EMIFormData } from '../schemas/emi.schema'
import { useEMICheck } from '../hooks/useEMICheck'
import { useSessionStore } from '../../../store/session.store'
import { GuestBanner } from '../../../components/layout/GuestBanner'
import { formatRupee } from '../../../utils/currency'
import {
  Sparkles,
  Calculator,
  ShieldCheck,
  ShieldAlert,
  Loader2,
  Lock,
  Coins
} from 'lucide-react'

export function EMIPage() {
  const { isGuest, emiCheckCount } = useSessionStore()
  const { mutate: checkAffordability, data: result, isPending, error } = useEMICheck()

  const isLocked = isGuest && emiCheckCount >= 1

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors }
  } = useForm({
    resolver: zodResolver(EMISchema),
    defaultValues: {
      salary: 75000,
      monthlyExpenses: 30000,
      loanAmount: 1500000,
      tenureMonths: 180,
      interestRatePerAnnum: 8.5
    }
  })

  const onSubmit = (data: any) => {
    if (isLocked) return
    checkAffordability(data as EMIFormData)
  }

  // Quick fill helper
  const handleQuickFill = (salary: number, expenses: number, loan: number, term: number, rate: number) => {
    if (isLocked) return
    setValue('salary', salary)
    setValue('monthlyExpenses', expenses)
    setValue('loanAmount', loan)
    setValue('tenureMonths', term)
    setValue('interestRatePerAnnum', rate)
  }

  // Get status badge colors
  const getStatusDetails = (status: string | null) => {
    switch (status) {
      case 'Safe':
        return {
          bg: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
          icon: ShieldCheck,
          text: 'Safe Affordability'
        }
      case 'Risky':
        return {
          bg: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
          icon: ShieldAlert,
          text: 'Risky Budget'
        }
      case 'Danger':
        return {
          bg: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
          icon: ShieldAlert,
          text: 'Over-Leveraged'
        }
      default:
        return {
          bg: 'bg-slate-800 text-slate-400 border-slate-700',
          icon: ShieldAlert,
          text: 'Not Available'
        }
    }
  }

  const statusConfig = result ? getStatusDetails(result.status) : null

  return (
    <div className="py-10 px-6 max-w-6xl mx-auto space-y-8">
      {/* Title */}
      <div>
        <h1 className="text-2xl font-bold text-white">EMI Affordability Checker</h1>
        <p className="text-slate-400 text-xs mt-0.5">Determine if your net salary and monthly expenses support a new loan EMI</p>
      </div>

      {/* Main Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-5 gap-8 items-start">
        {/* Left Form (2/5) */}
        <div className="lg:col-span-2 relative">
          {/* Lock Overlay for Guests */}
          {isLocked && (
            <div className="absolute inset-0 bg-slate-950/80 backdrop-blur-sm z-10 rounded-2xl flex flex-col items-center justify-center p-6 border border-slate-800 text-center">
              <div className="w-12 h-12 bg-amber-500/10 rounded-full flex items-center justify-center text-amber-400 border border-amber-500/20 mb-4 animate-bounce">
                <Lock className="w-5 h-5" />
              </div>
              <h4 className="text-white font-bold text-sm mb-1.5">Checker Locked</h4>
              <p className="text-slate-400 text-xs max-w-xs leading-relaxed mb-4">
                You have reached the maximum free limit of 1 check. Sign up to unlock unlimited affordability assessments.
              </p>
            </div>
          )}

          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-6">
            <h3 className="text-sm font-bold text-white flex items-center gap-2">
              <Calculator className="w-4 h-4 text-emerald-400" />
              <span>Loan Parameters</span>
            </h3>

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Salary */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Monthly Net Salary (Rs.)
                </label>
                <input
                  type="number"
                  step="any"
                  disabled={isLocked}
                  className="w-full px-4 py-2 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none outline-0 transition-all"
                  {...register('salary', { valueAsNumber: true })}
                />
                {errors.salary && <p className="mt-1 text-xs text-rose-400">{errors.salary.message}</p>}
              </div>

              {/* Monthly Expenses */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Existing Monthly Expenses (Rs.)
                </label>
                <input
                  type="number"
                  step="any"
                  disabled={isLocked}
                  className="w-full px-4 py-2 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none outline-0 transition-all"
                  {...register('monthlyExpenses', { valueAsNumber: true })}
                />
                {errors.monthlyExpenses && <p className="mt-1 text-xs text-rose-400">{errors.monthlyExpenses.message}</p>}
              </div>

              {/* Loan Amount */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Proposed Loan Amount (Rs.)
                </label>
                <input
                  type="number"
                  step="any"
                  disabled={isLocked}
                  className="w-full px-4 py-2 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none outline-0 transition-all"
                  {...register('loanAmount', { valueAsNumber: true })}
                />
                {errors.loanAmount && <p className="mt-1 text-xs text-rose-400">{errors.loanAmount.message}</p>}
              </div>

              {/* Tenure (Months) */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Tenure (Months)
                </label>
                <input
                  type="number"
                  step="any"
                  disabled={isLocked}
                  className="w-full px-4 py-2 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none outline-0 transition-all"
                  {...register('tenureMonths', { valueAsNumber: true })}
                />
                {errors.tenureMonths && <p className="mt-1 text-xs text-rose-400">{errors.tenureMonths.message}</p>}
              </div>

              {/* Interest Rate */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Interest Rate (% p.a.)
                </label>
                <input
                  type="number"
                  step="any"
                  disabled={isLocked}
                  className="w-full px-4 py-2 bg-slate-950 border border-slate-800 focus:border-emerald-500/30 focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-sm text-white placeholder-slate-600 outline-none outline-0 transition-all"
                  {...register('interestRatePerAnnum', { valueAsNumber: true })}
                />
                {errors.interestRatePerAnnum && <p className="mt-1 text-xs text-rose-400">{errors.interestRatePerAnnum.message}</p>}
              </div>

              <button
                type="submit"
                disabled={isPending || isLocked}
                className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl font-semibold text-sm shadow-lg shadow-emerald-600/15 flex items-center justify-center gap-2 transition-all disabled:opacity-50"
              >
                {isPending ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    <span>Analyzing...</span>
                  </>
                ) : (
                  <span>Evaluate Affordability</span>
                )}
              </button>
            </form>

            {/* Quick Fills */}
            {!isLocked && (
              <div className="pt-4 border-t border-slate-800 space-y-2">
                <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Quick Presets</span>
                <div className="flex flex-wrap gap-2">
                  <button
                    onClick={() => handleQuickFill(50000, 20000, 500000, 60, 9.5)}
                    className="px-2.5 py-1 bg-slate-950 hover:bg-slate-850 text-slate-400 rounded-lg text-[10px] border border-slate-800 transition-colors"
                  >
                    Car Loan (5L)
                  </button>
                  <button
                    onClick={() => handleQuickFill(120000, 45000, 4500000, 240, 8.55)}
                    className="px-2.5 py-1 bg-slate-950 hover:bg-slate-850 text-slate-400 rounded-lg text-[10px] border border-slate-800 transition-colors"
                  >
                    Home Loan (45L)
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Right Result Card (3/5) */}
        <div className="lg:col-span-3 space-y-6">
          {result ? (
            <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-6 relative overflow-hidden">
              {/* Header result */}
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                  <span className="text-slate-500 text-xs font-semibold uppercase tracking-wider">Proposed Monthly EMI</span>
                  <h2 className="text-2xl font-extrabold text-white mt-1">{formatRupee(result.emiAmount)}</h2>
                </div>

                {/* Status Badge */}
                {statusConfig && (
                  <div className={`px-4 py-2 border rounded-xl flex items-center gap-2 text-xs font-bold ${statusConfig.bg}`}>
                    <statusConfig.icon className="w-4 h-4" />
                    <span>{statusConfig.text}</span>
                  </div>
                )}
              </div>

              {/* Stats Columns */}
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-6 pt-4 border-t border-slate-800">
                <div className="space-y-1">
                  <span className="text-slate-500 text-[10px] font-semibold uppercase tracking-wider">Disposable Income</span>
                  <p className="text-sm font-bold text-white">{formatRupee(result.disposableIncome)}</p>
                </div>
                <div className="space-y-1">
                  <span className="text-slate-500 text-[10px] font-semibold uppercase tracking-wider">Safe EMI Limit</span>
                  <p className="text-sm font-bold text-emerald-400">{formatRupee(result.safeEMILimit)}</p>
                </div>
                <div className="space-y-1">
                  <span className="text-slate-500 text-[10px] font-semibold uppercase tracking-wider">EMI % of Salary</span>
                  <p className="text-sm font-bold text-white">{result.emiAsPercentOfSalary.toFixed(1)}%</p>
                </div>
              </div>

              {/* AI Affordability Explanation */}
              <div className="p-4 rounded-xl bg-slate-950 border border-slate-850 space-y-3.5">
                <h4 className="text-xs font-bold text-white flex items-center justify-between gap-2 w-full">
                  <span className="flex items-center gap-2">
                    <Sparkles className="w-4.5 h-4.5 text-emerald-400" />
                    <span>Affordability Review</span>
                  </span>
                  <span className="px-1.5 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 text-[9px] font-semibold select-none animate-pulse">
                    Llama 3 (Groq API)
                  </span>
                </h4>
                <p className="text-xs text-slate-300 leading-relaxed">
                  {result.reason}
                </p>
              </div>

              {/* AI Recommendation */}
              <div className="p-4 rounded-xl bg-emerald-500/5 border border-emerald-500/10 space-y-2">
                <h4 className="text-xs font-bold text-emerald-400 flex items-center gap-1.5">
                  <Coins className="w-4 h-4" />
                  <span>Recommendation</span>
                </h4>
                <p className="text-xs text-slate-400 leading-relaxed">
                  {result.recommendation}
                </p>
              </div>
            </div>
          ) : (
            <div className="bg-slate-900 border border-slate-800 rounded-2xl p-8 shadow-xl text-center flex flex-col items-center justify-center min-h-[22rem]">
              <div className="w-12 h-12 bg-slate-800 rounded-xl flex items-center justify-center mb-4 text-slate-400 border border-slate-700">
                <Calculator className="w-6 h-6" />
              </div>
              <h3 className="text-white font-bold text-sm">Awaiting Evaluation</h3>
              <p className="text-slate-500 text-xs mt-1.5 max-w-xs leading-relaxed">
                Fill in the details in the left panel and click evaluate to review affordability analytics.
              </p>
            </div>
          )}

          {isLocked && (
            <GuestBanner
              title="Unlock Unlimited Affordability Checks"
              description="Keep evaluating different budgets, interest rates, and structures. Create an account to check as many loan affordability scenarios as you want."
            />
          )}

          {error && (
            <div className="p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm text-center">
              {error.message || 'Error occurred while evaluating loan affordability.'}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
