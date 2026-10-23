import { useMutation, useQuery } from '@tanstack/react-query'
import {
  cancelAdminPayment,
  createAdminPayment,
  getAdminDebtors,
  getAdminPayments,
  getAdminResidents,
  getBuildingOccupancy,
  getDashboardSummary,
  getPaymentMethods,
  getRecentPayments,
  getRevenueByMonth,
  getTopDebtors,
} from '@/api/admin/admin'
import { queryClient } from '@/api/queryClient'
import type { CreatePaymentRequest, DebtorsQuery, PaymentsQuery } from '@/types/api'

export const adminQueryKeys = {
  all: ['admin'] as const,
  dashboard: () => [...adminQueryKeys.all, 'dashboard'] as const,
  summary: () => [...adminQueryKeys.dashboard(), 'summary'] as const,
  revenue: (months: number) => [...adminQueryKeys.dashboard(), 'revenue', months] as const,
  paymentMethods: (from: string, to: string) => [...adminQueryKeys.dashboard(), 'payment-methods', from, to] as const,
  occupancy: () => [...adminQueryKeys.dashboard(), 'occupancy'] as const,
  topDebtors: (limit: number) => [...adminQueryKeys.dashboard(), 'top-debtors', limit] as const,
  recentPayments: (limit: number) => [...adminQueryKeys.dashboard(), 'recent-payments', limit] as const,
  payments: () => [...adminQueryKeys.all, 'payments'] as const,
  paymentList: (query: PaymentsQuery) => [...adminQueryKeys.payments(), query] as const,
  debtors: () => [...adminQueryKeys.all, 'debtors'] as const,
  debtorList: (query: DebtorsQuery) => [...adminQueryKeys.debtors(), query] as const,
  residents: () => [...adminQueryKeys.all, 'resident-options'] as const,
  residentOptions: (search: string) => [...adminQueryKeys.residents(), search] as const,
}

export function useDashboardSummary() {
  return useQuery({
    queryKey: adminQueryKeys.summary(),
    queryFn: getDashboardSummary,
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useRevenueByMonth(months: number) {
  return useQuery({
    queryKey: adminQueryKeys.revenue(months),
    queryFn: () => getRevenueByMonth(months),
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useDashboardPaymentMethods(from: string, to: string) {
  return useQuery({
    queryKey: adminQueryKeys.paymentMethods(from, to),
    queryFn: () => getPaymentMethods(from, to),
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useBuildingOccupancy() {
  return useQuery({
    queryKey: adminQueryKeys.occupancy(),
    queryFn: getBuildingOccupancy,
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useTopDebtors(limit = 10) {
  return useQuery({
    queryKey: adminQueryKeys.topDebtors(limit),
    queryFn: () => getTopDebtors(limit),
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useRecentPayments(limit = 10) {
  return useQuery({
    queryKey: adminQueryKeys.recentPayments(limit),
    queryFn: () => getRecentPayments(limit),
    refetchInterval: 60_000,
    refetchIntervalInBackground: false,
  })
}

export function useAdminPayments(query: PaymentsQuery) {
  return useQuery({
    queryKey: adminQueryKeys.paymentList(query),
    queryFn: () => getAdminPayments(query),
  })
}

export function useAdminDebtors(query: DebtorsQuery) {
  return useQuery({
    queryKey: adminQueryKeys.debtorList(query),
    queryFn: () => getAdminDebtors(query),
  })
}

export function useAdminResidents(search: string, enabled = true) {
  return useQuery({
    queryKey: adminQueryKeys.residentOptions(search),
    queryFn: () => getAdminResidents(search),
    enabled,
  })
}

function invalidatePaymentData() {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: adminQueryKeys.payments() }),
    queryClient.invalidateQueries({ queryKey: adminQueryKeys.debtors() }),
    queryClient.invalidateQueries({ queryKey: adminQueryKeys.dashboard() }),
  ])
}

export function useCreateAdminPayment() {
  return useMutation({
    mutationFn: (request: CreatePaymentRequest) => createAdminPayment(request),
    onSuccess: invalidatePaymentData,
  })
}

export function useCancelAdminPayment() {
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => cancelAdminPayment(id, reason),
    onSuccess: invalidatePaymentData,
  })
}
