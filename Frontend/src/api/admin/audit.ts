import { apiClient } from '@/api/client'
import type { AuditLogDto } from '@/types/admin'
import type { PagedList } from '@/types/api'

export interface AuditLogsQuery {
  page: number
  pageSize: number
  search?: string
  action?: string
  entityName?: string
  from?: string
  to?: string
  sortDir?: 'asc' | 'desc'
}

export function getAuditLogs(query: AuditLogsQuery): Promise<PagedList<AuditLogDto>> {
  return apiClient.get<PagedList<AuditLogDto>>('/admin/audit-logs', { params: query }).then(({ data }) => data)
}
