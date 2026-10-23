import { apiClient } from '@/api/client'
import type {
  BuildingOccupancyDto,
  CreatePaymentRequest,
  DashboardSummaryDto,
  DebtorDto,
  DebtorsQuery,
  MethodShareDto,
  PagedList,
  PaymentsQuery,
  PaymentDto,
  RecentPaymentDto,
  ResidentListItemDto,
  RevenuePointDto,
  TopDebtorDto,
} from '@/types/api'

export async function getDashboardSummary(): Promise<DashboardSummaryDto> {
  const { data } = await apiClient.get<DashboardSummaryDto>('/admin/dashboard')
  return data
}

export async function getRevenueByMonth(months: number): Promise<RevenuePointDto[]> {
  const { data } = await apiClient.get<RevenuePointDto[]>('/admin/dashboard/charts/revenue', { params: { months } })
  return data
}

export async function getPaymentMethods(from: string, to: string): Promise<MethodShareDto[]> {
  const { data } = await apiClient.get<MethodShareDto[]>('/admin/dashboard/charts/payment-methods', { params: { from, to } })
  return data
}

export async function getBuildingOccupancy(): Promise<BuildingOccupancyDto[]> {
  const { data } = await apiClient.get<BuildingOccupancyDto[]>('/admin/dashboard/charts/occupancy')
  return data
}

export async function getTopDebtors(limit = 10): Promise<TopDebtorDto[]> {
  const { data } = await apiClient.get<TopDebtorDto[]>('/admin/dashboard/top-debtors', { params: { limit } })
  return data
}

export async function getRecentPayments(limit = 10): Promise<RecentPaymentDto[]> {
  const { data } = await apiClient.get<RecentPaymentDto[]>('/admin/dashboard/recent-payments', { params: { limit } })
  return data
}

export async function getAdminPayments(query: PaymentsQuery): Promise<PagedList<PaymentDto>> {
  const { data } = await apiClient.get<PagedList<PaymentDto>>('/admin/payments', { params: query })
  return data
}

export async function createAdminPayment(request: CreatePaymentRequest): Promise<PaymentDto> {
  const { data } = await apiClient.post<PaymentDto>('/admin/payments', request)
  return data
}

export async function cancelAdminPayment(id: string, reason: string): Promise<void> {
  await apiClient.post(`/admin/payments/${id}/cancel`, { reason })
}

export async function getAdminDebtors(query: DebtorsQuery): Promise<PagedList<DebtorDto>> {
  const { data } = await apiClient.get<PagedList<DebtorDto>>('/admin/debtors', { params: query })
  return data
}

export async function exportAdminPayments(query: PaymentsQuery): Promise<Blob> {
  const { data } = await apiClient.get<Blob>('/admin/payments/export', {
    params: query,
    responseType: 'blob',
  })
  return data
}

export async function getAdminResidents(search: string): Promise<PagedList<ResidentListItemDto>> {
  const { data } = await apiClient.get<PagedList<ResidentListItemDto>>('/admin/residents', {
    params: { page: 1, pageSize: 10, search: search || undefined, status: 'active' },
  })
  return data
}
