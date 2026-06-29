import { useAnalytics } from '../hooks/useAnalytics'
import { useSessionStore } from '../../../store/session.store'
import { SpendingPieChart } from '../components/SpendingPieChart'
import { SpendingBarChart } from '../components/SpendingBarChart'
import { GuestBanner } from '../../../components/layout/GuestBanner'
import { formatRupee } from '../../../utils/currency'
import { formatDate } from '../../../utils/date'
import {
  TrendingUp,
  TrendingDown,
  PiggyBank,
  RefreshCcw,
  ArrowRight,
  Upload,
  ArrowUpRight,
  ArrowDownLeft
} from 'lucide-react'
import { Link } from 'react-router-dom'

export function DashboardPage() {
  const { data: analytics, isLoading, error } = useAnalytics()
  const { sessionId, isGuest } = useSessionStore()

  // 1. Loading State
  if (isLoading) {
    return (
      <div className="p-8 max-w-7xl mx-auto space-y-6">
        <div className="h-8 w-48 bg-slate-900 border border-slate-800 rounded-lg animate-pulse mb-6"></div>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="h-28 bg-slate-900 border border-slate-800 rounded-xl animate-pulse"></div>
          ))}
        </div>
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 h-80 bg-slate-900 border border-slate-800 rounded-xl animate-pulse"></div>
          <div className="h-80 bg-slate-900 border border-slate-800 rounded-xl animate-pulse"></div>
        </div>
      </div>
    )
  }

  // 2. Error State
  if (error) {
    return (
      <div className="p-8 max-w-md mx-auto text-center space-y-4 mt-20">
        <h3 className="text-white font-bold text-lg">Failed to Load Dashboard</h3>
        <p className="text-slate-400 text-sm">{error.message || 'Check your server connection.'}</p>
        <Link to="/upload" className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl text-sm font-semibold">
          <Upload className="w-4 h-4" />
          <span>Upload New CSV</span>
        </Link>
      </div>
    )
  }

  // 3. No Session (No CSV uploaded yet)
  if (!sessionId || !analytics) {
    return (
      <div className="min-h-[calc(100vh-8rem)] flex items-center justify-center px-4">
        <div className="text-center max-w-md space-y-6">
          <div className="w-16 h-16 bg-slate-900 border border-slate-800 rounded-2xl flex items-center justify-center mx-auto text-emerald-400 shadow-lg shadow-slate-950/20">
            <Upload className="w-8 h-8" />
          </div>
          <div>
            <h2 className="text-xl font-bold text-white mb-2">No Statement Loaded</h2>
            <p className="text-slate-400 text-sm leading-relaxed">
              Upload your bank statement CSV file first to populate the spending analysis and charts.
            </p>
          </div>
          <Link
            to="/upload"
            className="inline-flex items-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl font-semibold text-sm shadow-lg shadow-emerald-600/15 transition-all duration-200"
          >
            <span>Upload CSV Statement</span>
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    )
  }

  // 4. Data Processing (Guest Restriction: Top 3 Categories only)
  let categoryBreakdowns = analytics.byCategory
  if (isGuest) {
    const sorted = [...analytics.byCategory].sort((a, b) => b.amount - a.amount)
    const top3 = sorted.slice(0, 3)
    const remaining = sorted.slice(3)
    const othersAmount = remaining.reduce((sum, c) => sum + c.amount, 0)
    
    if (othersAmount > 0) {
      top3.push({
        category: 'Others',
        amount: othersAmount,
        percentage: analytics.totalSpending > 0 ? (othersAmount / analytics.totalSpending) * 100 : 0
      })
    }
    categoryBreakdowns = top3
  }

  return (
    <div className="py-10 px-6 max-w-7xl mx-auto space-y-8">
      {/* Title */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-white">Financial Dashboard</h1>
          <p className="text-slate-400 text-xs mt-0.5">Overview of your categorized bank statement analysis</p>
        </div>
        {isGuest && (
          <span className="px-3 py-1 bg-amber-500/10 text-amber-400 border border-amber-500/20 rounded-full text-xs font-semibold uppercase tracking-wider">
            Guest Mode (Limited View)
          </span>
        )}
      </div>

      {/* Metric Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Income */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl flex items-center justify-between relative overflow-hidden">
          <div className="space-y-1.5">
            <span className="text-slate-500 text-xs font-semibold uppercase tracking-wider">Total Income</span>
            <h3 className="text-xl font-extrabold text-white">{formatRupee(analytics.totalIncome)}</h3>
          </div>
          <div className="w-10 h-10 bg-emerald-500/10 rounded-xl flex items-center justify-center border border-emerald-500/20 text-emerald-400">
            <TrendingUp className="w-5 h-5" />
          </div>
        </div>

        {/* Expenses */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl flex items-center justify-between relative overflow-hidden">
          <div className="space-y-1.5">
            <span className="text-slate-500 text-xs font-semibold uppercase tracking-wider">Total Spending</span>
            <h3 className="text-xl font-extrabold text-white">{formatRupee(analytics.totalSpending)}</h3>
          </div>
          <div className="w-10 h-10 bg-rose-500/10 rounded-xl flex items-center justify-center border border-rose-500/20 text-rose-400">
            <TrendingDown className="w-5 h-5" />
          </div>
        </div>

        {/* Net Savings */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl flex items-center justify-between relative overflow-hidden">
          <div className="space-y-1.5">
            <span className="text-slate-500 text-xs font-semibold uppercase tracking-wider">Net Savings</span>
            <h3 className="text-xl font-extrabold text-white">{formatRupee(analytics.netSavings)}</h3>
          </div>
          <div className="w-10 h-10 bg-blue-500/10 rounded-xl flex items-center justify-center border border-blue-500/20 text-blue-400">
            <PiggyBank className="w-5 h-5" />
          </div>
        </div>

        {/* Refunds */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl flex items-center justify-between relative overflow-hidden">
          <div className="space-y-1.5">
            <span className="text-slate-500 text-xs font-semibold uppercase tracking-wider">Refunds / Cashbacks</span>
            <h3 className="text-xl font-extrabold text-white">{formatRupee(analytics.totalRefunds)}</h3>
          </div>
          <div className="w-10 h-10 bg-teal-500/10 rounded-xl flex items-center justify-center border border-teal-500/20 text-teal-400">
            <RefreshCcw className="w-5 h-5" />
          </div>
        </div>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Bar Chart (spending breakdown) */}
        <div className="lg:col-span-2 bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl">
          <h3 className="text-sm font-bold text-white mb-6">Spending Breakdown</h3>
          <SpendingBarChart data={categoryBreakdowns} />
        </div>

        {/* Pie Chart (category ratios) */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl">
          <h3 className="text-sm font-bold text-white mb-6">Expense Allocation</h3>
          <SpendingPieChart data={categoryBreakdowns} />
        </div>
      </div>

      {/* Guest Limitation Warning Banner */}
      {isGuest && (
        <GuestBanner
          title="Showing Truncated Category Data"
          description="In Guest Mode, you only see details for your top 3 spending categories. Create a free account to unlock full allocations, transaction tracking, and goal planning features."
        />
      )}

      {/* Transactions Ledger */}
      <div className="bg-slate-900 border border-slate-800 rounded-2xl shadow-xl overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-800">
          <h3 className="text-sm font-bold text-white">Top Transactions</h3>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-slate-850 text-[10px] font-semibold text-slate-500 uppercase tracking-wider bg-slate-900/50">
                <th className="px-6 py-3.5">Date</th>
                <th className="px-6 py-3.5">Description</th>
                <th className="px-6 py-3.5">Category</th>
                <th className="px-6 py-3.5 text-right">Amount</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-850 text-xs">
              {analytics.topTransactions.map((tx) => {
                const isCredit = tx.type === 'Credit'
                return (
                  <tr key={tx.id} className="hover:bg-slate-950/20 transition-colors text-slate-300">
                    <td className="px-6 py-4 whitespace-nowrap text-slate-400 font-medium">
                      {formatDate(tx.date)}
                    </td>
                    <td className="px-6 py-4 font-semibold text-white truncate max-w-xs">
                      {tx.description}
                    </td>
                    <td className="px-6 py-4">
                      <span className="px-2.5 py-1 rounded-full bg-slate-800 border border-slate-700 text-slate-400 font-medium text-[10px]">
                        {tx.category}
                      </span>
                    </td>
                    <td className={`px-6 py-4 text-right font-extrabold whitespace-nowrap flex items-center justify-end gap-1.5
                      ${isCredit ? 'text-emerald-400' : 'text-slate-300'}
                    `}>
                      {isCredit ? (
                        <ArrowUpRight className="w-3.5 h-3.5" />
                      ) : (
                        <ArrowDownLeft className="w-3.5 h-3.5 text-slate-500" />
                      )}
                      <span>{formatRupee(tx.amount)}</span>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
