import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { GuestRoute } from './GuestRoute'

// Pages
import { LoginPage } from '../features/auth/pages/LoginPage'
import { RegisterPage } from '../features/auth/pages/RegisterPage'
import { UploadPage } from '../features/upload/pages/UploadPage'
import { DashboardPage } from '../features/analytics/pages/DashboardPage'
import { EMIPage } from '../features/emi/pages/EMIPage'
import { PredictorPage } from '../features/predictor/pages/PredictorPage'
import { GoalsPage } from '../features/goals/pages/GoalsPage'
import { ChatPage } from '../features/chat/pages/ChatPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      // Public guest-only routes (redirect to dashboard if logged in)
      {
        element: <GuestRoute />,
        children: [
          { path: 'login', element: <LoginPage /> },
          { path: 'register', element: <RegisterPage /> }
        ]
      },

      // Public routes (accessible to everyone)
      { index: true, element: <UploadPage /> },
      { path: 'upload', element: <UploadPage /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'emi', element: <EMIPage /> },
      { path: 'chat', element: <ChatPage /> },

      // Protected routes (requires login)
      {
        element: <ProtectedRoute />,
        children: [
          { path: 'predictor', element: <PredictorPage /> },
          { path: 'goals', element: <GoalsPage /> }
        ]
      }
    ]
  }
])
