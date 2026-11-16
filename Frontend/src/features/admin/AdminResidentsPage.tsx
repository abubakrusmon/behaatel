import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { Controller, useForm } from 'react-hook-form'
import { useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import {
  useCreateManagementResident,
  useCheckInResident,
  useCheckOutResident,
  useDeleteManagementResident,
  useManagementBuildings,
  useManagementResident,
  useManagementResidents,
  useManagementRooms,
  useResetResidentPassword,
  useSetResidentBlocked,
  useUpdateManagementResident,
} from '@/api/admin/managementHooks'
import { getErrorMessage } from '@/api/errors'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { DataTable } from '@/components/shared/DataTable'
import { DatePicker } from '@/components/shared/DatePicker'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { FilterBar } from '@/components/shared/FilterBar'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { formatDate } from '@/lib/format'
import { toast } from '@/lib/toast'
import type { ResidentListItemDto, ResidentStatus } from '@/types/api'
import type { AdminResidentDto, ManagementResidentsQuery } from '@/types/admin'

const residentStatuses: ResidentStatus[] = ['active', 'checkedOut']
const residentFormSchema = z.object({
  fullName: z.string().trim().min(1, t('adminResidents.required')),
  email: z.string().trim().email(t('adminResidents.invalidEmail')),
  phone: z.string(),
  password: z.string(),
  passportNumber: z.string().trim().min(1, t('adminResidents.required')),
  birthDate: z.string().min(1, t('adminResidents.required')),
  organization: z.string(),
  roomId: z.string(),
  checkInDate: z.string(),
}).superRefine((values, context) => {
  if (values.password && values.password.length < 8) {
    context.addIssue({ code: 'custom', path: ['password'], message: t('adminResidents.passwordShort') })
  }
  if (values.birthDate && Number.isNaN(Date.parse(values.birthDate))) {
    context.addIssue({ code: 'custom', path: ['birthDate'], message: t('adminResidents.required') })
  }
})
type ResidentFormValues = z.infer<typeof residentFormSchema>

function ResidentFormDialog({
  open,
  resident,
  onOpenChange,
  onSaved,
}: {
  open: boolean
  resident: ResidentListItemDto | null
  onOpenChange: (open: boolean) => void
  onSaved: () => void
}) {
  const detail = useManagementResident(resident?.id ?? '', open && !!resident)
  const rooms = useManagementRooms({ page: 1, pageSize: 100, onlyWithFreeBeds: true })
  const createResident = useCreateManagementResident()
  const updateResident = useUpdateManagementResident()
  const {
    register,
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ResidentFormValues>({
    resolver: zodResolver(residentFormSchema),
    defaultValues: {
      fullName: '', email: '', phone: '', password: '', passportNumber: '',
      birthDate: '', organization: '', roomId: 'none', checkInDate: '',
    },
  })

  useEffect(() => {
    if (!open) return
    const data = detail.data
    if (resident && !data) return
    reset({
      fullName: data?.fullName ?? '',
      email: data?.email ?? resident?.email ?? '',
      phone: data?.phone ?? '',
      password: '',
      passportNumber: data?.passportNumber ?? '',
      birthDate: data?.birthDate ?? '',
      organization: data?.organization ?? '',
      roomId: 'none',
      checkInDate: data?.checkInDate ?? '',
    })
  }, [detail.data, open, resident, reset])

  const submit = handleSubmit(async (values) => {
    if (!resident && values.password.length < 8) {
      setError('password', { type: 'manual', message: t('adminResidents.passwordShort') })
      return
    }
    try {
      if (resident) {
        await updateResident.mutateAsync({
          id: resident.id,
          payload: {
            fullName: values.fullName,
            phone: values.phone.trim() || null,
            passportNumber: values.passportNumber,
            birthDate: values.birthDate,
            organization: values.organization.trim() || null,
          },
        })
        toast.success(t('adminResidents.saveSuccess'))
      } else {
        await createResident.mutateAsync({
          email: values.email,
          fullName: values.fullName,
          phone: values.phone.trim() || null,
          password: values.password,
          passportNumber: values.passportNumber,
          birthDate: values.birthDate,
          organization: values.organization.trim() || null,
          roomId: values.roomId === 'none' ? null : values.roomId,
          checkInDate: values.roomId === 'none' || !values.checkInDate ? null : values.checkInDate,
        })
        toast.success(t('adminResidents.createSuccess'))
      }
      onOpenChange(false)
      onSaved()
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })

  const errorText = (message?: string) => message ? <span className="text-xs text-destructive" role="alert">{message}</span> : null
  const isPending = isSubmitting || createResident.isPending || updateResident.isPending

  return (
    <Dialog open={open} onClose={() => onOpenChange(false)} title={resident ? t('adminResidents.formEdit') : t('adminResidents.formCreate')}>
      {resident && detail.isPending ? <p className="text-sm text-muted-foreground">{t('common.loading')}</p> : null}
      {resident && detail.isError ? <ErrorState onRetry={() => void detail.refetch()} /> : null}
      {(!resident || detail.data) && (
        <form className="max-h-[75vh] space-y-4 overflow-y-auto pr-1" onSubmit={(event) => { void submit(event) }} noValidate>
          <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.name')}<Input {...register('fullName')} aria-invalid={Boolean(errors.fullName)} />{errorText(errors.fullName?.message)}</label>
          {!resident && <>
            <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.email')}<Input type="email" autoComplete="email" {...register('email')} aria-invalid={Boolean(errors.email)} />{errorText(errors.email?.message)}</label>
            <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.password')}<Input type="password" autoComplete="new-password" {...register('password')} aria-invalid={Boolean(errors.password)} />{errorText(errors.password?.message)}<span className="text-xs text-muted-foreground">{t('adminResidents.passwordHint')}</span></label>
          </>}
          <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.phone')}<Input type="tel" {...register('phone')} /></label>
          <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.passport')}<Input {...register('passportNumber')} aria-invalid={Boolean(errors.passportNumber)} />{errorText(errors.passportNumber?.message)}</label>
          <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.birthDate')}<Controller control={control} name="birthDate" render={({ field }) => <DatePicker value={field.value} onChange={field.onChange} ariaLabel={t('adminResidents.birthDate')} />} />{errorText(errors.birthDate?.message)}</label>
          <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.organization')}<Input {...register('organization')} /></label>
          {!resident && <>
            <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.initialRoom')}
              <Controller control={control} name="roomId" render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">{t('adminResidents.noRoom')}</SelectItem>
                    {rooms.data?.items.map((room) => <SelectItem key={room.id} value={room.id}>{room.buildingName} · {room.number} ({room.freeBeds})</SelectItem>)}
                  </SelectContent>
                </Select>
              )} />
              {rooms.isError && <span className="text-xs text-destructive">{getErrorMessage(rooms.error)}</span>}
            </label>
            <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.checkInDate')}<Controller control={control} name="checkInDate" render={({ field }) => <DatePicker value={field.value} onChange={field.onChange} ariaLabel={t('adminResidents.checkInDate')} />} /></label>
          </>}
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button>
            <Button type="submit" disabled={isPending}>{isPending ? t('common.loading') : t('common.save')}</Button>
          </div>
        </form>
      )}
    </Dialog>
  )
}

function ResidentDetailsDialog({ resident, open, onOpenChange }: { resident: ResidentListItemDto | null; open: boolean; onOpenChange: (open: boolean) => void }) {
  const detail = useManagementResident(resident?.id ?? '', open && !!resident)
  return (
    <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminResidents.details')}>
      {detail.isPending && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      {detail.isError && <ErrorState onRetry={() => void detail.refetch()} />}
      {detail.data && <ResidentDetails data={detail.data} />}
    </Dialog>
  )
}

function ResidentDetails({ data }: { data: AdminResidentDto }) {
  return (
    <div className="max-h-[70vh] space-y-5 overflow-y-auto">
      <dl className="grid gap-3 sm:grid-cols-2">
        <DetailItem label={t('adminResidents.email')} value={data.email} />
        <DetailItem label={t('adminResidents.phone')} value={data.phone ?? '—'} />
        <DetailItem label={t('adminResidents.passport')} value={data.passportNumber} />
        <DetailItem label={t('adminResidents.birthDate')} value={formatDate(data.birthDate)} />
        <DetailItem label={t('adminResidents.organization')} value={data.organization ?? '—'} />
        <DetailItem label={t('adminResidents.checkInDate')} value={data.checkInDate ? formatDate(data.checkInDate) : '—'} />
        <DetailItem label={t('adminResidents.checkOut')} value={data.checkOutDate ? formatDate(data.checkOutDate) : '—'} />
        <DetailItem label={t('adminResidents.currentRoom')} value={data.currentRoom ? `${data.currentRoom.buildingName} · ${data.currentRoom.roomNumber}, ${data.currentRoom.floor}` : '—'} />
        <DetailItem label={t('adminResidents.active')} value={data.isActive ? t('status.active') : t('adminResidents.inactive')} />
        <DetailItem label={t('adminResidents.totalCharged')} value={<MoneyText value={data.totalCharged} />} />
        <DetailItem label={t('adminResidents.totalPaid')} value={<MoneyText value={data.totalPaid} />} />
        <DetailItem label={t('adminResidents.balance')} value={<MoneyText value={data.balance} balance />} />
      </dl>
      <section>
        <h3 className="mb-2 text-sm font-semibold">{t('adminResidents.occupancyHistory')}</h3>
        {data.occupancyHistory.length ? <ul className="space-y-2 text-sm">{data.occupancyHistory.map((entry, index) => (
          <li key={`${entry.from}-${entry.roomNumber}-${index}`} className="rounded-xl border border-border p-3">
            {entry.buildingName} · {entry.roomNumber} — {formatDate(entry.from)} – {entry.to ? formatDate(entry.to) : '…'}
          </li>
        ))}</ul> : <p className="text-sm text-muted-foreground">{t('adminResidents.noOccupancyHistory')}</p>}
      </section>
    </div>
  )
}

function ResidentCheckInDialog({ resident, open, onOpenChange }: { resident: ResidentListItemDto | null; open: boolean; onOpenChange: (open: boolean) => void }) {
  const rooms = useManagementRooms({ page: 1, pageSize: 100, onlyWithFreeBeds: true })
  const checkIn = useCheckInResident()
  const [today] = useState(() => new Date().toISOString().slice(0, 10))
  const schema = z.object({
    roomId: z.string().min(1, t('adminResidents.roomRequired')),
    date: z.string().min(1, t('adminResidents.required')),
  })
  type Values = z.infer<typeof schema>
  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { roomId: '', date: today },
  })
  useEffect(() => {
    if (open) reset({ roomId: '', date: today })
  }, [open, reset, today])
  const submit = handleSubmit(async (values) => {
    if (!resident) return
    try {
      await checkIn.mutateAsync({ id: resident.id, roomId: values.roomId, date: values.date })
      toast.success(t('adminResidents.checkIn'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminResidents.checkInTitle')}>
    <form className="space-y-4" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.selectRoom')}<Controller control={control} name="roomId" render={({ field }) => (
        <Select value={field.value} onValueChange={field.onChange}><SelectTrigger><SelectValue placeholder={t('adminResidents.selectRoom')} /></SelectTrigger><SelectContent>{rooms.data?.items.map((room) => <SelectItem key={room.id} value={room.id}>{room.buildingName} · {room.number} ({room.freeBeds})</SelectItem>)}</SelectContent></Select>
      )} />{errors.roomId?.message && <span className="text-xs text-destructive" role="alert">{errors.roomId.message}</span>}{rooms.isError && <span className="text-xs text-destructive">{getErrorMessage(rooms.error)}</span>}{!rooms.isPending && !rooms.isError && rooms.data?.items.length === 0 && <span className="text-xs text-muted-foreground">{t('adminResidents.noAvailableRooms')}</span>}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.operationDate')}<Controller control={control} name="date" render={({ field }) => <DatePicker value={field.value} onChange={field.onChange} ariaLabel={t('adminResidents.operationDate')} />} />{errors.date?.message && <span className="text-xs text-destructive" role="alert">{errors.date.message}</span>}</label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={isSubmitting || checkIn.isPending || rooms.isPending || !rooms.data?.items.length}>{isSubmitting || checkIn.isPending ? t('common.loading') : t('adminResidents.checkIn')}</Button></div>
    </form>
  </Dialog>
}

function ResidentCheckOutDialog({ resident, open, onOpenChange }: { resident: ResidentListItemDto | null; open: boolean; onOpenChange: (open: boolean) => void }) {
  const checkOut = useCheckOutResident()
  const [today] = useState(() => new Date().toISOString().slice(0, 10))
  const schema = z.object({ date: z.string().min(1, t('adminResidents.required')) })
  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<{ date: string }>({
    resolver: zodResolver(schema),
    defaultValues: { date: today },
  })
  useEffect(() => {
    if (open) reset({ date: today })
  }, [open, reset, today])
  const submit = handleSubmit(async ({ date }) => {
    if (!resident) return
    try {
      await checkOut.mutateAsync({ id: resident.id, date })
      toast.success(t('adminResidents.checkOut'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminResidents.checkOutTitle')}>
    <form className="space-y-4" onSubmit={(event) => { void submit(event) }} noValidate>
      <p className="text-sm text-muted-foreground">{t('adminResidents.checkOutConfirm')}</p>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.operationDate')}<Controller control={control} name="date" render={({ field }) => <DatePicker value={field.value} onChange={field.onChange} ariaLabel={t('adminResidents.operationDate')} />} />{errors.date?.message && <span className="text-xs text-destructive" role="alert">{errors.date.message}</span>}</label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button variant="destructive" disabled={isSubmitting || checkOut.isPending}>{isSubmitting || checkOut.isPending ? t('common.loading') : t('adminResidents.checkOut')}</Button></div>
    </form>
  </Dialog>
}

function ResidentPasswordDialog({ resident, open, onOpenChange }: { resident: ResidentListItemDto | null; open: boolean; onOpenChange: (open: boolean) => void }) {
  const resetPassword = useResetResidentPassword()
  const schema = z.object({ newPassword: z.string().min(8, t('adminResidents.passwordShort')) })
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<{ newPassword: string }>({
    resolver: zodResolver(schema),
    defaultValues: { newPassword: '' },
  })
  useEffect(() => { if (open) reset({ newPassword: '' }) }, [open, reset])
  const submit = handleSubmit(async ({ newPassword }) => {
    if (!resident) return
    try {
      await resetPassword.mutateAsync({ id: resident.id, newPassword })
      toast.success(t('adminResidents.passwordReset'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminResidents.resetPassword')}>
    <form className="space-y-4" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.newPassword')}<Input type="password" autoComplete="new-password" {...register('newPassword')} aria-invalid={Boolean(errors.newPassword)} />{errors.newPassword?.message && <span className="text-xs text-destructive" role="alert">{errors.newPassword.message}</span>}</label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={isSubmitting || resetPassword.isPending}>{t('common.save')}</Button></div>
    </form>
  </Dialog>
}

function DetailItem({ label, value }: { label: string; value: ReactNode }) {
  return <div><dt className="text-xs text-muted-foreground">{label}</dt><dd className="mt-1 text-sm font-medium">{value}</dd></div>
}

export function AdminResidentsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const { can } = useAuth()
  const [searchDraft, setSearchDraft] = useState(searchParams.get('search') ?? '')
  const [selected, setSelected] = useState<ResidentListItemDto | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [detailsOpen, setDetailsOpen] = useState(false)
  const [checkInOpen, setCheckInOpen] = useState(false)
  const [checkOutOpen, setCheckOutOpen] = useState(false)
  const [passwordOpen, setPasswordOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<ResidentListItemDto | null>(null)
  const [statusTarget, setStatusTarget] = useState<ResidentListItemDto | null>(null)
  const deleteResident = useDeleteManagementResident()
  const blockResident = useSetResidentBlocked()
  const buildings = useManagementBuildings()

  const page = Math.max(1, Number(searchParams.get('page')) || 1)
  const statusValue = searchParams.get('status') ?? ''
  const status = residentStatuses.includes(statusValue as ResidentStatus) ? statusValue as ResidentStatus : undefined
  const buildingId = searchParams.get('buildingId') || undefined
  const debtValue = searchParams.get('hasDebt') ?? ''
  const query: ManagementResidentsQuery = {
    page,
    pageSize: 10,
    search: searchParams.get('search') || undefined,
    status,
    buildingId,
    hasDebt: debtValue === 'true' ? true : debtValue === 'false' ? false : undefined,
    sortBy: 'fullName',
    sortDir: 'asc',
  }
  const residents = useManagementResidents(query)

  useEffect(() => {
    const syncSearchFromHistory = () => setSearchDraft(new URLSearchParams(window.location.search).get('search') ?? '')
    window.addEventListener('popstate', syncSearchFromHistory)
    return () => window.removeEventListener('popstate', syncSearchFromHistory)
  }, [])

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setSearchParams((current) => {
        const next = new URLSearchParams(current)
        const value = searchDraft.trim()
        if (value) next.set('search', value)
        else next.delete('search')
        next.delete('page')
        return next
      }, { replace: true })
    }, 350)
    return () => window.clearTimeout(timer)
  }, [searchDraft, setSearchParams])

  const updateFilter = (key: string, value: string) => setSearchParams((current) => {
    const next = new URLSearchParams(current)
    if (value) next.set(key, value)
    else next.delete(key)
    if (key !== 'page') next.delete('page')
    return next
  }, { replace: true })
  const openForm = (resident: ResidentListItemDto | null) => { setSelected(resident); setFormOpen(true) }
  const openDetails = (resident: ResidentListItemDto) => { setSelected(resident); setDetailsOpen(true) }

  const columns = useMemo<ColumnDef<ResidentListItemDto, unknown>[]>(() => [
    { accessorKey: 'fullName', header: t('adminResidents.name'), cell: ({ row }) => <button className="font-medium text-primary hover:underline" onClick={() => openDetails(row.original)}>{row.original.fullName}</button> },
    { accessorKey: 'email', header: t('adminResidents.email') },
    { accessorKey: 'buildingName', header: t('adminResidents.room'), cell: ({ row }) => row.original.buildingName ? `${row.original.buildingName} · ${row.original.roomNumber ?? '—'}` : '—' },
    { accessorKey: 'status', header: t('adminResidents.statusFilter'), cell: ({ row }) => <StatusBadge status={row.original.status} /> },
    { accessorKey: 'balance', header: t('adminResidents.balance'), cell: ({ row }) => <MoneyText value={row.original.balance} balance /> },
    { id: 'actions', header: t('adminResidents.actions'), enableSorting: false, cell: ({ row }) => (
      <div className="flex flex-wrap gap-2">
        <Button size="sm" variant="outline" onClick={() => { setSelected(row.original); setDetailsOpen(true) }}>{t('adminResidents.details')}</Button>
        {can('residentsManage') && <>
          <Button size="sm" variant="outline" onClick={() => { setSelected(row.original); setFormOpen(true) }}>{t('adminResidents.edit')}</Button>
          <Button size="sm" variant="outline" onClick={() => { setSelected(row.original); setPasswordOpen(true) }}>{t('adminResidents.resetPassword')}</Button>
          {row.original.buildingName ? <Button size="sm" variant="outline" onClick={() => { setSelected(row.original); setCheckOutOpen(true) }}>{t('adminResidents.checkOut')}</Button> : <Button size="sm" variant="outline" onClick={() => { setSelected(row.original); setCheckInOpen(true) }}>{t('adminResidents.checkIn')}</Button>}
          <Button size="sm" variant="outline" onClick={() => setStatusTarget(row.original)}>{row.original.isActive ? t('adminResidents.block') : t('adminResidents.unblock')}</Button>
          <Button size="sm" variant="destructive" onClick={() => setDeleteTarget(row.original)}>{t('adminResidents.delete')}</Button>
        </>}
      </div>
    ) },
  ], [can])

  const confirmDelete = async () => {
    if (!deleteTarget) return
    try {
      await deleteResident.mutateAsync(deleteTarget.id)
      toast.success(t('adminResidents.deleteSuccess'))
      setDeleteTarget(null)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }
  const confirmBlock = async () => {
    if (!statusTarget) return
    try {
      await blockResident.mutateAsync({ id: statusTarget.id, blocked: statusTarget.isActive })
      toast.success(statusTarget.isActive ? t('adminResidents.inactive') : t('adminResidents.active'))
      setStatusTarget(null)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('adminResidents.title')} description={t('adminResidents.description')}
        actions={can('residentsManage') ? <Button onClick={() => openForm(null)}>{t('adminResidents.create')}</Button> : undefined} />
      <FilterBar onReset={() => { setSearchDraft(''); setSearchParams(new URLSearchParams(), { replace: true }) }}>
        <label className="grid gap-1.5 text-sm font-medium sm:col-span-2">{t('common.search')}<Input value={searchDraft} placeholder={t('adminResidents.search')} onChange={(event) => setSearchDraft(event.target.value)} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.statusFilter')}<Select value={status ?? 'all'} onValueChange={(value) => updateFilter('status', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminResidents.allStatuses')}</SelectItem>{residentStatuses.map((value) => <SelectItem key={value} value={value}>{t(`status.${value}`)}</SelectItem>)}</SelectContent></Select></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.buildingFilter')}<Select value={buildingId ?? 'all'} onValueChange={(value) => updateFilter('buildingId', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminResidents.allBuildings')}</SelectItem>{buildings.data?.map((building) => <SelectItem key={building.id} value={building.id}>{building.name}</SelectItem>)}</SelectContent></Select></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminResidents.debtFilter')}<Select value={debtValue || 'all'} onValueChange={(value) => updateFilter('hasDebt', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminResidents.anyDebt')}</SelectItem><SelectItem value="true">{t('adminResidents.withDebt')}</SelectItem><SelectItem value="false">{t('adminResidents.withoutDebt')}</SelectItem></SelectContent></Select></label>
      </FilterBar>
      {residents.isError ? <ErrorState onRetry={() => void residents.refetch()} /> : residents.data?.items.length === 0 && !residents.isPending ? (
        <EmptyState title={t('adminResidents.noResidents')} description={t('adminResidents.noResidentsDescription')} badge={false} />
      ) : <DataTable columns={columns} data={residents.data?.items ?? []} page={page} totalPages={residents.data?.totalPages ?? 1}
        onPageChange={(nextPage) => updateFilter('page', String(nextPage))} loading={residents.isPending} />}
      <ResidentFormDialog open={formOpen} resident={selected} onOpenChange={setFormOpen} onSaved={() => { void residents.refetch() }} />
      <ResidentDetailsDialog resident={selected} open={detailsOpen} onOpenChange={setDetailsOpen} />
      <ResidentCheckInDialog resident={selected} open={checkInOpen} onOpenChange={setCheckInOpen} />
      <ResidentCheckOutDialog resident={selected} open={checkOutOpen} onOpenChange={setCheckOutOpen} />
      <ResidentPasswordDialog resident={selected} open={passwordOpen} onOpenChange={setPasswordOpen} />
      <ConfirmDialog open={!!deleteTarget} onOpenChange={(open) => { if (!open) setDeleteTarget(null) }}
        title={t('adminResidents.delete')} description={t('adminResidents.deleteConfirm')}
        confirmLabel={t('adminResidents.delete')} confirmingLabel={t('common.loading')}
        pending={deleteResident.isPending} onConfirm={() => { void confirmDelete() }} />
      <ConfirmDialog open={!!statusTarget} onOpenChange={(open) => { if (!open) setStatusTarget(null) }}
        title={statusTarget?.isActive ? t('adminResidents.block') : t('adminResidents.unblock')}
        description={statusTarget?.isActive ? t('adminResidents.blockConfirm') : t('adminResidents.unblockConfirm')}
        confirmLabel={statusTarget?.isActive ? t('adminResidents.block') : t('adminResidents.unblock')}
        confirmingLabel={t('common.loading')} pending={blockResident.isPending} onConfirm={() => { void confirmBlock() }} />
    </div>
  )
}
