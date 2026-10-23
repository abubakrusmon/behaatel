import { useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { useMyPayments } from '@/api/meHooks'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading } from '@/features/user/components'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Select } from '@/components/ui/select'
import { t } from '@/i18n/ru'
import { formatDateTime } from '@/lib/format'
import type { MyPaymentDto } from '@/types/api'

export function PaymentsPage() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const payments = useMyPayments({
    page,
    pageSize,
    from: from ? `${from}T00:00:00Z` : undefined,
    to: to ? `${to}T23:59:59Z` : undefined,
  })
  const columns = useMemo<ColumnDef<MyPaymentDto, unknown>[]>(() => [
    { accessorKey: 'paidAt', header: t('user.date'), cell: ({ row }) => formatDateTime(row.original.paidAt) },
    { accessorKey: 'amount', header: t('user.amount'), cell: ({ row }) => (
      <span className={row.original.status === 'cancelled' ? 'line-through' : ''}><MoneyText value={row.original.amount} /></span>
    ) },
    { accessorKey: 'method', header: t('user.method'), cell: ({ row }) => t(`user.paymentMethod.${row.original.method}`) },
    { accessorKey: 'receiptNumber', header: t('user.receiptNumber') },
    { accessorKey: 'status', header: t('user.status'), cell: ({ row }) => <StatusBadge status={row.original.status} /> },
  ], [])

  if (payments.isPending) return <PageLoading cards={2} />
  if (payments.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void payments.refetch()} /></div>

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.myPayments.title')} description={t('pages.myPayments.description')} />
      <div className="flex flex-col gap-3 sm:flex-row">
        <label className="grid gap-1.5 text-sm font-medium">{t('user.from')}<input aria-label={t('user.from')} className="h-10 rounded-xl border border-input bg-background px-3 text-sm" type="date" value={from} max={to || undefined} onChange={(event) => { setFrom(event.target.value); setPage(1) }} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('user.to')}<input aria-label={t('user.to')} className="h-10 rounded-xl border border-input bg-background px-3 text-sm" type="date" value={to} min={from || undefined} onChange={(event) => { setTo(event.target.value); setPage(1) }} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('common.pageSize')}<Select aria-label={t('common.pageSize')} className="min-w-32" value={pageSize} onChange={(event) => { setPageSize(Number(event.target.value)); setPage(1) }}>{[2, 5, 10, 20].map((size) => <option key={size} value={size}>{size}</option>)}</Select></label>
      </div>
      {payments.data.items.length ? (
        <DataTable
          columns={columns}
          data={payments.data.items}
          page={payments.data.page}
          totalPages={payments.data.totalPages}
          onPageChange={setPage}
          getRowClassName={(payment) => payment.status === 'cancelled' ? 'opacity-55' : ''}
        />
      ) : <EmptyState title={t('user.emptyPaymentsTitle')} description={t('user.emptyPaymentsDescription')} badge={false} />}
    </div>
  )
}
