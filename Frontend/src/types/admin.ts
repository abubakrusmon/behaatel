import type {
  ChargeStatus,
  ChargeType,
  Permission,
  ResidentStatus,
  RoomType,
} from '@/types/api'
import type { PaginationQuery } from '@/types/api'

export interface AdminResidentDto {
  id: string
  fullName: string
  email: string
  phone: string | null
  passportNumber: string
  birthDate: string
  organization: string | null
  status: ResidentStatus
  isActive: boolean
  checkInDate: string | null
  checkOutDate: string | null
  currentRoom: { buildingName: string; roomNumber: string; floor: number; pricePerBed: number } | null
  balance: number
  totalCharged: number
  totalPaid: number
  occupancyHistory: { roomNumber: string; buildingName: string; from: string; to: string | null }[]
}

export interface AdminBuildingDto {
  id: string
  name: string
  address: string
  floorsCount: number
  totalRooms: number
  totalCapacity: number
  occupiedBeds: number
}

export interface AdminRoomDto {
  id: string
  buildingId: string
  buildingName: string
  number: string
  floor: number
  capacity: number
  occupiedBeds: number
  freeBeds: number
  pricePerBed: number
  type: RoomType
}

export interface AdminChargeDto {
  id: string
  residentId: string
  residentName: string
  period: string
  type: ChargeType
  amount: number
  paidAmount: number
  remainingAmount: number
  dueDate: string
  status: ChargeStatus
}

export interface AuditLogDto {
  id: string
  userId: string | null
  userName: string | null
  action: string
  entityName: string
  entityId: string | null
  oldValues: string | null
  newValues: string | null
  ipAddress: string | null
  createdAt: string
}

export interface AdminAnnouncementDto {
  id: string
  title: string
  body: string
  publishedAt: string | null
  authorName: string
  createdAt: string
  audienceKind?: NotificationAudienceKind
  buildingId?: string | null
  residentIds?: string[] | null
  telegramStatus?: TelegramQueueStatus | null
}

export type TelegramQueueStatus = 'queued' | 'notConfigured'

export interface AdminNotificationRecipientDto {
  residentId: string
  fullName: string
  buildingId: string
  buildingName: string
  telegramConnected: boolean
}

export interface NotificationSendResultDto {
  id: string
  recipientCount: number
  telegramStatus: TelegramQueueStatus | null
}

export type NotificationAudienceKind = 'allResidents' | 'building' | 'selectedResidents'

export interface SuperAdminDto {
  id: string
  email: string
  fullName: string
  phone: string | null
  isActive: boolean
  permissions: Permission[]
  lastLoginAt: string | null
  createdAt: string
}

export interface ManagementResidentsQuery extends PaginationQuery {
  status?: ResidentStatus
  buildingId?: string
  roomId?: string
  hasDebt?: boolean
}

export interface ManagementRoomsQuery extends PaginationQuery {
  buildingId?: string
  floor?: number
  type?: RoomType
  onlyWithFreeBeds?: boolean
}

export interface ManagementChargesQuery extends PaginationQuery {
  residentId?: string
  period?: string
  status?: ChargeStatus
  type?: ChargeType
}

export interface ManagementAnnouncementsQuery extends PaginationQuery {
  isPublished?: boolean
}

export interface ManagementAdminsQuery extends PaginationQuery {
  isActive?: boolean
}

export interface CreateResidentPayload {
  email: string
  fullName: string
  phone: string | null
  password: string
  passportNumber: string
  birthDate: string
  organization: string | null
  roomId: string | null
  checkInDate: string | null
}

export interface UpdateResidentPayload {
  fullName: string
  phone: string | null
  passportNumber: string
  birthDate: string
  organization: string | null
}
