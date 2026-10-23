import { create } from 'zustand'
import type { AuthResponse, UserInfoDto } from '@/types/api'

const REFRESH_KEY = 'behaatel-refresh-token'

interface AuthState {
  user: UserInfoDto | null
  accessToken: string | null
  refreshToken: string | null
  status: 'loading' | 'authenticated' | 'guest'
  setSession: (session: AuthResponse) => void
  setUser: (user: UserInfoDto) => void
  clearSession: () => void
  initializeGuest: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  accessToken: null,
  refreshToken: localStorage.getItem(REFRESH_KEY),
  status: 'loading',
  setSession: (session) => {
    localStorage.setItem(REFRESH_KEY, session.refreshToken)
    set({
      user: session.user,
      accessToken: session.accessToken,
      refreshToken: session.refreshToken,
      status: 'authenticated',
    })
  },
  setUser: (user) => set({ user, status: 'authenticated' }),
  clearSession: () => {
    localStorage.removeItem(REFRESH_KEY)
    set({ user: null, accessToken: null, refreshToken: null, status: 'guest' })
  },
  initializeGuest: () => set({ status: 'guest' }),
}))

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_KEY)
}
