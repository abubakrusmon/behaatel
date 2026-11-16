import { apiClient } from '@/api/client'
import type { ResidentListItemDto, Permission, PagedList } from '@/types/api'
import type {
  AdminAnnouncementDto,
  AdminBuildingDto,
  AdminChargeDto,
  AdminResidentDto,
  AdminRoomDto,
  AdminNotificationRecipientDto,
  CreateResidentPayload,
  ManagementAdminsQuery,
  ManagementAnnouncementsQuery,
  ManagementChargesQuery,
  ManagementResidentsQuery,
  ManagementRoomsQuery,
  SuperAdminDto,
  NotificationAudienceKind,
  NotificationSendResultDto,
  TelegramQueueStatus,
  UpdateResidentPayload,
} from '@/types/admin'

export function getManagementResidents(query: ManagementResidentsQuery): Promise<PagedList<ResidentListItemDto>> {
  return apiClient.get<PagedList<ResidentListItemDto>>('/admin/residents', { params: query }).then(({ data }) => data)
}

export function getManagementResident(id: string): Promise<AdminResidentDto> {
  return apiClient.get<AdminResidentDto>(`/admin/residents/${id}`).then(({ data }) => data)
}

export function createManagementResident(payload: CreateResidentPayload): Promise<ResidentListItemDto> {
  return apiClient.post<ResidentListItemDto>('/admin/residents', payload).then(({ data }) => data)
}

export async function updateManagementResident(id: string, payload: UpdateResidentPayload): Promise<void> {
  await apiClient.put(`/admin/residents/${id}`, payload)
}

export async function deleteManagementResident(id: string): Promise<void> {
  await apiClient.delete(`/admin/residents/${id}`)
}

export async function checkInResident(id: string, roomId: string, date: string): Promise<void> {
  await apiClient.post(`/admin/residents/${id}/check-in`, { roomId, date })
}

export async function checkOutResident(id: string, date: string): Promise<void> {
  await apiClient.post(`/admin/residents/${id}/check-out`, { date })
}

export async function setResidentBlocked(id: string, blocked: boolean): Promise<void> {
  await apiClient.post(`/admin/residents/${id}/${blocked ? 'block' : 'unblock'}`)
}

export async function resetResidentPassword(id: string, newPassword: string): Promise<void> {
  await apiClient.post(`/admin/residents/${id}/reset-password`, { newPassword })
}

export function getManagementBuildings(): Promise<AdminBuildingDto[]> {
  return apiClient.get<AdminBuildingDto[]>('/admin/buildings').then(({ data }) => data)
}

export function createManagementBuilding(payload: { name: string; address: string; floorsCount: number }): Promise<AdminBuildingDto> {
  return apiClient.post<AdminBuildingDto>('/admin/buildings', payload).then(({ data }) => data)
}

export async function updateManagementBuilding(id: string, payload: { name: string; address: string; floorsCount: number }): Promise<void> {
  await apiClient.put(`/admin/buildings/${id}`, payload)
}

export async function deleteManagementBuilding(id: string): Promise<void> {
  await apiClient.delete(`/admin/buildings/${id}`)
}

export function getManagementRooms(query: ManagementRoomsQuery): Promise<PagedList<AdminRoomDto>> {
  return apiClient.get<PagedList<AdminRoomDto>>('/admin/rooms', { params: query }).then(({ data }) => data)
}

export function createManagementRoom(payload: { buildingId: string; number: string; floor: number; capacity: number; pricePerBed: number; type: AdminRoomDto['type'] }): Promise<AdminRoomDto> {
  return apiClient.post<AdminRoomDto>('/admin/rooms', payload).then(({ data }) => data)
}

export async function updateManagementRoom(id: string, payload: { number: string; floor: number; capacity: number; pricePerBed: number; type: AdminRoomDto['type'] }): Promise<void> {
  await apiClient.put(`/admin/rooms/${id}`, payload)
}

export async function deleteManagementRoom(id: string): Promise<void> {
  await apiClient.delete(`/admin/rooms/${id}`)
}

