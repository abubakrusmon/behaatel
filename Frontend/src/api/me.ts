import { apiClient } from '@/api/client'
import type {
  MonthlyPointDto,
  MyAnnouncementDto,
  MyChargeDto,
  MyPaymentDto,
  MyProfileDto,
  MyRoomDto,
  MySummaryDto,
  PagedList,
  PaginationQuery,
} from '@/types/api'
import type { ChargeStatus } from '@/types/api'

export type MyChargesQuery = PaginationQuery & {
  period?: string
  status?: ChargeStatus
}

export type MyPaymentsQuery = PaginationQuery & {
  from?: string
  to?: string
}

export async function getMyProfile(): Promise<MyProfileDto> {
  const { data } = await apiClient.get<MyProfileDto>('/me')
  return data
}

export async function getMyRoom(): Promise<MyRoomDto | null> {
  const { data } = await apiClient.get<MyRoomDto | null>('/me/room')
  return data
}

export async function getMySummary(): Promise<MySummaryDto> {
  const { data } = await apiClient.get<MySummaryDto>('/me/summary')
  return data
}

export async function getMyCharges(query: MyChargesQuery): Promise<PagedList<MyChargeDto>> {
  const { data } = await apiClient.get<PagedList<MyChargeDto>>('/me/charges', { params: query })
  return data
}

export async function getMyPayments(query: MyPaymentsQuery): Promise<PagedList<MyPaymentDto>> {
  const { data } = await apiClient.get<PagedList<MyPaymentDto>>('/me/payments', { params: query })
  return data
}

export async function getMyMonthlyChart(months: number): Promise<MonthlyPointDto[]> {
  const { data } = await apiClient.get<MonthlyPointDto[]>('/me/charts/monthly', { params: { months } })
  return data
}

export async function getMyAnnouncements(query: PaginationQuery): Promise<PagedList<MyAnnouncementDto>> {
  const { data } = await apiClient.get<PagedList<MyAnnouncementDto>>('/me/announcements', { params: query })
  return data
}
