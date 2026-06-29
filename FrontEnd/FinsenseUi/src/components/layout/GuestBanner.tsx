import { Link } from 'react-router-dom'
import { Sparkles } from 'lucide-react'

interface GuestBannerProps {
  title?: string
  description?: string
}

export function GuestBanner({
  title = "Unlock Premium Financial Insights",
  description = "You have used your free access. Create a free account to unlock unlimited access, Month-End Predictors, Savings Goal Trackers, and unlimited AI consultations."
}: GuestBannerProps) {
  return (
    <div className="w-full max-w-2xl mx-auto my-8 bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden relative shadow-xl shadow-slate-950/20">
      {/* Decorative colored glow */}
      <div className="absolute -top-12 -left-12 w-32 h-32 bg-emerald-500/10 rounded-full blur-2xl"></div>
      <div className="absolute -bottom-12 -right-12 w-32 h-32 bg-indigo-500/10 rounded-full blur-2xl"></div>

      <div className="p-8 relative flex flex-col items-center text-center">
        <div className="w-12 h-12 bg-emerald-500/10 rounded-xl flex items-center justify-center mb-5 border border-emerald-500/20">
          <Sparkles className="w-6 h-6 text-emerald-400" />
        </div>

        <h3 className="text-xl font-bold text-white mb-3">{title}</h3>
        <p className="text-sm text-slate-400 leading-relaxed mb-8 max-w-lg">
          {description}
        </p>

        <div className="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
          <Link
            to="/register"
            className="px-6 py-2.5 rounded-xl font-semibold text-sm bg-emerald-600 hover:bg-emerald-500 text-white shadow-lg shadow-emerald-600/20 text-center transition-all duration-200"
          >
            Create Free Account
          </Link>
          <Link
            to="/login"
            className="px-6 py-2.5 rounded-xl font-semibold text-sm border border-slate-700 bg-slate-800 hover:bg-slate-700/80 text-white text-center transition-all duration-200"
          >
            Sign In
          </Link>
        </div>
      </div>
    </div>
  )
}
