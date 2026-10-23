import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { getRefreshToken, useAuthStore } from '@/store/authStore'
import type { AuthResponse, ProblemDetails } from '@/types/api'

interface RetriableConfig extends InternalAxiosRequestConfig {
  _retriedAfterRefresh?: boolean
}

declare module 'axios' {
  interface InternalAxiosRequestConfig {
    _retriedAfterRefresh?: boolean
  }
}

const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1'

export const apiClient = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
})

let refreshPromise: Promise<AuthResponse> | null = null

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ProblemDetails>) => {
    const original = error.config as RetriableConfig | undefined
    const url = original?.url ?? ''
    const authRequest = url.includes('/auth/login') || url.includes('/auth/refresh')
    if (error.response?.status !== 401 || !original || authRequest || original._retriedAfterRefresh) {
      return Promise.reject(error)
    }

    original._retriedAfterRefresh = true
    const refreshToken = getRefreshToken()
    if (!refreshToken) {
      expireSession()
      return Promise.reject(error)
    }

    try {
      refreshPromise ??= axios
        .post<AuthResponse>(`${baseURL}/auth/refresh`, { refreshToken })
        .then(({ data }) => {
          useAuthStore.getState().setSession(data)
          return data
        })
        .finally(() => {
          refreshPromise = null
        })
      const session = await refreshPromise
      original.headers.Authorization = `Bearer ${session.accessToken}`
      return await apiClient.request(original)
    } catch (refreshError) {
      expireSession()
      return Promise.reject(refreshError)
    }
  },
)

function expireSession(): void {
  useAuthStore.getState().clearSession()
  window.dispatchEvent(new Event('behaatel:session-expired'))
  if (window.location.pathname !== '/login') window.location.assign('/login')
}
