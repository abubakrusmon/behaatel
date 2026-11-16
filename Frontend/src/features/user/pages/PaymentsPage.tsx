import { useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { useMyPayments } from '@/api/meHooks'
import { DataTable } from '@/components/shared/DataTable'
import { DateRangeFields } from '@/components/shared/DatePicker'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { FilterBar } from '@/components/shared/FilterBar'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading } from '@/features/user/components'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
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
      <FilterBar onReset={() => { setFrom(''); setTo(''); setPage(1) }}>
        <DateRangeFields
          from={from}
          to={to}
          fromLabel={t('user.from')}
          toLabel={t('user.to')}
          onFromChange={(value) => { setFrom(value); setPage(1) }}
          onToChange={(value) => { setTo(value); setPage(1) }}
        />
        <label className="grid gap-1.5 text-sm font-medium">{t('common.pageSize')}
          <Select value={String(pageSize)} onValueChange={(value) => { setPageSize(Number(value)); setPage(1) }}>
            <SelectTrigger aria-label={t('common.pageSize')} className="min-w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {[2, 5, 10, 20].map((size) => <SelectItem key={size} value={String(size)}>{size}</SelectItem>)}
            </SelectContent>
          </Select>
        </label>
      </FilterBar>
      {payments.data.items.length ? (
        <>
          <div className="space-y-3 md:hidden">
            {payments.data.items.map((payment) => (
              <Card key={payment.id} className={`p-4 ${payment.status === 'cancelled' ? 'opacity-60' : ''}`}>
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-xs text-muted-foreground">{formatDateTime(payment.paidAt)}</p>
                    <p className="mt-1 font-medium">{t(`user.paymentMethod.${payment.method}`)}</p>
                  </div>
                  <StatusBadge status={payment.status} />
                </div>
                <div className="mt-3 flex items-end justify-between gap-3 border-t border-border pt-3">
                  <span className="text-xs text-muted-foreground">{t('user.receiptNumber')}: {payment.receiptNumber}</span>
                  <span className={`text-lg font-semibold ${payment.status === 'cancelled' ? 'line-through' : ''}`}><MoneyText value={payment.amount} /></span>
                </div>
              </Card>
            ))}
          </div>
          <div className="hidden md:block">
            <DataTable
              columns={columns}
              data={payments.data.items}
              page={payments.data.page}
              totalPages={payments.data.totalPages}
              onPageChange={setPage}
              getRowClassName={(payment) => payment.status === 'cancelled' ? 'opacity-55' : ''}
            />
          </div>
          <div className="flex items-center justify-between gap-3 text-sm text-muted-foreground md:hidden">
            <span>{t('common.page')} {payments.data.page} {t('common.of')} {Math.max(1, payments.data.totalPages)}</span>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" disabled={!payments.data.hasPrevious} onClick={() => setPage((value) => value - 1)}>{t('common.previousPage')}</Button>
              <Button variant="outline" size="sm" disabled={!payments.data.hasNext} onClick={() => setPage((value) => value + 1)}>{t('common.nextPage')}</Button>
            </div>
          </div>
        </>
      ) : <EmptyState title={t('user.emptyPaymentsTitle')} description={t('user.emptyPaymentsDescription')} badge={false} />}
    </div>
  )
}
