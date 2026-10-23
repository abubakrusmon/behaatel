import { useQuery } from '@tanstack/react-query'
import {
  getMyAnnouncements,
  getMyCharges,
  getMyMonthlyChart,
  getMyPayments,
  getMyProfile,
  getMyRoom,
  getMySummary,
  type MyChargesQuery,
  type MyPaymentsQuery,
} from '@/api/me'
import type { PaginationQuery } from '@/types/api'

export const meQueryKeys = {
  all: ['me'] as const,
  profile: () => [...meQueryKeys.all, 'profile'] as const,
  room: () => [...meQueryKeys.all, 'room'] as const,
  summary: () => [...meQueryKeys.all, 'summary'] as const,
  charges: (query: MyChargesQuery) => [...meQueryKeys.all, 'charges', query] as const,
  payments: (query: MyPaymentsQuery) => [...meQueryKeys.all, 'payments', query] as const,
  monthly: (months: number) => [...meQueryKeys.all, 'monthly', months] as const,
  announcements: (query: PaginationQuery) => [...meQueryKeys.all, 'announcements', query] as const,
}

export function useMyProfile() {
  return useQuery({ queryKey: meQueryKeys.profile(), queryFn: getMyProfile })
}

export function useMyRoom() {
  return useQuery({ queryKey: meQueryKeys.room(), queryFn: getMyRoom })
}

export function useMySummary() {
  return useQuery({ queryKey: meQueryKeys.summary(), queryFn: getMySummary })
}

export function useMyCharges(query: MyChargesQuery) {
  return useQuery({ queryKey: meQueryKeys.charges(query), queryFn: () => getMyCharges(query) })
}

export function useMyPayments(query: MyPaymentsQuery) {
  return useQuery({ queryKey: meQueryKeys.payments(query), queryFn: () => getMyPayments(query) })
}

export function useMyMonthlyChart(months: number) {
  return useQuery({ queryKey: meQueryKeys.monthly(months), queryFn: () => getMyMonthlyChart(months) })
}

export function useMyAnnouncements(query: PaginationQuery) {
  return useQuery({ queryKey: meQueryKeys.announcements(query), queryFn: () => getMyAnnouncements(query) })
}
