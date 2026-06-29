import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard,
  UploadCloud,
  Calculator,
  TrendingUp,
  Target,
  MessageSquare,
  History,
  Lock
} from 'lucide-react'
import { useAuthStore } from '../../store/auth.store'

export function Sidebar() {
  const isAuthenticated = useAuthStore(state => state.isAuthenticated)

  const navItems = [
    { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard, role: 'public' },
    { to: '/upload', label: 'Upload CSV', icon: UploadCloud, role: 'public' },
    { to: '/emi', label: 'EMI Checker', icon: Calculator, role: 'public' },
    { to: '/chat', label: 'AI Advisor', icon: MessageSquare, role: 'public' },
    { to: '/predictor', label: 'Predictor', icon: TrendingUp, role: 'protected' },
    { to: '/goals', label: 'Goal Tracker', icon: Target, role: 'protected' }
  ]

  return (
    <aside className="w-64 bg-slate-900 border-r border-slate-800 text-slate-300 flex flex-col h-screen sticky top-0">
      {/* Brand Header */}
      <div className="h-16 flex items-center px-6 border-b border-slate-800 gap-2.5">
        <div className="w-8 h-8 rounded-lg bg-emerald-500 flex items-center justify-center font-bold text-white shadow-md shadow-emerald-500/20">
          F
        </div>
        <span className="font-semibold text-lg text-white tracking-wide">FinSense AI</span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-4 py-6 space-y-1.5 overflow-y-auto">
        {navItems.map(item => {
          const isLocked = item.role === 'protected' && !isAuthenticated

          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `
                flex items-center justify-between px-4 py-3 rounded-lg text-sm font-medium transition-all duration-200 group
                ${isActive
                  ? 'bg-emerald-600/10 text-emerald-400 border border-emerald-500/20'
                  : 'hover:bg-slate-800/60 hover:text-white border border-transparent'
                }
              `}
            >
              <div className="flex items-center gap-3">
                <item.icon className="w-4.5 h-4.5 text-slate-400 group-hover:text-emerald-400 transition-colors" />
                <span>{item.label}</span>
              </div>
              {isLocked && (
                <Lock className="w-3.5 h-3.5 text-slate-500 group-hover:text-amber-500 transition-colors" />
              )}
            </NavLink>
          )
        })}

        {/* History Nav (visible only to logged in users) */}
        {isAuthenticated && (
          <div className="pt-6 mt-6 border-t border-slate-800">
            <span className="px-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Management
            </span>
            <div className="mt-2">
              <NavLink
                to="/upload"
                className="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-slate-300 hover:bg-slate-800/60 hover:text-white transition-all border border-transparent"
              >
                <History className="w-4.5 h-4.5 text-slate-400" />
                <span>Upload History</span>
              </NavLink>
            </div>
          </div>
        )}
      </nav>

      {/* User Info / Profile segment at bottom */}
      <div className="p-4 border-t border-slate-800 bg-slate-950/40">
        <div className="flex items-center gap-3 px-2 py-1.5">
          <div className="w-9 h-9 rounded-full bg-slate-800 flex items-center justify-center font-semibold text-sm text-emerald-400 border border-slate-700">
            {isAuthenticated ? useAuthStore.getState().user?.fullName?.charAt(0) ?? 'U' : 'G'}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-white truncate">
              {isAuthenticated ? useAuthStore.getState().user?.fullName : 'Guest User'}
            </p>
            <p className="text-xs text-slate-500 truncate">
              {isAuthenticated ? useAuthStore.getState().user?.email : 'Limited Free Access'}
            </p>
          </div>
        </div>
      </div>
    </aside>
  )
}
