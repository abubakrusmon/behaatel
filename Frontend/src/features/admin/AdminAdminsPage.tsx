import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { useForm } from 'react-hook-form'
import { useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import {
  useCreateManagementAdmin,
  useDeleteManagementAdmin,
  useManagementAdmins,
  useResetManagementAdminPassword,
  useSetManagementAdminBlocked,
  useUpdateManagementAdminPermissions,
} from '@/api/admin/managementHooks'
import { getErrorMessage } from '@/api/errors'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { FilterBar } from '@/components/shared/FilterBar'
import { PageHeader } from '@/components/shared/PageHeader'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { formatDateTime } from '@/lib/format'
import { toast } from '@/lib/toast'
import type { Permission } from '@/types/api'
import type { ManagementAdminsQuery, SuperAdminDto } from '@/types/admin'

const permissions = [
  'residentsView', 'residentsManage', 'roomsView', 'roomsManage', 'chargesView',
  'chargesManage', 'paymentsView', 'paymentsManage', 'reportsView', 'announcementsManage',
] as const satisfies readonly Permission[]
const createAdminSchema = z.object({
  fullName: z.string().trim().min(1, t('adminAdmins.nameRequired')),
  email: z.string().trim().email(t('adminAdmins.emailRequired')),
  phone: z.string(),
  password: z.string().min(8, t('adminAdmins.passwordRequired')),
  permissions: z.array(z.enum(permissions)).min(1, t('adminAdmins.permissionsRequired')),
})
type CreateAdminForm = z.infer<typeof createAdminSchema>
const resetPasswordSchema = z.object({ newPassword: z.string().min(8, t('adminAdmins.passwordRequired')) })
type ResetPasswordForm = z.infer<typeof resetPasswordSchema>

function PermissionCheckboxes({ values, onChange }: { values: Permission[]; onChange: (permissions: Permission[]) => void }) {
  return <div className="grid gap-3 sm:grid-cols-2">
    {permissions.map((permission) => <label key={permission} className="flex items-start gap-2 text-sm">
      <input type="checkbox" checked={values.includes(permission)} onChange={(event) => {
        onChange(event.target.checked ? [...values, permission] : values.filter((value) => value !== permission))
      }} />
      {t(`adminAdmins.permission.${permission}`)}
    </label>)}
  </div>
}

function CreateAdminDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const create = useCreateManagementAdmin()
  const { register, handleSubmit, watch, setValue, reset, formState: { errors, isSubmitting } } = useForm<CreateAdminForm>({
    resolver: zodResolver(createAdminSchema),
    defaultValues: { fullName: '', email: '', phone: '', password: '', permissions: [] },
  })
  useEffect(() => {
    if (open) reset({ fullName: '', email: '', phone: '', password: '', permissions: [] })
  }, [open, reset])
  const selectedPermissions = watch('permissions')
  const submit = handleSubmit(async (values) => {
    try {
      await create.mutateAsync({ ...values, phone: values.phone.trim() || null })
      toast.success(t('adminAdmins.createdSuccess'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  const fieldError = (message?: string) => message ? <span className="text-xs text-destructive" role="alert">{message}</span> : null
  const pending = isSubmitting || create.isPending
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminAdmins.createTitle')}>
    <form className="max-h-[75vh] space-y-4 overflow-y-auto pr-1" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAdmins.name')}<Input {...register('fullName')} aria-invalid={Boolean(errors.fullName)} />{fieldError(errors.fullName?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAdmins.email')}<Input type="email" autoComplete="email" {...register('email')} aria-invalid={Boolean(errors.email)} />{fieldError(errors.email?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAdmins.phone')}<Input type="tel" {...register('phone')} /></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAdmins.password')}<Input type="password" autoComplete="new-password" {...register('password')} aria-invalid={Boolean(errors.password)} />{fieldError(errors.password?.message)}</label>
      <fieldset className="space-y-3"><legend className="text-sm font-medium">{t('adminAdmins.permissions')}</legend><PermissionCheckboxes values={selectedPermissions} onChange={(value) => setValue('permissions', value, { shouldValidate: true })} />{fieldError(errors.permissions?.message)}</fieldset>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={pending}>{pending ? t('common.loading') : t('common.save')}</Button></div>
    </form>
  </Dialog>
}

function PermissionsDialog({ admin, open, onOpenChange }: { admin: SuperAdminDto | null; open: boolean; onOpenChange: (open: boolean) => void }) {
  const update = useUpdateManagementAdminPermissions()
  const [selected, setSelected] = useState<Permission[]>(() => admin?.permissions ?? [])
  const save = async () => {
    if (!admin) return
    try {
      await update.mutateAsync({ id: admin.id, permissions: selected })
      toast.success(t('adminAdmins.savedSuccess'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminAdmins.permissionTitle')}>
    <div className="space-y-5">
      <PermissionCheckboxes values={selected} onChange={setSelected} />
      <div className="flex justify-end gap-2"><Button variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={update.isPending} onClick={() => { void save() }}>{update.isPending ? t('common.loading') : t('common.save')}</Button></div>
    </div>
  </Dialog>
}

function ResetAdminPasswordDialog({ admin, open, onOpenChange }: { admin: SuperAdminDto | null; open: boolean; onOpenChange: (open: boolean) => void }) {
  const resetPassword = useResetManagementAdminPassword()
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<ResetPasswordForm>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '' },
  })
  useEffect(() => { if (open) reset({ newPassword: '' }) }, [open, reset])
  const submit = handleSubmit(async ({ newPassword }) => {
    if (!admin) return
    try {
      await resetPassword.mutateAsync({ id: admin.id, newPassword })
      toast.success(t('adminAdmins.resetSuccess'))
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminAdmins.resetPassword')}>
    <form className="space-y-4" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAdmins.newPassword')}<Input type="password" autoComplete="new-password" {...register('newPassword')} aria-invalid={Boolean(errors.newPassword)} />{errors.newPassword?.message && <span className="text-xs text-destructive" role="alert">{errors.newPassword.message}</span>}</label>
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={isSubmitting || resetPassword.isPending}>{t('common.save')}</Button></div>
    </form>
  </Dialog>
}

export function AdminAdminsPage() {
  const [params, setParams] = useSearchParams()
  const { user } = useAuth()
  const [searchDraft, setSearchDraft] = useState(params.get('search') ?? '')
  const [createOpen, setCreateOpen] = useState(false)
  const [permissionTarget, setPermissionTarget] = useState<SuperAdminDto | null>(null)
  const [passwordTarget, setPasswordTarget] = useState<SuperAdminDto | null>(null)
  const [blockTarget, setBlockTarget] = useState<SuperAdminDto | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<SuperAdminDto | null>(null)
  const page = Math.max(1, Number(params.get('page')) || 1)
  const activeValue = params.get('active') ?? ''
  const isActive = activeValue === 'true' ? true : activeValue === 'false' ? false : undefined
  const query: ManagementAdminsQuery = {
    page,
    pageSize: 10,
    search: params.get('search') || undefined,
    isActive,
    sortBy: 'fullName',
    sortDir: 'asc',
  }
  const admins = useManagementAdmins(query)
  const block = useSetManagementAdminBlocked()
  const remove = useDeleteManagementAdmin()

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
  const columns = useMemo<ColumnDef<SuperAdminDto, unknown>[]>(() => [
    { accessorKey: 'fullName', header: t('adminAdmins.name') },
    { accessorKey: 'email', header: t('adminAdmins.email') },
    { accessorKey: 'phone', header: t('adminAdmins.phone'), cell: ({ row }) => row.original.phone ?? '—' },
    { accessorKey: 'isActive', header: t('adminAdmins.statusFilter'), cell: ({ row }) => <StatusBadge status={row.original.isActive ? 'active' : 'inactive'} /> },
    { accessorKey: 'permissions', header: t('adminAdmins.permissions'), cell: ({ row }) => <span className="text-xs">{row.original.permissions.map((value) => t(`adminAdmins.permission.${value}`)).join(', ') || '—'}</span> },
    { accessorKey: 'lastLoginAt', header: t('adminAdmins.lastLogin'), cell: ({ row }) => row.original.lastLoginAt ? formatDateTime(row.original.lastLoginAt) : '—' },
    { id: 'actions', header: t('adminAdmins.actions'), enableSorting: false, cell: ({ row }) => {
      if (row.original.id === user?.id) return null
      return <div className="flex flex-wrap gap-2">
        <Button size="sm" variant="outline" onClick={() => setPermissionTarget(row.original)}>{t('adminAdmins.editPermissions')}</Button>
        <Button size="sm" variant="outline" onClick={() => setPasswordTarget(row.original)}>{t('adminAdmins.resetPassword')}</Button>
        <Button size="sm" variant="outline" onClick={() => setBlockTarget(row.original)}>{row.original.isActive ? t('adminAdmins.block') : t('adminAdmins.unblock')}</Button>
        <Button size="sm" variant="destructive" onClick={() => setDeleteTarget(row.original)}>{t('adminAdmins.delete')}</Button>
      </div>
    } },
  ], [user?.id])
  const confirmBlock = async () => {
    if (!blockTarget) return
    try {
      await block.mutateAsync({ id: blockTarget.id, blocked: blockTarget.isActive })
      toast.success(blockTarget.isActive ? t('adminAdmins.blockedSuccess') : t('adminAdmins.unblockedSuccess'))
      setBlockTarget(null)
    } catch (error) { toast.error(getErrorMessage(error)) }
  }
  const confirmDelete = async () => {
    if (!deleteTarget) return
    try {
      await remove.mutateAsync(deleteTarget.id)
      toast.success(t('adminAdmins.deletedSuccess'))
      setDeleteTarget(null)
    } catch (error) { toast.error(getErrorMessage(error)) }
  }

  return <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
    <PageHeader title={t('adminAdmins.title')} description={t('adminAdmins.description')} actions={<Button onClick={() => setCreateOpen(true)}>{t('adminAdmins.create')}</Button>} />
    <FilterBar onReset={() => { setSearchDraft(''); updateParam('active', '') }}>
      <label className="grid gap-1.5 text-sm font-medium">{t('common.search')}<Input value={searchDraft} placeholder={t('adminAdmins.search')} onChange={(event) => setSearchDraft(event.target.value)} /></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAdmins.statusFilter')}<Select value={activeValue || 'all'} onValueChange={(value) => updateParam('active', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminAdmins.allStatuses')}</SelectItem><SelectItem value="true">{t('adminAdmins.activeOnly')}</SelectItem><SelectItem value="false">{t('adminAdmins.blockedOnly')}</SelectItem></SelectContent></Select></label>
    </FilterBar>
    {admins.isError ? <ErrorState onRetry={() => void admins.refetch()} /> : admins.data?.items.length === 0 && !admins.isPending ? <EmptyState title={t('adminAdmins.noAdmins')} description={t('adminAdmins.noAdminsDescription')} badge={false} /> :
      <DataTable columns={columns} data={admins.data?.items ?? []} page={page} totalPages={admins.data?.totalPages ?? 1} onPageChange={(value) => updateParam('page', String(value))} loading={admins.isPending} />}
    <CreateAdminDialog open={createOpen} onOpenChange={setCreateOpen} />
    <PermissionsDialog key={permissionTarget?.id ?? 'none'} admin={permissionTarget} open={!!permissionTarget} onOpenChange={(open) => { if (!open) setPermissionTarget(null) }} />
    <ResetAdminPasswordDialog admin={passwordTarget} open={!!passwordTarget} onOpenChange={(open) => { if (!open) setPasswordTarget(null) }} />
    <ConfirmDialog open={!!blockTarget} onOpenChange={(open) => { if (!open) setBlockTarget(null) }} title={blockTarget?.isActive ? t('adminAdmins.block') : t('adminAdmins.unblock')} description={blockTarget?.isActive ? t('adminAdmins.blockConfirm') : t('adminAdmins.unblockConfirm')} confirmLabel={blockTarget?.isActive ? t('adminAdmins.block') : t('adminAdmins.unblock')} confirmingLabel={t('common.loading')} pending={block.isPending} onConfirm={() => { void confirmBlock() }} />
    <ConfirmDialog open={!!deleteTarget} onOpenChange={(open) => { if (!open) setDeleteTarget(null) }} title={t('adminAdmins.delete')} description={t('adminAdmins.deleteConfirm')} confirmLabel={t('adminAdmins.delete')} confirmingLabel={t('common.loading')} pending={remove.isPending} onConfirm={() => { void confirmDelete() }} />
  </div>
}
