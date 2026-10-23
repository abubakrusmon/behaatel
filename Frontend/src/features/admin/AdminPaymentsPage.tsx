import { useEffect, useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import type { SortingState } from '@tanstack/table-core'
import { Download, Filter, Plus, X } from 'lucide-react'
import { useSearchParams } from 'react-router-dom'
import { exportAdminPayments } from '@/api/admin/admin'
import { useAdminPayments, useCancelAdminPayment } from '@/api/admin/hooks'
import { getErrorMessage } from '@/api/errors'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import { Sheet } from '@/components/ui/sheet'
import { Textarea } from '@/components/ui/textarea'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { formatDateTime } from '@/lib/format'
import { toast } from '@/lib/toast'
import type { PaymentDto, PaymentMethod, PaymentStatus, PaymentsQuery } from '@/types/api'
import { PaymentDialog } from '@/features/admin/PaymentDialog'

const paymentStatuses: PaymentStatus[] = ['completed', 'cancelled']
const paymentMethods: PaymentMethod[] = ['cash', 'card', 'bankTransfer', 'online']

function utcDateRange(date: string, end = false): string {
  return `${date}T${end ? '23:59:59.999' : '00:00:00.000'}Z`
}

function optionalAmount(value: string | null): number | undefined {
  if (value === null || value.trim() === '') return undefined
  const amount = Number(value)
  return Number.isFinite(amount) ? amount : undefined
}

function FilterFields({
  search,
  from,
  to,
  status,
  method,
  minAmount,
  maxAmount,
  onSearch,
  onFilter,
}: {
  search: string
  from: string
  to: string
  status: string
  method: string
  minAmount: string
  maxAmount: string
  onSearch: (value: string) => void
  onFilter: (key: string, value: string) => void
}) {
  return (
    <>
      <label className="grid gap-1.5 text-sm font-medium">
        {t('adminFinance.searchPayment')}
        <Input value={search} placeholder={t('adminFinance.searchHint')} onChange={(event) => onSearch(event.target.value)} />
      </label>
      <div className="grid grid-cols-2 gap-3">
        <label className="grid gap-1.5 text-sm font-medium">{t('adminFinance.dateFrom')}<Input type="date" value={from} onChange={(event) => onFilter('from', event.target.value)} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminFinance.dateTo')}<Input type="date" value={to} onChange={(event) => onFilter('to', event.target.value)} /></label>
      </div>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminFinance.status')}
        <Select value={status} onChange={(event) => onFilter('status', event.target.value)}>
          <option value="">{t('adminFinance.allStatuses')}</option>
          {paymentStatuses.map((value) => <option key={value} value={value}>{t(`status.${value}`)}</option>)}
        </Select>
      </label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminFinance.method')}
        <Select value={method} onChange={(event) => onFilter('method', event.target.value)}>
          <option value="">{t('adminFinance.allMethods')}</option>
          {paymentMethods.map((value) => <option key={value} value={value}>{t(`paymentMethod.${value}`)}</option>)}
        </Select>
      </label>
      <div className="grid grid-cols-2 gap-3">
        <label className="grid gap-1.5 text-sm font-medium">{t('adminFinance.amountFrom')}<Input type="number" min="0" step="0.01" value={minAmount} onChange={(event) => onFilter('minAmount', event.target.value)} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminFinance.amountTo')}<Input type="number" min="0" step="0.01" value={maxAmount} onChange={(event) => onFilter('maxAmount', event.target.value)} /></label>
      </div>
    </>
  )
}

export function AdminPaymentsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const { can } = useAuth()
  const [searchDraft, setSearchDraft] = useState(searchParams.get('search') ?? '')
  const [filtersOpen, setFiltersOpen] = useState(false)
  const [paymentDialogOpen, setPaymentDialogOpen] = useState(false)
  const [paymentToCancel, setPaymentToCancel] = useState<PaymentDto | null>(null)
  const [cancelReason, setCancelReason] = useState('')
  const [cancelReasonError, setCancelReasonError] = useState('')
  const [exporting, setExporting] = useState(false)
  const cancelPayment = useCancelAdminPayment()
  const urlSearch = searchParams.get('search') ?? ''

  useEffect(() => {
    const syncSearchFromHistory = () => setSearchDraft(new URLSearchParams(window.location.search).get('search') ?? '')
    window.addEventListener('popstate', syncSearchFromHistory)
    return () => window.removeEventListener('popstate', syncSearchFromHistory)
  }, [])

  useEffect(() => {
    if (searchDraft.trim() === urlSearch) return
    const timer = window.setTimeout(() => {
      setSearchParams((current) => {
        const next = new URLSearchParams(current)
        if (searchDraft.trim()) next.set('search', searchDraft.trim())
        else next.delete('search')
        next.delete('page')
        return next
      }, { replace: true })
    }, 400)
    return () => window.clearTimeout(timer)
  }, [searchDraft, setSearchParams, urlSearch])

  const page = Math.max(1, Number(searchParams.get('page')) || 1)
  const dateFrom = searchParams.get('from') ?? ''
  const dateTo = searchParams.get('to') ?? ''
  const status = searchParams.get('status') ?? ''
  const method = searchParams.get('method') ?? ''
  const minAmount = searchParams.get('minAmount') ?? ''
  const maxAmount = searchParams.get('maxAmount') ?? ''
  const sortByParam = searchParams.get('sortBy')
  const sortBy = sortByParam === 'amount' || sortByParam === 'paidAt' ? sortByParam : 'paidAt'
  const sortDir = searchParams.get('sortDir') === 'asc' ? 'asc' : 'desc'
  const query: PaymentsQuery = {
    page,
    pageSize: 10,
    search: searchParams.get('search') || undefined,
    status: paymentStatuses.includes(status as PaymentStatus) ? status as PaymentStatus : undefined,
    method: paymentMethods.includes(method as PaymentMethod) ? method as PaymentMethod : undefined,
    from: dateFrom ? utcDateRange(dateFrom) : undefined,
    to: dateTo ? utcDateRange(dateTo, true) : undefined,
    minAmount: optionalAmount(searchParams.get('minAmount')),
    maxAmount: optionalAmount(searchParams.get('maxAmount')),
    sortBy,
    sortDir,
  }
  const payments = useAdminPayments(query)
  const sorting: SortingState = [{ id: sortBy, desc: sortDir === 'desc' }]

  const updateParam = (key: string, value: string) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)
      if (value) next.set(key, value)
      else next.delete(key)
      if (key !== 'page') next.delete('page')
      return next
    }, { replace: true })
  }

  const clearFilters = () => {
    setSearchDraft('')
    setSearchParams(new URLSearchParams(), { replace: true })
    setFiltersOpen(false)
  }

  const columns = useMemo<ColumnDef<PaymentDto, unknown>[]>(() => [
    {
      accessorKey: 'paidAt',
      header: t('adminFinance.paidAt'),
      cell: ({ row }) => <span className="whitespace-nowrap">{formatDateTime(row.original.paidAt)}</span>,
    },
    { accessorKey: 'residentName', header: t('user.fullName'), enableSorting: false },
    {
      accessorKey: 'amount',
      header: t('adminFinance.amount'),
      cell: ({ row }) => row.original.status === 'cancelled'
        ? <span className="text-muted-foreground line-through"><MoneyText value={row.original.amount} /></span>
        : <MoneyText value={row.original.amount} />,
    },
    { accessorKey: 'method', header: t('adminFinance.method'), enableSorting: false, cell: ({ row }) => t(`paymentMethod.${row.original.method}`) },
    { accessorKey: 'status', header: t('adminFinance.status'), enableSorting: false, cell: ({ row }) => <StatusBadge status={row.original.status} /> },
    { accessorKey: 'receiptNumber', header: t('adminFinance.receiptNumber'), enableSorting: false },
    { accessorKey: 'comment', header: t('adminFinance.comment'), enableSorting: false, cell: ({ row }) => row.original.comment || '—' },
    {
      id: 'allocation',
      header: t('adminFinance.allocation'),
      enableSorting: false,
      cell: ({ row }) => (
        <span className="whitespace-nowrap text-xs">
          <span className="block">{t('adminFinance.allocated')}: <MoneyText value={row.original.allocatedAmount} /></span>
          <span className="block text-muted-foreground">{t('adminFinance.unallocated')}: <MoneyText value={row.original.unallocatedAmount} /></span>
        </span>
      ),
    },
    {
      id: 'actions',
      header: t('admin.actions'),
      enableSorting: false,
      cell: ({ row }) => can('paymentsManage') && row.original.status !== 'cancelled' ? (
        <details className="relative">
          <summary className="cursor-pointer list-none rounded-lg px-3 py-2 text-sm text-primary hover:bg-muted focus-visible:outline-2 focus-visible:outline-ring" aria-label={`${t('adminFinance.paymentActions')} ${row.original.receiptNumber}`}>{t('admin.actions')}</summary>
          <div className="absolute right-0 z-20 mt-1 min-w-40 rounded-xl border border-border bg-popover p-1 shadow-lg">
            <button className="w-full rounded-lg px-3 py-2 text-left text-sm text-destructive hover:bg-muted focus-visible:bg-muted" type="button" onClick={(event) => {
              event.currentTarget.closest('details')?.removeAttribute('open')
              setPaymentToCancel(row.original)
              setCancelReason('')
              setCancelReasonError('')
            }}>{t('adminFinance.cancelPayment')}</button>
          </div>
        </details>
      ) : null,
    },
  ], [can])

  const applySorting = (next: SortingState) => {
    const column = next[0]
    if (!column) return
    setSearchParams((current) => {
      const params = new URLSearchParams(current)
      params.set('sortBy', column.id)
      params.set('sortDir', column.desc ? 'desc' : 'asc')
      params.delete('page')
      return params
    }, { replace: true })
  }

  const handleExport = async () => {
    setExporting(true)
    try {
      const blob = await exportAdminPayments({
        search: query.search,
        status: query.status,
        method: query.method,
        from: query.from,
        to: query.to,
        minAmount: query.minAmount,
        maxAmount: query.maxAmount,
        sortBy: query.sortBy,
        sortDir: query.sortDir,
      })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `payments-${new Date().toISOString().slice(0, 10)}.csv`
      document.body.append(link)
      link.click()
      link.remove()
      window.setTimeout(() => URL.revokeObjectURL(url), 1000)
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setExporting(false)
    }
  }

  const confirmCancel = () => {
    if (!paymentToCancel) return
    const reason = cancelReason.trim()
    if (!reason) {
      setCancelReasonError(t('adminFinance.cancelReasonRequired'))
      return
    }
    cancelPayment.mutate({ id: paymentToCancel.id, reason }, {
      onSuccess: () => {
        toast.success(t('adminFinance.paymentCancelled'))
        setPaymentToCancel(null)
      },
      onError: (error) => toast.error(getErrorMessage(error)),
    })
  }

  const filterFieldsProps = {
    search: searchDraft,
    from: dateFrom,
    to: dateTo,
    status,
    method,
    minAmount,
    maxAmount,
    onSearch: setSearchDraft,
    onFilter: updateParam,
  }

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader
        title={t('pages.payments.title')}
        description={t('pages.payments.description')}
        actions={<div className="flex flex-wrap gap-2">
          <Button variant="outline" onClick={() => void handleExport()} disabled={exporting} aria-label={t('adminFinance.exportCsv')}>
            <Download size={17} />{exporting ? t('adminFinance.exportLoading') : t('adminFinance.exportCsv')}
          </Button>
          {can('paymentsManage') && <Button onClick={() => setPaymentDialogOpen(true)}><Plus size={17} />{t('adminFinance.createPayment')}</Button>}
        </div>}
      />
      <p className="-mt-4 text-xs text-muted-foreground">{t('adminFinance.csvLimitHint')}</p>

      <div className="flex items-center justify-between gap-3 lg:hidden">
        <Button variant="outline" onClick={() => setFiltersOpen(true)}><Filter size={16} />{t('adminFinance.filters')}</Button>
        <Button variant="ghost" size="sm" onClick={clearFilters}><X size={15} />{t('adminFinance.resetFilters')}</Button>
      </div>
      <Sheet open={filtersOpen} onOpenChange={setFiltersOpen} side="right">
        <div className="h-full space-y-4 overflow-y-auto p-5 pt-16">
          <h2 className="text-lg font-semibold">{t('adminFinance.filters')}</h2>
          <FilterFields {...filterFieldsProps} />
          <Button variant="outline" className="w-full" onClick={clearFilters}>{t('adminFinance.resetFilters')}</Button>
        </div>
      </Sheet>
      <Card className="hidden gap-4 p-5 lg:grid lg:grid-cols-4">
        <FilterFields {...filterFieldsProps} />
        <div className="flex items-end"><Button variant="outline" onClick={clearFilters}>{t('adminFinance.resetFilters')}</Button></div>
      </Card>

      {payments.isPending ? (
        <div className="h-96 animate-pulse rounded-2xl bg-muted" aria-label={t('common.loading')} />
      ) : payments.isError ? (
        <ErrorState onRetry={() => void payments.refetch()} />
      ) : payments.data.items.length ? (
        <div className="max-w-full">
          <DataTable
            columns={columns}
            data={payments.data.items}
            page={payments.data.page}
            totalPages={payments.data.totalPages}
            onPageChange={(nextPage) => updateParam('page', String(nextPage))}
            sorting={sorting}
            onSortChange={applySorting}
            getRowClassName={(payment) => payment.status === 'cancelled' ? 'opacity-60' : ''}
          />
        </div>
      ) : (
        <EmptyState badge={false} title={t('adminFinance.noPayments')} description={t('adminFinance.noPaymentsDescription')} />
      )}

      <PaymentDialog open={paymentDialogOpen} onOpenChange={setPaymentDialogOpen} />
      <ConfirmDialog
        open={Boolean(paymentToCancel)}
        onOpenChange={(open) => { if (!open) setPaymentToCancel(null) }}
        title={t('adminFinance.cancelPayment')}
        description={paymentToCancel ? `${t('adminFinance.cancelPaymentDescription')} ${paymentToCancel.receiptNumber}` : undefined}
        confirmLabel={t('adminFinance.confirmCancelPayment')}
        confirmingLabel={t('adminFinance.cancellingPayment')}
        pending={cancelPayment.isPending}
        onConfirm={confirmCancel}
      >
        <label className="mt-4 grid gap-1.5 text-sm font-medium">
          {t('adminFinance.cancelReason')}
          <Textarea value={cancelReason} maxLength={1000} aria-invalid={Boolean(cancelReasonError)} onChange={(event) => {
            setCancelReason(event.target.value)
            setCancelReasonError('')
          }} />
          {cancelReasonError && <span className="text-xs text-destructive" role="alert">{cancelReasonError}</span>}
        </label>
      </ConfirmDialog>
    </div>
  )
}
