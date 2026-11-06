import { useMemo, useState } from 'react'
import { isBefore, parseISO, startOfDay } from 'date-fns'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { useMyCharges } from '@/api/meHooks'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading } from '@/features/user/components'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { t } from '@/i18n/ru'
import { formatDate, formatPeriod } from '@/lib/format'
import type { ChargeStatus, MyChargeDto } from '@/types/api'

const statuses: ChargeStatus[] = ['unpaid', 'partiallyPaid', 'paid', 'cancelled']

export function ChargesPage() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [period, setPeriod] = useState('')
  const [status, setStatus] = useState<ChargeStatus | ''>('')
  const charges = useMyCharges({ page, pageSize, period: period ? `${period}-01` : undefined, status: status || undefined })
  const columns = useMemo<ColumnDef<MyChargeDto, unknown>[]>(() => [
    { accessorKey: 'period', header: t('user.period'), cell: ({ row }) => formatPeriod(row.original.period.slice(0, 7)) },
    { accessorKey: 'type', header: t('user.type'), cell: ({ row }) => t(`user.chargeType.${row.original.type}`) },
    { accessorKey: 'description', header: t('user.description'), cell: ({ row }) => row.original.description || '—' },
    { accessorKey: 'amount', header: t('user.amount'), cell: ({ row }) => <MoneyText value={row.original.amount} /> },
    { accessorKey: 'paidAmount', header: t('user.paidAmount'), cell: ({ row }) => <MoneyText value={row.original.paidAmount} /> },
    { accessorKey: 'remainingAmount', header: t('user.remainingAmount'), cell: ({ row }) => (
      <span className={row.original.remainingAmount > 0 ? 'font-semibold text-red-600 dark:text-red-400' : ''}>
        <MoneyText value={row.original.remainingAmount} />
      </span>
    ) },
    { accessorKey: 'dueDate', header: t('user.dueDate'), cell: ({ row }) => formatDate(row.original.dueDate) },
    { accessorKey: 'status', header: t('user.status'), cell: ({ row }) => <StatusBadge status={row.original.status} /> },
  ], [])

  if (charges.isPending) return <PageLoading cards={2} />
  if (charges.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void charges.refetch()} /></div>

  const items = charges.data?.items ?? []
  const today = startOfDay(new Date())

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.myCharges.title')} description={t('pages.myCharges.description')} />
      <div className="flex flex-col gap-3 sm:flex-row">
        <label className="grid gap-1.5 text-sm font-medium">
          {t('user.filterPeriod')}
          <input
            aria-label={t('user.filterPeriod')}
            className="h-10 rounded-xl border border-input bg-background px-3 text-sm"
            type="month"
            value={period}
            onChange={(event) => { setPeriod(event.target.value); setPage(1) }}
          />
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          {t('user.filterStatus')}
          <Select value={status || 'all'} onValueChange={(value) => { setStatus(value === 'all' ? '' : value as ChargeStatus); setPage(1) }}>
            <SelectTrigger aria-label={t('user.filterStatus')} className="min-w-48">
              <SelectValue placeholder={t('user.allStatuses')} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t('user.allStatuses')}</SelectItem>
              {statuses.map((item) => <SelectItem key={item} value={item}>{t(`status.${item}`)}</SelectItem>)}
            </SelectContent>
          </Select>
        </label>
        <label className="grid gap-1.5 text-sm font-medium">
          {t('common.pageSize')}
          <Select value={String(pageSize)} onValueChange={(value) => { setPageSize(Number(value)); setPage(1) }}>
            <SelectTrigger aria-label={t('common.pageSize')} className="min-w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {[2, 5, 10, 20].map((size) => <SelectItem key={size} value={String(size)}>{size}</SelectItem>)}
            </SelectContent>
          </Select>
        </label>
      </div>
      {items.length ? (
        <DataTable
          columns={columns}
          data={items}
          page={charges.data.page}
          totalPages={charges.data.totalPages}
          onPageChange={setPage}
          getRowClassName={(charge) => charge.status !== 'paid' && charge.status !== 'cancelled' && isBefore(parseISO(charge.dueDate), today)
            ? 'bg-red-50/60 dark:bg-red-950/20'
            : ''}
        />
      ) : <EmptyState title={t('user.emptyChargesTitle')} description={t('user.emptyChargesDescription')} badge={false} />}
    </div>
  )
}
