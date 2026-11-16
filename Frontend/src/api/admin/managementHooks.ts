import { useMutation, useQuery } from '@tanstack/react-query'
import {
  cancelManagementCharge,
  checkInResident,
  checkOutResident,
  createManagementAdmin,
  createManagementAnnouncement,
  createManagementBuilding,
  createManagementCharge,
  createManagementRoom,
  createManagementResident,
  deleteManagementAdmin,
  deleteManagementAnnouncement,
  deleteManagementBuilding,
  deleteManagementResident,
  deleteManagementRoom,
  generateManagementCharges,
  getManagementAdmins,
  getManagementAnnouncements,
  getManagementBuildings,
  getManagementCharges,
  getManagementResident,
  getManagementResidents,
  getManagementRooms,
  publishManagementAnnouncement,
  resetManagementAdminPassword,
  resetResidentPassword,
  setManagementAdminBlocked,
  setResidentBlocked,
  updateManagementAdminPermissions,
  updateManagementAnnouncement,
  updateManagementBuilding,
  updateManagementRoom,
  updateManagementResident,
} from '@/api/admin/management'
import { queryClient } from '@/api/queryClient'
import type { Permission } from '@/types/api'
import type {
  ManagementAdminsQuery,
  ManagementAnnouncementsQuery,
  ManagementChargesQuery,
  ManagementResidentsQuery,
  ManagementRoomsQuery,
  UpdateResidentPayload,
} from '@/types/admin'

export const managementKeys = {
  residents: ['admin', 'management-residents'] as const,
  resident: (id: string) => ['admin', 'management-resident', id] as const,
  buildings: ['admin', 'management-buildings'] as const,
  rooms: ['admin', 'management-rooms'] as const,
  roomList: (query: ManagementRoomsQuery) => [...managementKeys.rooms, query] as const,
  charges: ['admin', 'management-charges'] as const,
  chargeList: (query: ManagementChargesQuery) => [...managementKeys.charges, query] as const,
  announcements: ['admin', 'management-announcements'] as const,
  announcementList: (query: ManagementAnnouncementsQuery) => [...managementKeys.announcements, query] as const,
  admins: ['superadmin', 'management-admins'] as const,
  adminList: (query: ManagementAdminsQuery) => [...managementKeys.admins, query] as const,
}

export const useManagementResidents = (query: ManagementResidentsQuery) => useQuery({
  queryKey: [...managementKeys.residents, query],
  queryFn: () => getManagementResidents(query),
})

export const useManagementResident = (id: string, enabled: boolean) => useQuery({
  queryKey: managementKeys.resident(id),
  queryFn: () => getManagementResident(id),
  enabled,
})

export const useManagementBuildings = () => useQuery({
  queryKey: managementKeys.buildings,
  queryFn: getManagementBuildings,
})

export const useManagementRooms = (query: ManagementRoomsQuery) => useQuery({
  queryKey: managementKeys.roomList(query),
  queryFn: () => getManagementRooms(query),
})

export const useManagementCharges = (query: ManagementChargesQuery) => useQuery({
  queryKey: managementKeys.chargeList(query),
  queryFn: () => getManagementCharges(query),
})

export const useManagementAnnouncements = (query: ManagementAnnouncementsQuery) => useQuery({
  queryKey: managementKeys.announcementList(query),
  queryFn: () => getManagementAnnouncements(query),
})

export const useManagementAdmins = (query: ManagementAdminsQuery) => useQuery({
  queryKey: managementKeys.adminList(query),
  queryFn: () => getManagementAdmins(query),
})

const invalidate = (...keys: readonly (readonly unknown[])[]) => Promise.all(
  keys.map((queryKey) => queryClient.invalidateQueries({ queryKey })),
)

export const useCreateManagementResident = () => useMutation({
  mutationFn: createManagementResident,
  onSuccess: () => invalidate(managementKeys.residents, managementKeys.buildings, managementKeys.rooms),
})
export const useUpdateManagementResident = () => useMutation({
  mutationFn: ({ id, payload }: { id: string; payload: UpdateResidentPayload }) =>
    updateManagementResident(id, payload),
  onSuccess: (_data, variables) => invalidate(managementKeys.residents, managementKeys.resident(variables.id)),
})
export const useDeleteManagementResident = () => useMutation({
  mutationFn: deleteManagementResident,
  onSuccess: () => invalidate(managementKeys.residents),
})
export const useCheckInResident = () => useMutation({
  mutationFn: ({ id, roomId, date }: { id: string; roomId: string; date: string }) => checkInResident(id, roomId, date),
  onSuccess: () => invalidate(managementKeys.residents, managementKeys.buildings, managementKeys.rooms),
})
export const useCheckOutResident = () => useMutation({
  mutationFn: ({ id, date }: { id: string; date: string }) => checkOutResident(id, date),
  onSuccess: () => invalidate(managementKeys.residents, managementKeys.buildings, managementKeys.rooms),
})
export const useSetResidentBlocked = () => useMutation({
  mutationFn: ({ id, blocked }: { id: string; blocked: boolean }) => setResidentBlocked(id, blocked),
  onSuccess: () => invalidate(managementKeys.residents),
})
export const useResetResidentPassword = () => useMutation({
  mutationFn: ({ id, newPassword }: { id: string; newPassword: string }) => resetResidentPassword(id, newPassword),
})

