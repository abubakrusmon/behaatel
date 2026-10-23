import { apiClient } from '@/api/client'
import type { ApprovalStatus, PagedList, RegistrationRequestDto, RegistrationRequestsQuery, SettingDto } from '@/types/api'

export async function getRegistrationRequests(query: RegistrationRequestsQuery): Promise<PagedList<RegistrationRequestDto>> {
  const { data } = await apiClient.get<PagedList<RegistrationRequestDto>>('/admin/registrations', { params: query })
  return data
}

export async function approveRegistration(userId: string): Promise<void> {
  await apiClient.post(`/admin/registrations/${userId}/approve`)
}

export async function rejectRegistration(userId: string, reason: string | null): Promise<void> {
  await apiClient.post(`/admin/registrations/${userId}/reject`, { reason })
}

export async function getSettings(): Promise<SettingDto[]> {
  const { data } = await apiClient.get<SettingDto[]>('/admin/settings')
  return data
}

export async function updateSetting(key: string, value: string): Promise<SettingDto> {
  const { data } = await apiClient.put<SettingDto>('/admin/settings', { key, value })
  return data
}

export const approvalStatuses: ApprovalStatus[] = ['pending', 'approved', 'rejected']
