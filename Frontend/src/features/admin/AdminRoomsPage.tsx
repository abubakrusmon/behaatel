import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { Controller, useForm } from 'react-hook-form'
import { useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import {
  useCreateManagementBuilding,
  useCreateManagementRoom,
  useDeleteManagementBuilding,
  useDeleteManagementRoom,
  useManagementBuildings,
  useManagementRooms,
  useUpdateManagementBuilding,
  useUpdateManagementRoom,
} from '@/api/admin/managementHooks'
import { getErrorMessage } from '@/api/errors'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { FilterBar } from '@/components/shared/FilterBar'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { Button } from '@/components/ui/button'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { toast } from '@/lib/toast'
import type { RoomType } from '@/types/api'
import type { AdminBuildingDto, AdminRoomDto, ManagementRoomsQuery } from '@/types/admin'

const roomTypes: RoomType[] = ['standard', 'comfort', 'lux']
const buildingSchema = z.object({
  name: z.string().trim().min(1, t('adminRooms.required')),
  address: z.string().trim().min(1, t('adminRooms.required')),
  floorsCount: z.string().regex(/^[1-9]\d*$/, t('adminRooms.floorsPositive')).transform(Number),
})
type BuildingFormInput = z.input<typeof buildingSchema>
type BuildingFormOutput = z.output<typeof buildingSchema>

const roomSchema = z.object({
  buildingId: z.string().min(1, t('adminRooms.buildingRequired')),
  number: z.string().trim().min(1, t('adminRooms.required')),
  floor: z.string().regex(/^[1-9]\d*$/, t('adminRooms.floorPositive')).transform(Number),
  capacity: z.string().regex(/^[1-9]\d*$/, t('adminRooms.capacityPositive')).transform(Number),
  pricePerBed: z.string().regex(/^\d+(\.\d{1,2})?$/, t('adminRooms.priceNonNegative')).transform(Number),
  type: z.enum(roomTypes),
})
type RoomFormInput = z.input<typeof roomSchema>
type RoomFormOutput = z.output<typeof roomSchema>

function BuildingFormDialog({ open, building, onOpenChange }: { open: boolean; building: AdminBuildingDto | null; onOpenChange: (open: boolean) => void }) {
  const create = useCreateManagementBuilding()
  const update = useUpdateManagementBuilding()
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<BuildingFormInput, unknown, BuildingFormOutput>({
    resolver: zodResolver(buildingSchema),
    defaultValues: { name: '', address: '', floorsCount: '1' },
  })
  useEffect(() => {
    if (open) reset({ name: building?.name ?? '', address: building?.address ?? '', floorsCount: String(building?.floorsCount ?? 1) })
  }, [building, open, reset])
  const submit = handleSubmit(async (values) => {
    try {
      if (building) {
        await update.mutateAsync({ id: building.id, payload: values })
        toast.success(t('adminRooms.saveSuccess'))
      } else {
        await create.mutateAsync(values)
        toast.success(t('adminRooms.buildingCreated'))
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  const pending = isSubmitting || create.isPending || update.isPending
  const errorText = (message?: string) => message ? <span className="text-xs text-destructive" role="alert">{message}</span> : null
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={building ? t('adminRooms.editBuilding') : t('adminRooms.createBuilding')}>
    <form className="space-y-4" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.name')}<Input {...register('name')} aria-invalid={Boolean(errors.name)} />{errorText(errors.name?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.address')}<Input {...register('address')} aria-invalid={Boolean(errors.address)} />{errorText(errors.address?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.floorsCount')}<Input type="number" min="1" step="1" {...register('floorsCount')} aria-invalid={Boolean(errors.floorsCount)} />{errorText(errors.floorsCount?.message)}</label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={pending}>{pending ? t('common.loading') : t('common.save')}</Button></div>
    </form>
  </Dialog>
}

function RoomFormDialog({ open, room, buildings, onOpenChange }: { open: boolean; room: AdminRoomDto | null; buildings: AdminBuildingDto[]; onOpenChange: (open: boolean) => void }) {
  const create = useCreateManagementRoom()
  const update = useUpdateManagementRoom()
  const { register, control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<RoomFormInput, unknown, RoomFormOutput>({
    resolver: zodResolver(roomSchema),
    defaultValues: { buildingId: '', number: '', floor: '1', capacity: '1', pricePerBed: '0', type: 'standard' },
  })
  useEffect(() => {
    if (open) reset({
      buildingId: room?.buildingId ?? buildings[0]?.id ?? '',
      number: room?.number ?? '',
      floor: String(room?.floor ?? 1),
      capacity: String(room?.capacity ?? 1),
      pricePerBed: String(room?.pricePerBed ?? 0),
      type: room?.type ?? 'standard',
    })
  }, [buildings, open, reset, room])
  const submit = handleSubmit(async (values) => {
    try {
      if (room) {
        const { buildingId: _buildingId, ...payload } = values
        await update.mutateAsync({ id: room.id, payload })
        toast.success(t('adminRooms.saveSuccess'))
      } else {
        await create.mutateAsync(values)
        toast.success(t('adminRooms.roomCreated'))
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  const pending = isSubmitting || create.isPending || update.isPending
  const errorText = (message?: string) => message ? <span className="text-xs text-destructive" role="alert">{message}</span> : null
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={room ? t('adminRooms.editRoom') : t('adminRooms.createRoom')}>
    <form className="max-h-[75vh] space-y-4 overflow-y-auto pr-1" onSubmit={(event) => { void submit(event) }} noValidate>
      {!room && <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.building')}<Controller control={control} name="buildingId" render={({ field }) => (
        <Select value={field.value} onValueChange={field.onChange}><SelectTrigger><SelectValue placeholder={t('adminRooms.building')} /></SelectTrigger><SelectContent>{buildings.map((building) => <SelectItem key={building.id} value={building.id}>{building.name}</SelectItem>)}</SelectContent></Select>
      )} />{errorText(errors.buildingId?.message)}</label>}
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.roomNumber')}<Input {...register('number')} aria-invalid={Boolean(errors.number)} />{errorText(errors.number?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.floor')}<Input type="number" min="1" step="1" {...register('floor')} aria-invalid={Boolean(errors.floor)} />{errorText(errors.floor?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.capacityLabel')}<Input type="number" min="1" step="1" {...register('capacity')} aria-invalid={Boolean(errors.capacity)} />{errorText(errors.capacity?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.pricePerBed')}<Input type="number" min="0" step="0.01" {...register('pricePerBed')} aria-invalid={Boolean(errors.pricePerBed)} />{errorText(errors.pricePerBed?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.type')}<Controller control={control} name="type" render={({ field }) => (
        <Select value={field.value} onValueChange={field.onChange}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent>{roomTypes.map((type) => <SelectItem key={type} value={type}>{t(`roomType.${type}`)}</SelectItem>)}</SelectContent></Select>
      )} />{errorText(errors.type?.message)}</label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={pending}>{pending ? t('common.loading') : t('common.save')}</Button></div>
    </form>
  </Dialog>
}

export function AdminRoomsPage() {
  const [params, setParams] = useSearchParams()
  const { can } = useAuth()
  const [searchDraft, setSearchDraft] = useState(params.get('search') ?? '')
  const [buildingFormOpen, setBuildingFormOpen] = useState(false)
  const [roomFormOpen, setRoomFormOpen] = useState(false)
  const [buildingTarget, setBuildingTarget] = useState<AdminBuildingDto | null>(null)
  const [roomTarget, setRoomTarget] = useState<AdminRoomDto | null>(null)
  const [deleteBuildingTarget, setDeleteBuildingTarget] = useState<AdminBuildingDto | null>(null)
  const [deleteRoomTarget, setDeleteRoomTarget] = useState<AdminRoomDto | null>(null)
  const buildings = useManagementBuildings()
  const tab = params.get('tab') === 'rooms' ? 'rooms' : 'buildings'
  const page = Math.max(1, Number(params.get('page')) || 1)
  const buildingId = params.get('buildingId') || undefined
  const floorValue = params.get('floor') ?? ''
  const roomType = params.get('type') ?? ''
  const query: ManagementRoomsQuery = {
    page,
    pageSize: 10,
    search: params.get('search') || undefined,
    buildingId,
    floor: Number(floorValue) || undefined,
    type: roomTypes.includes(roomType as RoomType) ? roomType as RoomType : undefined,
    onlyWithFreeBeds: params.get('onlyWithFreeBeds') === 'true' ? true : undefined,
    sortBy: 'number',
    sortDir: 'asc',
  }
  const rooms = useManagementRooms(query)
  const deleteBuilding = useDeleteManagementBuilding()
  const deleteRoom = useDeleteManagementRoom()

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
    setParams((current) => {
      const next = new URLSearchParams()
      if (current.get('tab') === 'rooms') next.set('tab', 'rooms')
      return next
    }, { replace: true })
  }
  const buildingColumns = useMemo<ColumnDef<AdminBuildingDto, unknown>[]>(() => [
    { accessorKey: 'name', header: t('adminRooms.name') },
    { accessorKey: 'address', header: t('adminRooms.address') },
    { accessorKey: 'floorsCount', header: t('adminRooms.floorsCount') },
    { accessorKey: 'totalRooms', header: t('adminRooms.totalRooms') },
    { accessorKey: 'totalCapacity', header: t('adminRooms.capacity') },
    { accessorKey: 'occupiedBeds', header: t('adminRooms.occupied') },
    { id: 'actions', header: t('adminRooms.actions'), enableSorting: false, cell: ({ row }) => <div className="flex gap-2">
      {can('roomsManage') && <><Button size="sm" variant="outline" onClick={() => { setBuildingTarget(row.original); setBuildingFormOpen(true) }}>{t('adminRooms.editBuilding')}</Button><Button size="sm" variant="destructive" onClick={() => setDeleteBuildingTarget(row.original)}>{t('adminRooms.deleteBuilding')}</Button></>}
    </div> },
  ], [can])
  const roomColumns = useMemo<ColumnDef<AdminRoomDto, unknown>[]>(() => [
    { accessorKey: 'buildingName', header: t('adminRooms.building') },
    { accessorKey: 'number', header: t('adminRooms.roomNumber') },
    { accessorKey: 'floor', header: t('adminRooms.floor') },
    { accessorKey: 'type', header: t('adminRooms.type'), cell: ({ row }) => t(`roomType.${row.original.type}`) },
    { accessorKey: 'capacity', header: t('adminRooms.capacityLabel'), cell: ({ row }) => `${row.original.occupiedBeds} / ${row.original.capacity}` },
    { accessorKey: 'freeBeds', header: t('adminRooms.freeBeds') },
    { accessorKey: 'pricePerBed', header: t('adminRooms.pricePerBed'), cell: ({ row }) => <MoneyText value={row.original.pricePerBed} /> },
    { id: 'actions', header: t('adminRooms.actions'), enableSorting: false, cell: ({ row }) => <div className="flex gap-2">
      {can('roomsManage') && <><Button size="sm" variant="outline" onClick={() => { setRoomTarget(row.original); setRoomFormOpen(true) }}>{t('adminRooms.editRoom')}</Button><Button size="sm" variant="destructive" onClick={() => setDeleteRoomTarget(row.original)}>{t('adminRooms.deleteRoom')}</Button></>}
    </div> },
  ], [can])
  const confirmDeleteBuilding = async () => {
    if (!deleteBuildingTarget) return
    try { await deleteBuilding.mutateAsync(deleteBuildingTarget.id); toast.success(t('adminRooms.deleted')); setDeleteBuildingTarget(null) }
    catch (error) { toast.error(getErrorMessage(error)) }
  }
  const confirmDeleteRoom = async () => {
    if (!deleteRoomTarget) return
    try { await deleteRoom.mutateAsync(deleteRoomTarget.id); toast.success(t('adminRooms.deleted')); setDeleteRoomTarget(null) }
    catch (error) { toast.error(getErrorMessage(error)) }
  }
  const buildingsData = buildings.data ?? []

  return <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
    <PageHeader title={t('adminRooms.title')} description={t('adminRooms.description')} actions={can('roomsManage') ? <Button onClick={() => { if (tab === 'rooms') { setRoomTarget(null); setRoomFormOpen(true) } else { setBuildingTarget(null); setBuildingFormOpen(true) } }}>{tab === 'rooms' ? t('adminRooms.createRoom') : t('adminRooms.createBuilding')}</Button> : undefined} />
    <div className="flex gap-2 border-b border-border">
      <Button variant={tab === 'buildings' ? 'default' : 'ghost'} size="sm" onClick={() => updateParam('tab', '')}>{t('adminRooms.buildingsTab')}</Button>
      <Button variant={tab === 'rooms' ? 'default' : 'ghost'} size="sm" onClick={() => updateParam('tab', 'rooms')}>{t('adminRooms.roomsTab')}</Button>
    </div>
    {tab === 'buildings' ? (
      buildings.isError ? <ErrorState onRetry={() => void buildings.refetch()} /> : buildingsData.length === 0 && !buildings.isPending ? <EmptyState title={t('adminRooms.noBuildings')} description={t('adminRooms.noBuildingsDescription')} badge={false} /> :
        <DataTable columns={buildingColumns} data={buildingsData} page={1} totalPages={1} onPageChange={() => undefined} loading={buildings.isPending} />
    ) : <>
      <FilterBar onReset={clearFilters}>
        <label className="grid gap-1.5 text-sm font-medium sm:col-span-2">{t('common.search')}<Input value={searchDraft} placeholder={t('adminRooms.search')} onChange={(event) => setSearchDraft(event.target.value)} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.building')}<Select value={buildingId ?? 'all'} onValueChange={(value) => updateParam('buildingId', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminRooms.allBuildings')}</SelectItem>{buildingsData.map((building) => <SelectItem key={building.id} value={building.id}>{building.name}</SelectItem>)}</SelectContent></Select></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.typeFilter')}<Select value={roomType || 'all'} onValueChange={(value) => updateParam('type', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminRooms.allTypes')}</SelectItem>{roomTypes.map((type) => <SelectItem key={type} value={type}>{t(`roomType.${type}`)}</SelectItem>)}</SelectContent></Select></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminRooms.floorFilter')}<Input type="number" min="1" step="1" value={floorValue} onChange={(event) => updateParam('floor', event.target.value)} /></label>
        <label className="flex items-center gap-2 text-sm font-medium"><input type="checkbox" checked={params.get('onlyWithFreeBeds') === 'true'} onChange={(event) => updateParam('onlyWithFreeBeds', event.target.checked ? 'true' : '')} />{t('adminRooms.onlyFreeBeds')}</label>
      </FilterBar>
      {rooms.isError ? <ErrorState onRetry={() => void rooms.refetch()} /> : rooms.data?.items.length === 0 && !rooms.isPending ? <EmptyState title={t('adminRooms.noRooms')} description={t('adminRooms.noRoomsDescription')} badge={false} /> :
        <DataTable columns={roomColumns} data={rooms.data?.items ?? []} page={page} totalPages={rooms.data?.totalPages ?? 1} onPageChange={(value) => updateParam('page', String(value))} loading={rooms.isPending} />}
    </>}
    <BuildingFormDialog open={buildingFormOpen} building={buildingTarget} onOpenChange={setBuildingFormOpen} />
    <RoomFormDialog open={roomFormOpen} room={roomTarget} buildings={buildingsData} onOpenChange={setRoomFormOpen} />
    <ConfirmDialog open={!!deleteBuildingTarget} onOpenChange={(open) => { if (!open) setDeleteBuildingTarget(null) }} title={t('adminRooms.deleteBuilding')} description={t('adminRooms.deleteBuildingConfirm')} confirmLabel={t('adminRooms.deleteBuilding')} confirmingLabel={t('common.loading')} pending={deleteBuilding.isPending} onConfirm={() => { void confirmDeleteBuilding() }} />
    <ConfirmDialog open={!!deleteRoomTarget} onOpenChange={(open) => { if (!open) setDeleteRoomTarget(null) }} title={t('adminRooms.deleteRoom')} description={t('adminRooms.deleteRoomConfirm')} confirmLabel={t('adminRooms.deleteRoom')} confirmingLabel={t('common.loading')} pending={deleteRoom.isPending} onConfirm={() => { void confirmDeleteRoom() }} />
  </div>
}