export const useCreateManagementBuilding = () => useMutation({
  mutationFn: createManagementBuilding,
  onSuccess: () => invalidate(managementKeys.buildings),
})
export const useUpdateManagementBuilding = () => useMutation({
  mutationFn: ({ id, payload }: { id: string; payload: Parameters<typeof updateManagementBuilding>[1] }) => updateManagementBuilding(id, payload),
  onSuccess: () => invalidate(managementKeys.buildings),
})
export const useDeleteManagementBuilding = () => useMutation({
  mutationFn: deleteManagementBuilding,
  onSuccess: () => invalidate(managementKeys.buildings),
})
export const useCreateManagementRoom = () => useMutation({
  mutationFn: createManagementRoom,
  onSuccess: () => invalidate(managementKeys.rooms, managementKeys.buildings),
})
export const useUpdateManagementRoom = () => useMutation({
  mutationFn: ({ id, payload }: { id: string; payload: Parameters<typeof updateManagementRoom>[1] }) => updateManagementRoom(id, payload),
  onSuccess: () => invalidate(managementKeys.rooms),
})
export const useDeleteManagementRoom = () => useMutation({
  mutationFn: deleteManagementRoom,
  onSuccess: () => invalidate(managementKeys.rooms, managementKeys.buildings),
})

export const useCreateManagementCharge = () => useMutation({
  mutationFn: createManagementCharge,
  onSuccess: () => invalidate(managementKeys.charges),
})
export const useCancelManagementCharge = () => useMutation({
  mutationFn: ({ id, reason }: { id: string; reason: string }) => cancelManagementCharge(id, reason),
  onSuccess: () => invalidate(managementKeys.charges),
})
export const useGenerateManagementCharges = () => useMutation({
  mutationFn: generateManagementCharges,
  onSuccess: () => invalidate(managementKeys.charges),
})

export const useCreateManagementAnnouncement = () => useMutation({
  mutationFn: createManagementAnnouncement,
  onSuccess: () => invalidate(managementKeys.announcements),
})
export const useUpdateManagementAnnouncement = () => useMutation({
  mutationFn: ({ id, payload }: { id: string; payload: { title: string; body: string } }) => updateManagementAnnouncement(id, payload),
  onSuccess: () => invalidate(managementKeys.announcements),
})
export const usePublishManagementAnnouncement = () => useMutation({
  mutationFn: ({ id, sendTelegram }: { id: string; sendTelegram: boolean }) =>
    publishManagementAnnouncement(id, sendTelegram),
  onSuccess: () => invalidate(managementKeys.announcements),
})
export const useDeleteManagementAnnouncement = () => useMutation({
  mutationFn: deleteManagementAnnouncement,
  onSuccess: () => invalidate(managementKeys.announcements),
})

export const useCreateManagementAdmin = () => useMutation({
  mutationFn: createManagementAdmin,
  onSuccess: () => invalidate(managementKeys.admins),
})
export const useUpdateManagementAdminPermissions = () => useMutation({
  mutationFn: ({ id, permissions }: { id: string; permissions: Permission[] }) => updateManagementAdminPermissions(id, permissions),
  onSuccess: () => invalidate(managementKeys.admins),
})
export const useSetManagementAdminBlocked = () => useMutation({
  mutationFn: ({ id, blocked }: { id: string; blocked: boolean }) => setManagementAdminBlocked(id, blocked),
  onSuccess: () => invalidate(managementKeys.admins),
})
export const useResetManagementAdminPassword = () => useMutation({
  mutationFn: ({ id, newPassword }: { id: string; newPassword: string }) => resetManagementAdminPassword(id, newPassword),
})
export const useDeleteManagementAdmin = () => useMutation({
  mutationFn: deleteManagementAdmin,
  onSuccess: () => invalidate(managementKeys.admins),
})