export function getManagementCharges(query: ManagementChargesQuery): Promise<PagedList<AdminChargeDto>> {
  return apiClient.get<PagedList<AdminChargeDto>>('/admin/charges', { params: query }).then(({ data }) => data)
}

export function createManagementCharge(payload: { residentId: string; period: string; type: AdminChargeDto['type']; amount: number; dueDate: string; description: string | null }): Promise<AdminChargeDto> {
  return apiClient.post<AdminChargeDto>('/admin/charges', payload).then(({ data }) => data)
}

export async function cancelManagementCharge(id: string, reason: string): Promise<void> {
  await apiClient.post(`/admin/charges/${id}/cancel`, { reason })
}

export function generateManagementCharges(period: string): Promise<{ created: number; skipped: number }> {
  return apiClient.post<{ created: number; skipped: number }>('/admin/charges/generate', undefined, { params: { period } }).then(({ data }) => data)
}

export function getManagementAnnouncements(query: ManagementAnnouncementsQuery): Promise<PagedList<AdminAnnouncementDto>> {
  return apiClient.get<PagedList<AdminAnnouncementDto>>('/admin/announcements', { params: query }).then(({ data }) => data)
}

export function createManagementAnnouncement(payload: {
  title: string
  body: string
  publishNow: boolean
  sendTelegram: boolean
  audienceKind: NotificationAudienceKind
  buildingId: string | null
  residentIds: string[]
}): Promise<AdminAnnouncementDto> {
  return apiClient.post<AdminAnnouncementDto>('/admin/announcements', payload).then(({ data }) => data)
}

export async function updateManagementAnnouncement(id: string, payload: { title: string; body: string }): Promise<void> {
  await apiClient.put(`/admin/announcements/${id}`, payload)
}

export async function publishManagementAnnouncement(id: string, sendTelegram = true): Promise<{ telegramStatus: TelegramQueueStatus | null }> {
  const { data } = await apiClient.post<{ telegramStatus: TelegramQueueStatus | null }>(
    `/admin/announcements/${id}/publish`,
    undefined,
    { params: { sendTelegram } },
  )
  return data
}

export async function deleteManagementAnnouncement(id: string): Promise<void> {
  await apiClient.delete(`/admin/announcements/${id}`)
}

export async function getNotificationRecipients(): Promise<AdminNotificationRecipientDto[]> {
  const { data } = await apiClient.get<AdminNotificationRecipientDto[]>('/admin/notifications/recipients')
  return data
}

export async function createManagementNotification(payload: {
  title: string
  body: string
  audienceKind: NotificationAudienceKind
  buildingId: string | null
  residentIds: string[]
  sendTelegram: boolean
}): Promise<NotificationSendResultDto> {
  const { data } = await apiClient.post<NotificationSendResultDto>('/admin/notifications', payload)
  return data
}

export async function testTelegram(): Promise<{ telegramStatus: TelegramQueueStatus }> {
  const { data } = await apiClient.post<{ telegramStatus: TelegramQueueStatus }>('/admin/telegram/test')
  return data
}

export function getManagementAdmins(query: ManagementAdminsQuery): Promise<PagedList<SuperAdminDto>> {
  return apiClient.get<PagedList<SuperAdminDto>>('/superadmin/admins', { params: query }).then(({ data }) => data)
}

export function createManagementAdmin(payload: { email: string; fullName: string; phone: string | null; password: string; permissions: Permission[] }): Promise<SuperAdminDto> {
  return apiClient.post<SuperAdminDto>('/superadmin/admins', payload).then(({ data }) => data)
}

export async function updateManagementAdminPermissions(id: string, permissions: Permission[]): Promise<void> {
  await apiClient.put(`/superadmin/admins/${id}/permissions`, { permissions })
}

export async function setManagementAdminBlocked(id: string, blocked: boolean): Promise<void> {
  await apiClient.post(`/superadmin/admins/${id}/${blocked ? 'block' : 'unblock'}`)
}

export async function resetManagementAdminPassword(id: string, newPassword: string): Promise<void> {
  await apiClient.post(`/superadmin/admins/${id}/reset-password`, { newPassword })
}

export async function deleteManagementAdmin(id: string): Promise<void> {
  await apiClient.delete(`/superadmin/admins/${id}`)
}
