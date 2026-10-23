import { useEffect, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import type { SortingState } from '@tanstack/table-core'
import { useSearchParams } from 'react-router-dom'
import { useAdminDebtors } from '@/api/admin/hooks'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { formatDate } from '@/lib/format'
import type { DebtorDto, DebtorsQuery } from '@/types/api'
import { PaymentDialog, type PaymentResidentOption } from '@/features/admin/PaymentDialog'

export function AdminDebtorsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const { can } = useAuth()
  const [searchDraft, setSearchDraft] = useState(searchParams.get('search') ?? '')
  const [selectedResident, setSelectedResident] = useState<PaymentResidentOption>()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)
  const sortBy = searchParams.get('sortBy') || 'debtAmount'
  const sortDir = searchParams.get('sortDir') === 'asc' ? 'asc' : 'desc'
  const query: DebtorsQuery = {
    page,
    pageSize: 10,
    search: searchParams.get('search') || undefined,
    sortBy,
    sortDir,
  }
  const debtors = useAdminDebtors(query)
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

  const updateParam = (key: string, value: string) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)
      if (value) next.set(key, value)
      else next.delete(key)
      if (key !== 'page') next.delete('page')
      return next
    }, { replace: true })
  }

  const columns: ColumnDef<DebtorDto, unknown>[] = [
    { accessorKey: 'fullName', header: t('user.fullName') },
    { accessorKey: 'phone', header: t('user.phone'), enableSorting: false, cell: ({ row }) => row.original.phone ?? '—' },
    {
      id: 'location',
      header: t('adminFinance.buildingAndRoom'),
      enableSorting: false,
      cell: ({ row }) => `${row.original.buildingName ?? '—'} · ${row.original.roomNumber ?? '—'}`,
    },
    {
      accessorKey: 'debtAmount',
      header: t('adminFinance.debt'),
      cell: ({ row }) => <MoneyText value={row.original.debtAmount} />,
    },
    {
      accessorKey: 'oldestUnpaidDueDate',
      header: t('adminFinance.oldestUnpaidDate'),
      enableSorting: false,
      cell: ({ row }) => formatDate(row.original.oldestUnpaidDueDate),
    },
    {
      accessorKey: 'overdueDays',
      header: t('adminFinance.overdueDays'),
      cell: ({ row }) => {
        const days = row.original.overdueDays
        const color = days > 7 ? 'text-red-600 dark:text-red-400' : days > 0 ? 'text-amber-700 dark:text-amber-300' : 'text-muted-foreground'
        return <span className={`font-medium ${color}`}>{days}</span>
      },
    },
    {
      id: 'actions',
      header: t('admin.actions'),
      enableSorting: false,
      cell: ({ row }) => can('paymentsManage') ? (
        <Button size="sm" variant="outline" onClick={() => setSelectedResident({
          id: row.original.residentId,
          fullName: row.original.fullName,
          balance: -row.original.debtAmount,
        })}>{t('adminFinance.createPayment')}</Button>
      ) : null,
    },
  ]
  const sorting: SortingState = [{ id: sortBy, desc: sortDir === 'desc' }]

  const onSortChange = (next: SortingState) => {
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

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.debtors.title')} description={t('pages.debtors.description')} />
      <label className="grid max-w-md gap-1.5 text-sm font-medium">
        {t('adminFinance.searchDebtor')}
        <Input value={searchDraft} placeholder={t('user.fullName')} onChange={(event) => setSearchDraft(event.target.value)} />
      </label>
      {debtors.isPending ? (
        <div className="h-96 animate-pulse rounded-2xl bg-muted" aria-label={t('common.loading')} />
      ) : debtors.isError ? (
        <ErrorState onRetry={() => void debtors.refetch()} />
      ) : debtors.data.items.length ? (
        <div className="max-w-full">
          <DataTable
            columns={columns}
            data={debtors.data.items}
            page={debtors.data.page}
            totalPages={debtors.data.totalPages}
            onPageChange={(nextPage) => updateParam('page', String(nextPage))}
            sorting={sorting}
            onSortChange={onSortChange}
          />
        </div>
      ) : (
        <EmptyState badge={false} title={t('adminFinance.noDebtors')} description={t('adminFinance.noDebtorsDescription')} />
      )}
      <PaymentDialog
        open={Boolean(selectedResident)}
        initialResident={selectedResident}
        onOpenChange={(open) => { if (!open) setSelectedResident(undefined) }}
      />
    </div>
  )
}
