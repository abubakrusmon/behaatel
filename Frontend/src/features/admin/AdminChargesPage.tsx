import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { Controller, useForm } from 'react-hook-form'
import { useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import { useAdminResidents } from '@/api/admin/hooks'
import {
  useCancelManagementCharge,
  useCreateManagementCharge,
  useGenerateManagementCharges,
  useManagementCharges,
} from '@/api/admin/managementHooks'
import { getErrorMessage } from '@/api/errors'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { DatePicker } from '@/components/shared/DatePicker'
import { FilterBar } from '@/components/shared/FilterBar'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { formatDate } from '@/lib/format'
import { toast } from '@/lib/toast'
import type { ChargeStatus, ChargeType } from '@/types/api'
import type { AdminChargeDto, ManagementChargesQuery } from '@/types/admin'

const chargeTypes: ChargeType[] = ['rent', 'utilities', 'penalty', 'other']
const chargeStatuses: ChargeStatus[] = ['unpaid', 'partiallyPaid', 'paid', 'cancelled']
const chargeSchema = z.object({
  residentId: z.string().min(1, t('adminCharges.residentRequired')),
  period: z.string().regex(/^\d{4}-\d{2}$/, t('adminCharges.periodRequired')),
  type: z.enum(chargeTypes),
  amount: z.string().refine((value) => Number.isFinite(Number(value)) && Number(value) > 0, t('adminCharges.amountRequired')),
  dueDate: z.string().min(1, t('adminCharges.dueDateRequired')),
  description: z.string(),
})
type ChargeFormValues = z.infer<typeof chargeSchema>
const initialPeriod = () => new Date().toISOString().slice(0, 7)

function ChargeFormDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const [defaultPeriod] = useState(initialPeriod)
  const [residentSearch, setResidentSearch] = useState('')
  const residents = useAdminResidents(residentSearch.trim(), open)
  const create = useCreateManagementCharge()
  const { control, register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<ChargeFormValues>({
    resolver: zodResolver(chargeSchema),
    defaultValues: { residentId: '', period: defaultPeriod, type: 'rent', amount: '', dueDate: '', description: '' },
  })
  useEffect(() => {
    if (open) reset({ residentId: '', period: defaultPeriod, type: 'rent', amount: '', dueDate: '', description: '' })
  }, [defaultPeriod, open, reset])
  const submit = handleSubmit(async (values) => {
    try {
      await create.mutateAsync({
        residentId: values.residentId,
        period: `${values.period}-01`,
        type: values.type,
        amount: Number(values.amount),
        dueDate: values.dueDate,
        description: values.description.trim() || null,
      })
      toast.success(t('adminCharges.created'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  const fieldError = (message?: string) => message ? <span className="text-xs text-destructive" role="alert">{message}</span> : null
  const pending = isSubmitting || create.isPending
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminCharges.create')}>
    <form className="max-h-[75vh] space-y-4 overflow-y-auto pr-1" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.search')}<Input value={residentSearch} onChange={(event) => setResidentSearch(event.target.value)} placeholder={t('adminCharges.search')} /></label>
      {residents.isError && <p className="text-xs text-destructive" role="alert">{getErrorMessage(residents.error)}</p>}
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.resident')}<Controller control={control} name="residentId" render={({ field }) => (
        <Select value={field.value} onValueChange={field.onChange}><SelectTrigger><SelectValue placeholder={t('adminCharges.resident')} /></SelectTrigger><SelectContent>{residents.data?.items.map((resident) => <SelectItem key={resident.id} value={resident.id}>{resident.fullName}</SelectItem>)}</SelectContent></Select>
      )} />{fieldError(errors.residentId?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.period')}<Input type="month" {...register('period')} aria-invalid={Boolean(errors.period)} />{fieldError(errors.period?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.type')}<Controller control={control} name="type" render={({ field }) => (
        <Select value={field.value} onValueChange={field.onChange}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent>{chargeTypes.map((type) => <SelectItem key={type} value={type}>{t(`user.chargeType.${type}`)}</SelectItem>)}</SelectContent></Select>
      )} /></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.amount')}<Input type="number" min="0.01" step="0.01" {...register('amount')} aria-invalid={Boolean(errors.amount)} />{fieldError(errors.amount?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.dueDate')}<Controller control={control} name="dueDate" render={({ field }) => <DatePicker value={field.value} onChange={field.onChange} ariaLabel={t('adminCharges.dueDate')} />} />{fieldError(errors.dueDate?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.descriptionField')}<Textarea {...register('description')} /></label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={pending}>{pending ? t('common.loading') : t('common.save')}</Button></div>
    </form>
  </Dialog>
}

export function AdminChargesPage() {
  const [params, setParams] = useSearchParams()
  const { can } = useAuth()
  const [searchDraft, setSearchDraft] = useState(params.get('search') ?? '')
  const [createOpen, setCreateOpen] = useState(false)
  const [cancelTarget, setCancelTarget] = useState<AdminChargeDto | null>(null)
  const [cancelReason, setCancelReason] = useState('')
  const page = Math.max(1, Number(params.get('page')) || 1)
  const period = params.get('period') ?? ''
  const statusValue = params.get('status') ?? ''
  const typeValue = params.get('type') ?? ''
  const query: ManagementChargesQuery = {
    page,
    pageSize: 10,
    search: params.get('search') || undefined,
    period: period ? `${period}-01` : undefined,
    status: chargeStatuses.includes(statusValue as ChargeStatus) ? statusValue as ChargeStatus : undefined,
    type: chargeTypes.includes(typeValue as ChargeType) ? typeValue as ChargeType : undefined,
    sortBy: 'period',
    sortDir: 'desc',
  }
  const charges = useManagementCharges(query)
  const cancel = useCancelManagementCharge()
  const generate = useGenerateManagementCharges()
  const [generatePeriod, setGeneratePeriod] = useState(initialPeriod)
  const [generateOpen, setGenerateOpen] = useState(false)

  useEffect(() => {
    const syncSearchFromHistory = () => setSearchDraft(new URLSearchParams(window.location.search).get('search') ?? '')
    window.addEventListener('popstate', syncSearchFromHistory)
    return () => window.removeEventListener('popstate', syncSearchFromHistory)
  }, [])

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setParams((current) => {
        const next = new URLSearchParams(current)
        if (searchDraft.trim()) next.set('search', searchDraft.trim())
        else next.delete('search')
        next.delete('page')
        return next
      }, { replace: true })
    }, 350)
    return () => window.clearTimeout(timer)
  }, [searchDraft, setParams])
  const updateParam = (key: string, value: string) => setParams((current) => {
    const next = new URLSearchParams(current)
    if (value) next.set(key, value)
    else next.delete(key)
    if (key !== 'page') next.delete('page')
    return next
  }, { replace: true })
  const clearFilters = () => {
    setSearchDraft('')
    setParams(new URLSearchParams(), { replace: true })
  }
  const columns = useMemo<ColumnDef<AdminChargeDto, unknown>[]>(() => [
    { accessorKey: 'residentName', header: t('adminCharges.resident') },
    { accessorKey: 'period', header: t('adminCharges.period'), cell: ({ row }) => row.original.period.slice(0, 7) },
    { accessorKey: 'type', header: t('adminCharges.type'), cell: ({ row }) => t(`user.chargeType.${row.original.type}`) },
    { accessorKey: 'amount', header: t('adminCharges.amount'), cell: ({ row }) => <MoneyText value={row.original.amount} /> },
    { accessorKey: 'paidAmount', header: t('adminCharges.paid'), cell: ({ row }) => <MoneyText value={row.original.paidAmount} /> },
    { accessorKey: 'remainingAmount', header: t('adminCharges.remaining'), cell: ({ row }) => <MoneyText value={row.original.remainingAmount} /> },
    { accessorKey: 'dueDate', header: t('adminCharges.dueDate'), cell: ({ row }) => formatDate(row.original.dueDate) },
    { accessorKey: 'status', header: t('adminCharges.statusFilter'), cell: ({ row }) => <StatusBadge status={row.original.status} /> },
    { id: 'actions', header: t('adminCharges.actions'), enableSorting: false, cell: ({ row }) =>
      can('chargesManage') && row.original.status !== 'cancelled' ? <Button size="sm" variant="destructive" onClick={() => { setCancelTarget(row.original); setCancelReason('') }}>{t('adminCharges.cancel')}</Button> : null },
  ], [can])
  const confirmCancel = async () => {
    if (!cancelTarget) return
    if (!cancelReason.trim()) {
      toast.error(t('adminCharges.reasonRequired'))
      return
    }
    try {
      await cancel.mutateAsync({ id: cancelTarget.id, reason: cancelReason.trim() })
      toast.success(t('adminCharges.cancelled'))
      setCancelTarget(null)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }
  const confirmGenerate = async () => {
    try {
      const result = await generate.mutateAsync(generatePeriod)
      toast.success(`${t('adminCharges.generated')} ${result.created}; ${t('adminCharges.skipped')} ${result.skipped}`)
      setGenerateOpen(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  return <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
    <PageHeader title={t('adminCharges.title')} description={t('adminCharges.description')} actions={can('chargesManage') ? <>
      <Button variant="outline" onClick={() => setGenerateOpen(true)}>{t('adminCharges.generate')}</Button>
      <Button onClick={() => setCreateOpen(true)}>{t('adminCharges.create')}</Button>
    </> : undefined} />
    <FilterBar onReset={clearFilters}>
      <label className="grid gap-1.5 text-sm font-medium sm:col-span-2">{t('common.search')}<Input value={searchDraft} placeholder={t('adminCharges.search')} onChange={(event) => setSearchDraft(event.target.value)} /></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.periodFilter')}<Input type="month" value={period} onChange={(event) => updateParam('period', event.target.value)} /></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.statusFilter')}<Select value={statusValue || 'all'} onValueChange={(value) => updateParam('status', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminCharges.allStatuses')}</SelectItem>{chargeStatuses.map((status) => <SelectItem key={status} value={status}>{t(`status.${status}`)}</SelectItem>)}</SelectContent></Select></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminCharges.typeFilter')}<Select value={typeValue || 'all'} onValueChange={(value) => updateParam('type', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminCharges.allTypes')}</SelectItem>{chargeTypes.map((type) => <SelectItem key={type} value={type}>{t(`user.chargeType.${type}`)}</SelectItem>)}</SelectContent></Select></label>
    </FilterBar>
    {charges.isError ? <ErrorState onRetry={() => void charges.refetch()} /> : charges.data?.items.length === 0 && !charges.isPending ? <EmptyState title={t('adminCharges.noCharges')} description={t('adminCharges.noChargesDescription')} badge={false} /> :
      <DataTable columns={columns} data={charges.data?.items ?? []} page={page} totalPages={charges.data?.totalPages ?? 1} onPageChange={(value) => updateParam('page', String(value))} loading={charges.isPending} />}
    <ChargeFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    <ConfirmDialog open={generateOpen} onOpenChange={setGenerateOpen} title={t('adminCharges.generateTitle')} description={t('adminCharges.generateConfirm')} confirmLabel={t('adminCharges.generate')} confirmingLabel={t('common.loading')} pending={generate.isPending} onConfirm={() => { void confirmGenerate() }}>
      <label className="mt-4 grid gap-1.5 text-sm font-medium">{t('adminCharges.period')}<Input type="month" value={generatePeriod} onChange={(event) => setGeneratePeriod(event.target.value)} /></label>
    </ConfirmDialog>
    <ConfirmDialog open={!!cancelTarget} onOpenChange={(open) => { if (!open) setCancelTarget(null) }} title={t('adminCharges.cancelTitle')} description={t('adminCharges.cancelConfirm')} confirmLabel={t('adminCharges.cancel')} confirmingLabel={t('common.loading')} pending={cancel.isPending} onConfirm={() => { void confirmCancel() }}>
      <label className="mt-4 grid gap-1.5 text-sm font-medium">{t('adminCharges.reason')}<Textarea value={cancelReason} onChange={(event) => setCancelReason(event.target.value)} /></label>
    </ConfirmDialog>
  </div>
}
