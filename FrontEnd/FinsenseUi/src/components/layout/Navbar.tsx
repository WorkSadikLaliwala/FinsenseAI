import { useNavigate, Link } from 'react-router-dom'
import { LogOut, Wifi, WifiOff } from 'lucide-react'
import { useAuthStore } from '../../store/auth.store'
import { useSessionStore } from '../../store/session.store'
import { useEffect, useState } from 'react'

export function Navbar() {
  const navigate = useNavigate()
  const { isAuthenticated, clearAuth } = useAuthStore()
  const { clearSession } = useSessionStore()
  const [isOnline, setIsOnline] = useState(navigator.onLine)

  useEffect(() => {
    const handleOnline = () => setIsOnline(true)
    const handleOffline = () => setIsOnline(false)

    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)

    return () => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [])

  const handleLogout = () => {
    clearAuth()
    clearSession()
    navigate('/login')
  }

  return (
    <header className="h-16 border-b border-slate-800 bg-slate-900 px-6 flex items-center justify-between text-slate-300">
      {/* Title */}
      <div className="flex items-center gap-4">
        <span className="text-slate-400 text-sm font-medium">FinSense AI Workspace</span>
      </div>

      {/* Utilities / Actions */}
      <div className="flex items-center gap-5">
        {/* Offline Indicator */}
        <div className="flex items-center gap-1.5 text-xs font-medium">
          {isOnline ? (
            <div className="flex items-center gap-1 text-emerald-500">
              <Wifi className="w-4 h-4" />
              <span>Online</span>
            </div>
          ) : (
            <div className="flex items-center gap-1 text-rose-500 animate-pulse">
              <WifiOff className="w-4 h-4" />
              <span>Offline</span>
            </div>
          )}
        </div>

        {/* Auth Actions */}
        {isAuthenticated ? (
          <div className="flex items-center gap-4">
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium hover:bg-rose-500/10 hover:text-rose-400 border border-transparent hover:border-rose-500/20 transition-all duration-200"
            >
              <LogOut className="w-4 h-4" />
              <span>Logout</span>
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-3">
            <Link
              to="/login"
              className="px-3.5 py-1.5 rounded-lg text-sm font-medium hover:bg-slate-800 border border-slate-700 text-white transition-all duration-200"
            >
              Login
            </Link>
            <Link
              to="/register"
              className="px-3.5 py-1.5 rounded-lg text-sm font-medium bg-emerald-600 hover:bg-emerald-500 text-white shadow-md shadow-emerald-600/15 transition-all duration-200"
            >
              Create Account
            </Link>
          </div>
        )}
      </div>
    </header>
  )
}
