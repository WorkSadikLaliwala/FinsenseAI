import { CsvDropzone } from '../components/CsvDropzone'
import { UploadHistory } from '../components/UploadHistory'
import { useUpload } from '../hooks/useUpload'
import { useAuthStore } from '../../../store/auth.store'
import { History, Key } from 'lucide-react'
import { Link } from 'react-router-dom'

export function UploadPage() {
  const { mutate: uploadCsv, isPending, error } = useUpload()
  const isAuthenticated = useAuthStore(state => state.isAuthenticated)

  return (
    <div className="py-10 px-6 max-w-6xl mx-auto space-y-12">
      {/* Title Header */}
      <div className="text-center max-w-2xl mx-auto space-y-4">
        <span className="px-3 py-1 rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 text-xs font-semibold uppercase tracking-wider">
          Personal Finance Advisor
        </span>
        <h1 className="text-3xl sm:text-4xl font-extrabold text-white tracking-tight">
          Analyze Your Spending in Seconds
        </h1>
        <p className="text-slate-400 text-sm leading-relaxed">
          Upload your bank statement CSV file. Our AI engine will categorize your transactions, predict month-end balances, analyze EMI affordability, and answer custom questions.
        </p>
      </div>

      {/* Grid Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 items-start">
        {/* Upload Column (widespan) */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl">
            <h3 className="text-base font-bold text-white mb-1">Select Bank Statement</h3>
            <p className="text-xs text-slate-500 mb-6">Supported fields: Date, Description, Amount, Category (optional)</p>

            <CsvDropzone onUpload={uploadCsv} isPending={isPending} />

            {error && (
              <div className="mt-6 p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm text-center">
                {error.message || 'Failed to upload CSV statement.'}
              </div>
            )}
          </div>
        </div>

        {/* Info/History Column */}
        <div className="space-y-6">
          {isAuthenticated ? (
            <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl">
              <div className="flex items-center gap-2 mb-4 text-white">
                <History className="w-5 h-5 text-emerald-400" />
                <h3 className="text-base font-bold">Recent Uploads</h3>
              </div>
              <UploadHistory />
            </div>
          ) : (
            <div className="bg-slate-900 border border-slate-850 rounded-2xl p-6 shadow-xl relative overflow-hidden flex flex-col justify-center">
              <div className="absolute -top-12 -right-12 w-28 h-28 bg-indigo-500/5 rounded-full blur-xl"></div>
              <div className="w-10 h-10 bg-indigo-500/10 rounded-xl flex items-center justify-center mb-4 border border-indigo-500/20 text-indigo-400">
                <Key className="w-5 h-5" />
              </div>

              <h4 className="text-white font-bold text-sm mb-2">Want to save your statements?</h4>
              <p className="text-slate-400 text-xs leading-relaxed mb-6">
                Create a free account to persist your files, view your upload history from the sidebar, and save your budget tracking configurations.
              </p>

              <Link
                to="/register"
                className="w-full py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-center text-xs font-semibold shadow-md shadow-indigo-600/10 transition-all duration-200"
              >
                Sign Up Now
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
