import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { getAuditLogs } from '@/api/admin/audit'
import { DataTable } from '@/components/shared/DataTable'
import { DateRangeFields } from '@/components/shared/DatePicker'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { FilterBar } from '@/components/shared/FilterBar'
import { PageHeader } from '@/components/shared/PageHeader'
import { Input } from '@/components/ui/input'
import { t } from '@/i18n/ru'
import { formatDateTime } from '@/lib/format'
import type { AuditLogDto } from '@/types/admin'

function prettyJson(value: string | null) {
  if (!value) return null
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

function Details({ log }: { log: AuditLogDto }) {
  const oldValues = prettyJson(log.oldValues)
  const newValues = prettyJson(log.newValues)
  if (!oldValues && !newValues) return <span className="text-muted-foreground">—</span>
  return (
    <details className="max-w-xs text-left">
      <summary className="cursor-pointer text-sm text-primary">{t('adminAudit.details')}</summary>
      <div className="mt-2 space-y-2">
        {oldValues && <pre className="max-h-40 overflow-auto rounded-lg bg-muted p-2 text-xs">{oldValues}</pre>}
        {newValues && <pre className="max-h-40 overflow-auto rounded-lg bg-muted p-2 text-xs">{newValues}</pre>}
      </div>
    </details>
  )
}

export function AdminAuditPage() {
  const [page, setPage] = useState(1)
  const [searchDraft, setSearchDraft] = useState('')
  const [search, setSearch] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  useEffect(() => {
    const timer = window.setTimeout(() => { setSearch(searchDraft.trim()); setPage(1) }, 350)
    return () => window.clearTimeout(timer)
  }, [searchDraft])

  const query = { page, pageSize: 20, search: search || undefined, from: from ? `${from}T00:00:00Z` : undefined, to: to ? `${to}T23:59:59Z` : undefined, sortDir: 'desc' as const }
  const logs = useQuery({ queryKey: ['admin', 'audit-logs', query], queryFn: () => getAuditLogs(query), placeholderData: keepPreviousData })

  const columns = useMemo<ColumnDef<AuditLogDto, unknown>[]>(() => [
    { accessorKey: 'createdAt', header: t('adminAudit.when'), enableSorting: false, cell: ({ row }) => <span className="whitespace-nowrap">{formatDateTime(row.original.createdAt)}</span> },
    { accessorKey: 'userName', header: t('adminAudit.user'), enableSorting: false, cell: ({ row }) => row.original.userName ?? t('adminAudit.system') },
    { accessorKey: 'action', header: t('adminAudit.action'), enableSorting: false, cell: ({ row }) => <span className="font-medium">{row.original.action}</span> },
    { accessorKey: 'entityName', header: t('adminAudit.entity'), enableSorting: false },
    { accessorKey: 'ipAddress', header: t('adminAudit.ip'), enableSorting: false, cell: ({ row }) => row.original.ipAddress ?? '—' },
    { id: 'details', header: t('adminAudit.details'), enableSorting: false, cell: ({ row }) => <Details log={row.original} /> },
  ], [])

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.audit.title')} description={t('pages.audit.description')} />
      <FilterBar onReset={() => { setSearchDraft(''); setSearch(''); setFrom(''); setTo(''); setPage(1) }}>
        <label className="grid gap-1.5 text-sm font-medium">{t('common.search')}
          <Input value={searchDraft} placeholder={t('adminAudit.search')} onChange={(event) => setSearchDraft(event.target.value)} />
        </label>
        <DateRangeFields from={from} to={to} onFromChange={(value) => { setFrom(value); setPage(1) }} onToChange={(value) => { setTo(value); setPage(1) }} fromLabel={t('user.from')} toLabel={t('user.to')} />
      </FilterBar>
      {logs.isError ? <ErrorState onRetry={() => void logs.refetch()} />
        : logs.data?.items.length === 0 && !logs.isPending ? <EmptyState title={t('adminAudit.noLogs')} description={t('adminAudit.noLogsDescription')} badge={false} />
        : <DataTable columns={columns} data={logs.data?.items ?? []} page={page} totalPages={logs.data?.totalPages ?? 1} onPageChange={setPage} loading={logs.isPending} />}
    </div>
  )
}
