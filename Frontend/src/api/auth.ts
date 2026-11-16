import { apiClient } from '@/api/client'
import type { AuthActionDto, AuthResponse, LoginResultDto, TelegramLinkPollDto, UserInfoDto } from '@/types/api'

export interface LoginRequest {
  email: string
  password: string
}

export interface RefreshRequest {
  refreshToken: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export async function login(request: LoginRequest): Promise<LoginResultDto> {
  const { data } = await apiClient.post<LoginResultDto>('/auth/login', request)
  return data
}

export async function verifyLoginCode(challengeId: string, code: string): Promise<LoginResultDto> {
  const { data } = await apiClient.post<LoginResultDto>('/auth/login/verify-code', { challengeId, code })
  return data
}

export async function pollTelegramLink(attemptId: string, pollToken: string): Promise<TelegramLinkPollDto> {
  const { data } = await apiClient.post<TelegramLinkPollDto>('/auth/telegram-link/status', { attemptId, pollToken })
  return data
}

export async function resendLoginCode(challengeId: string): Promise<AuthActionDto> {
  const { data } = await apiClient.post<AuthActionDto>('/auth/login/resend-code', { challengeId })
  return data
}

export interface RegisterRequest {
  email: string
  fullName: string
  phone: string | null
  password: string
  passportNumber: string
  birthDate: string
  organization: string | null
}

export async function register(request: RegisterRequest): Promise<AuthActionDto> {
  const { data } = await apiClient.post<AuthActionDto>('/auth/register', request)
  return data
}

export async function verifyEmail(email: string, code: string): Promise<AuthActionDto> {
  const { data } = await apiClient.post<AuthActionDto>('/auth/verify-email', { email, code })
  return data
}

export async function resendVerification(email: string): Promise<AuthActionDto> {
  const { data } = await apiClient.post<AuthActionDto>('/auth/resend-verification', { email })
  return data
}

export async function forgotPassword(email: string): Promise<AuthActionDto> {
  const { data } = await apiClient.post<AuthActionDto>('/auth/forgot-password', { email })
  return data
}

export async function resetPassword(email: string, code: string, newPassword: string): Promise<AuthActionDto> {
  const { data } = await apiClient.post<AuthActionDto>('/auth/reset-password', { email, code, newPassword })
  return data
}

export async function refresh(request: RefreshRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/refresh', request)
  return data
}

export async function logout(request: RefreshRequest): Promise<void> {
  await apiClient.post('/auth/logout', request)
}

export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await apiClient.post('/auth/change-password', request)
}

export async function getMe(): Promise<UserInfoDto> {
  const { data } = await apiClient.get<UserInfoDto>('/auth/me')
  return data
}

export async function updateAvatar(avatarUrl: string | null): Promise<UserInfoDto> {
  const { data } = await apiClient.put<UserInfoDto>('/auth/avatar', { avatarUrl })
  return data
}
