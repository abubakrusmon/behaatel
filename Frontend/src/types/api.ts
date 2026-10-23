export type Role = 'superAdmin' | 'admin' | 'user'

export type Permission =
  | 'residentsView'
  | 'residentsManage'
  | 'roomsView'
  | 'roomsManage'
  | 'chargesView'
  | 'chargesManage'
  | 'paymentsView'
  | 'paymentsManage'
  | 'reportsView'
  | 'announcementsManage'

export type ResidentStatus = 'active' | 'checkedOut'
export type RoomType = 'standard' | 'comfort' | 'lux'
export type ChargeType = 'rent' | 'utilities' | 'penalty' | 'other'
export type ChargeStatus = 'unpaid' | 'partiallyPaid' | 'paid' | 'cancelled'
export type PaymentMethod = 'cash' | 'card' | 'bankTransfer' | 'online'
export type PaymentStatus = 'completed' | 'cancelled'
export type ApprovalStatus = 'pending' | 'approved' | 'rejected'

export interface DashboardSummaryDto {
  activeResidents: number
  totalRooms: number
  totalBeds: number
  occupiedBeds: number
  occupancyPercent: number
  incomeThisMonth: number
  incomeLastMonth: number
  chargedThisMonth: number
  totalDebt: number
  debtorsCount: number
}

export interface RevenuePointDto {
  period: string
  charged: number
  paid: number
}

export interface MethodShareDto {
  method: PaymentMethod
  totalAmount: number
  count: number
  percent: number
}

export interface BuildingOccupancyDto {
  buildingId: string
  buildingName: string
  totalBeds: number
  occupiedBeds: number
  occupancyPercent: number
}

export interface TopDebtorDto {
  residentId: string
  fullName: string
  buildingName: string | null
  roomNumber: string | null
  debtAmount: number
}

export interface RecentPaymentDto {
  id: string
  residentName: string
  amount: number
  method: PaymentMethod
  paidAt: string
  receiptNumber: string
}

export interface PaymentDto {
  id: string
  residentId: string
  residentName: string
  amount: number
  method: PaymentMethod
  status: PaymentStatus
  paidAt: string
  receiptNumber: string
  comment: string | null
  allocatedAmount: number
  unallocatedAmount: number
}

export interface DebtorDto {
  residentId: string
  fullName: string
  phone: string | null
  buildingName: string | null
  roomNumber: string | null
  debtAmount: number
  oldestUnpaidDueDate: string | null
  overdueDays: number
}

export interface ResidentListItemDto {
  id: string
  fullName: string
  email: string
  phone: string | null
  status: ResidentStatus
  buildingName: string | null
  roomNumber: string | null
  balance: number
  isActive: boolean
}

export interface CreatePaymentRequest {
  residentId: string
  amount: number
  method: PaymentMethod
  paidAt: string | null
  comment: string | null
}

export interface PaymentsQuery extends PaginationQuery {
  residentId?: string
  status?: PaymentStatus
  method?: PaymentMethod
  from?: string
  to?: string
  minAmount?: number
  maxAmount?: number
}

export type DebtorsQuery = PaginationQuery

export interface UserInfoDto {
  id: string
  email: string
  fullName: string
  role: Role
  permissions: Permission[]
  mustChangePassword: boolean
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserInfoDto
}

export interface LoginResultDto {
  requiresCode: boolean
  session: AuthResponse | null
  challengeId: string | null
  expiresAt: string | null
  maskedEmail: string | null
}

export interface AuthActionDto {
  message: string
}

export interface RegistrationRequestDto {
  userId: string
  fullName: string
  email: string
  phone: string | null
  requestedAt: string | null
  status: ApprovalStatus
  emailConfirmed: boolean
}

export interface SettingDto {
  key: string
  value: string
  updatedAt: string
}

export type RegistrationRequestsQuery = PaginationQuery & { status?: ApprovalStatus }

export interface PagedList<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Record<string, string[]>
}

export interface PaginationQuery {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDir?: 'asc' | 'desc'
}

export interface MyProfileDto {
  fullName: string
  email: string
  phone: string | null
  passportNumber: string
  organization: string | null
  checkInDate: string | null
  status: ResidentStatus
}

export interface MyRoomDto {
  buildingName: string
  address: string
  floor: number
  roomNumber: string
  type: RoomType
  neighbors: string[]
}

export interface MyLastPaymentDto {
  amount: number
  paidAt: string
}

export interface MySummaryDto {
  dueThisMonth: number
  balance: number
  totalDebt: number
  nextDueDate: string | null
  lastPayment: MyLastPaymentDto | null
}

export interface MyChargeDto {
  id: string
  period: string
  type: ChargeType
  description: string | null
  amount: number
  paidAmount: number
  remainingAmount: number
  dueDate: string
  status: ChargeStatus
}

export interface MyPaymentDto {
  id: string
  amount: number
  method: PaymentMethod
  paidAt: string
  receiptNumber: string
  status: PaymentStatus
}

export interface MonthlyPointDto {
  period: string
  charged: number
  paid: number
}

export interface MyAnnouncementDto {
  id: string
  title: string
  body: string
  publishedAt: string
}
