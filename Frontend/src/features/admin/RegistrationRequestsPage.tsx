import { useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { useMutation, useQuery } from '@tanstack/react-query'
import { approveRegistration, approvalStatuses, getRegistrationRequests, rejectRegistration } from '@/api/registrations'
import { queryClient } from '@/api/queryClient'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Select } from '@/components/ui/select'
import { t } from '@/i18n/ru'
import { formatDateTime } from '@/lib/format'
import type { RegistrationRequestDto } from '@/types/api'

export function RegistrationRequestsPage() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('pending')
  const query = useQuery({
    queryKey: ['admin', 'registrations', page, status],
    queryFn: () => getRegistrationRequests({ page, pageSize: 10, status: status ? status as RegistrationRequestDto['status'] : undefined }),
  })
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin', 'registrations'] })
  const approve = useMutation({ mutationFn: approveRegistration, onSuccess: invalidate })
  const reject = useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string | null }) => rejectRegistration(id, reason), onSuccess: invalidate })
  const columns = useMemo<ColumnDef<RegistrationRequestDto, unknown>[]>(() => [
    { accessorKey: 'fullName', header: t('user.fullName') },
    { accessorKey: 'email', header: t('auth.email') },
    { accessorKey: 'requestedAt', header: t('admin.registrationDate'), cell: ({ row }) => formatDateTime(row.original.requestedAt) },
    { accessorKey: 'status', header: t('user.status'), cell: ({ row }) => <StatusBadge status={row.original.status} /> },
    { id: 'actions', header: t('admin.actions'), cell: ({ row }) => row.original.status === 'pending' ? (
      <div className="flex flex-wrap gap-2">
        <Button size="sm" disabled={!row.original.emailConfirmed || approve.isPending || reject.isPending}
          onClick={() => { if (window.confirm(t('admin.confirmApprove'))) approve.mutate(row.original.userId) }}>{t('admin.approve')}</Button>
        <Button size="sm" variant="outline" disabled={approve.isPending || reject.isPending}
          onClick={() => {
            if (!window.confirm(t('admin.confirmReject'))) return
            const reason = window.prompt(t('admin.rejectReason'))
            if (reason !== null) reject.mutate({ id: row.original.userId, reason: reason.trim() || null })
          }}>{t('admin.reject')}</Button>
      </div>
    ) : <span className="text-sm text-muted-foreground">{row.original.emailConfirmed ? t('auth.email') : t('admin.emailNotConfirmed')}</span> },
  ], [approve, reject])

  if (query.isPending) return <RegistrationLoading />
  if (query.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void query.refetch()} /></div>
  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('admin.registrationsTitle')} description={t('admin.registrationsDescription')} />
      <label className="grid max-w-xs gap-1.5 text-sm font-medium">{t('user.filterStatus')}
        <Select aria-label={t('user.filterStatus')} value={status} onChange={(event) => { setStatus(event.target.value); setPage(1) }}>
          {approvalStatuses.map((item) => <option key={item} value={item}>{t(`status.${item}`)}</option>)}
        </Select>
      </label>
      {query.data.items.length ? (
        <DataTable columns={columns} data={query.data.items} page={query.data.page}
          totalPages={query.data.totalPages} onPageChange={setPage} />
      ) : <EmptyState title={t('admin.noRegistrations')} description={t('admin.noRegistrationsDescription')} badge={false} />}
      {(approve.isError || reject.isError) && <p className="text-sm text-destructive" role="alert">{t('errors.generic')}</p>}
    </div>
  )
}

function RegistrationLoading() {
  return <div className="mx-auto max-w-7xl space-y-5 p-4 sm:p-7" aria-label={t('common.loading')}>
    <div className="h-8 w-64 animate-pulse rounded bg-muted" />
    <div className="h-72 animate-pulse rounded-2xl bg-muted" />
  </div>
}
