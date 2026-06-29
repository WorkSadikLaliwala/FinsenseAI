export interface RegisterRequest {
  fullName: string
  email: string
  password: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthResponse {
  token: string | null
  fullName: string | null
  email: string | null
  expiresAt: string
  refreshToken: string | null
}

// Future endpoints
export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface UserProfile {
  fullName: string | null
  email: string | null
  createdAt: string
}

export interface UpdateUserProfileRequest {
  fullName?: string | null
  currentPassword?: string | null
  newPassword?: string | null
}

export interface DeleteAccountRequest {
  password?: string | null
}

// Zustand store shape
export interface AuthUser {
  fullName: string
  email: string
  expiresAt: string
}

export interface AuthState {
  user: AuthUser | null
  token: string | null
  refreshToken: string | null
  isAuthenticated: boolean
}
