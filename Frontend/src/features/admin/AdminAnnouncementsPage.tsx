import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState } from 'react'
import type { LegacyColumnDef as ColumnDef } from '@tanstack/react-table/legacy'
import { useForm } from 'react-hook-form'
import { useQuery } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import {
  useCreateManagementAnnouncement,
  useDeleteManagementAnnouncement,
  useManagementAnnouncements,
  usePublishManagementAnnouncement,
  useUpdateManagementAnnouncement,
} from '@/api/admin/managementHooks'
import { getNotificationRecipients } from '@/api/admin/management'
import { getErrorMessage } from '@/api/errors'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { DataTable } from '@/components/shared/DataTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { FilterBar } from '@/components/shared/FilterBar'
import { PageHeader } from '@/components/shared/PageHeader'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Dialog } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { t } from '@/i18n/ru'
import { formatDateTime } from '@/lib/format'
import { toast } from '@/lib/toast'
import type { AdminAnnouncementDto, ManagementAnnouncementsQuery } from '@/types/admin'

const announcementSchema = z.object({
  title: z.string().trim().min(3, t('adminAnnouncements.titleRequired')).max(150, t('adminAnnouncements.titleTooLong')),
  body: z.string().trim().min(1, t('adminAnnouncements.bodyRequired')).max(5000, t('adminAnnouncements.bodyTooLong')),
  publishNow: z.boolean(),
  sendTelegram: z.boolean(),
  audienceKind: z.enum(['allResidents', 'building', 'selectedResidents']),
  buildingId: z.string(),
  residentIds: z.array(z.string()),
}).superRefine((value, context) => {
  if (value.audienceKind === 'building' && !value.buildingId) {
    context.addIssue({ code: 'custom', path: ['buildingId'], message: t('adminAnnouncements.buildingRequired') })
  }
  if (value.audienceKind === 'selectedResidents' && value.residentIds.length === 0) {
    context.addIssue({ code: 'custom', path: ['residentIds'], message: t('adminAnnouncements.recipientsRequired') })
  }
})
type AnnouncementFormValues = z.infer<typeof announcementSchema>

function AnnouncementFormDialog({ open, announcement, onOpenChange }: { open: boolean; announcement: AdminAnnouncementDto | null; onOpenChange: (open: boolean) => void }) {
  const create = useCreateManagementAnnouncement()
  const update = useUpdateManagementAnnouncement()
  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } = useForm<AnnouncementFormValues>({
    resolver: zodResolver(announcementSchema),
    defaultValues: { title: '', body: '', publishNow: false, sendTelegram: true, audienceKind: 'allResidents', buildingId: '', residentIds: [] },
  })
  const recipients = useQuery({
    queryKey: ['admin', 'notification-recipients'],
    queryFn: getNotificationRecipients,
    enabled: open && !announcement,
  })
  const audienceKind = watch('audienceKind')
  const buildingId = watch('buildingId')
  const residentIds = watch('residentIds')
  const buildings = useMemo(() => [...new Map((recipients.data ?? []).filter((r) => r.buildingId !== '00000000-0000-0000-0000-000000000000').map((r) => [r.buildingId, r.buildingName])).entries()]
    .map(([id, name]) => ({ id, name })), [recipients.data])
  useEffect(() => {
    if (!open) return
    reset({ title: announcement?.title ?? '', body: announcement?.body ?? '', publishNow: false, sendTelegram: true, audienceKind: 'allResidents', buildingId: '', residentIds: [] })
  }, [announcement, open, reset])
  const submit = handleSubmit(async (values) => {
    try {
      if (announcement) {
        await update.mutateAsync({ id: announcement.id, payload: { title: values.title, body: values.body } })
        toast.success(t('adminAnnouncements.savedSuccess'))
      } else {
        const result = await create.mutateAsync({
          ...values,
          buildingId: values.audienceKind === 'building' ? values.buildingId : null,
          residentIds: values.audienceKind === 'selectedResidents' ? values.residentIds : [],
        })
        toast.success(t('adminAnnouncements.createdSuccess'))
        if (values.publishNow && values.sendTelegram) {
          toast.info(t(result.telegramStatus === 'queued'
            ? 'admin.telegramQueued'
            : 'admin.telegramNotConfigured'))
        }
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  })
  const pending = isSubmitting || create.isPending || update.isPending
  const fieldError = (message?: string) => message ? <span className="text-xs text-destructive" role="alert">{message}</span> : null
  return <Dialog open={open} onClose={() => onOpenChange(false)} title={announcement ? t('adminAnnouncements.edit') : t('adminAnnouncements.create')}>
    <form className="max-h-[75vh] space-y-4 overflow-y-auto pr-1" onSubmit={(event) => { void submit(event) }} noValidate>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAnnouncements.titleField')}<Input {...register('title')} aria-invalid={Boolean(errors.title)} />{fieldError(errors.title?.message)}</label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAnnouncements.body')}<Textarea rows={8} {...register('body')} aria-invalid={Boolean(errors.body)} />{fieldError(errors.body?.message)}</label>
      {!announcement && <>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminAnnouncements.audience')}
          <Select value={audienceKind} onValueChange={(value) => {
            const parsed = z.enum(['allResidents', 'building', 'selectedResidents']).safeParse(value)
            if (!parsed.success) return
            setValue('audienceKind', parsed.data)
            setValue('buildingId', '')
            setValue('residentIds', [])
          }}>
            <SelectTrigger aria-label={t('adminAnnouncements.audience')}><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="allResidents">{t('adminAnnouncements.audienceAll')}</SelectItem>
              <SelectItem value="building">{t('adminAnnouncements.audienceBuilding')}</SelectItem>
              <SelectItem value="selectedResidents">{t('adminAnnouncements.audienceSelected')}</SelectItem>
            </SelectContent>
          </Select>
        </label>
        {audienceKind === 'building' && <label className="grid gap-1.5 text-sm font-medium">{t('adminAnnouncements.selectBuilding')}
          <Select value={buildingId} onValueChange={(value) => setValue('buildingId', value, { shouldValidate: true })}>
            <SelectTrigger aria-label={t('adminAnnouncements.selectBuilding')}><SelectValue placeholder={t('adminAnnouncements.selectBuilding')} /></SelectTrigger>
            <SelectContent>{buildings.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}</SelectContent>
          </Select>
          {fieldError(errors.buildingId?.message)}
        </label>}
        {audienceKind === 'selectedResidents' && <fieldset className="space-y-2">
          <legend className="text-sm font-medium">{t('adminAnnouncements.selectResidents')}</legend>
          <div className="max-h-52 space-y-1 overflow-y-auto rounded-xl border border-border p-2">
            {(recipients.data ?? []).map((r) => <label key={r.residentId} className="flex items-center gap-3 rounded-lg px-2 py-1.5 text-sm hover:bg-muted">
              <Checkbox checked={residentIds.includes(r.residentId)} aria-label={r.fullName}
                onChange={(event) => setValue('residentIds', event.target.checked ? [...new Set([...residentIds, r.residentId])] : residentIds.filter((id) => id !== r.residentId), { shouldValidate: true })} />
              <span className="min-w-0 flex-1 truncate">{r.fullName} <span className="text-xs text-muted-foreground">· {r.buildingName}</span></span>
              {r.telegramConnected && <span className="shrink-0 rounded-full bg-accent px-2 py-0.5 text-xs text-primary">{t('adminAnnouncements.telegramConnected')}</span>}
            </label>)}
            {recipients.data?.length === 0 && <p className="py-3 text-center text-sm text-muted-foreground">{t('adminAnnouncements.noRecipients')}</p>}
          </div>
          {fieldError(errors.residentIds?.message)}
        </fieldset>}
        <label className="flex items-center gap-2 text-sm"><input type="checkbox" {...register('publishNow')} />{t('adminAnnouncements.publishNow')}</label>
        <label className="flex items-center gap-2 text-sm"><input type="checkbox" {...register('sendTelegram')} />{t('adminAnnouncements.sendToTelegram')}</label>
        <p className="text-xs text-muted-foreground">{t('admin.telegramNotGuaranteed')}</p>
      </>}
      <div className="flex justify-end gap-2"><Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button disabled={pending}>{pending ? t('common.loading') : t('common.save')}</Button></div>
    </form>
  </Dialog>
}

export function AdminAnnouncementsPage() {
  const [params, setParams] = useSearchParams()
  const [searchDraft, setSearchDraft] = useState(params.get('search') ?? '')
  const [formOpen, setFormOpen] = useState(false)
  const [selected, setSelected] = useState<AdminAnnouncementDto | null>(null)
  const [publishTarget, setPublishTarget] = useState<AdminAnnouncementDto | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<AdminAnnouncementDto | null>(null)
  const [sendTelegramOnPublish, setSendTelegramOnPublish] = useState(true)
  const page = Math.max(1, Number(params.get('page')) || 1)
  const publication = params.get('published') ?? ''
  const isPublished = publication === 'published' ? true : publication === 'draft' ? false : undefined
  const query: ManagementAnnouncementsQuery = {
    page,
    pageSize: 10,
    search: params.get('search') || undefined,
    isPublished,
    sortBy: 'createdAt',
    sortDir: 'desc',
  }
  const announcements = useManagementAnnouncements(query)
  const publish = usePublishManagementAnnouncement()
  const remove = useDeleteManagementAnnouncement()

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
  const columns = useMemo<ColumnDef<AdminAnnouncementDto, unknown>[]>(() => [
    { accessorKey: 'title', header: t('adminAnnouncements.titleField'), cell: ({ row }) => <div><p className="font-medium">{row.original.title}</p><p className="line-clamp-2 text-xs text-muted-foreground">{row.original.body}</p></div> },
    { accessorKey: 'authorName', header: t('adminAnnouncements.author') },
    { id: 'audience', header: t('adminAnnouncements.audience'), enableSorting: false, cell: ({ row }) => {
      const kind = row.original.audienceKind ?? 'allResidents'
      if (kind === 'building') return t('adminAnnouncements.audienceBuilding')
      if (kind === 'selectedResidents') return `${t('adminAnnouncements.audienceSelected')} (${row.original.residentIds?.length ?? 0})`
      return t('adminAnnouncements.audienceAll')
    } },
    { accessorKey: 'createdAt', header: t('adminAnnouncements.created'), cell: ({ row }) => formatDateTime(row.original.createdAt) },
    { accessorKey: 'publishedAt', header: t('adminAnnouncements.publishedAt'), cell: ({ row }) => row.original.publishedAt ? <StatusBadge status="published" /> : <StatusBadge status="draft" /> },
    { id: 'actions', header: t('adminAnnouncements.actions'), enableSorting: false, cell: ({ row }) => <div className="flex flex-wrap gap-2">
      {!row.original.publishedAt && <>
        <Button size="sm" variant="outline" onClick={() => { setSelected(row.original); setFormOpen(true) }}>{t('adminAnnouncements.edit')}</Button>
        <Button size="sm" onClick={() => setPublishTarget(row.original)}>{t('adminAnnouncements.publish')}</Button>
      </>}
      <Button size="sm" variant="destructive" onClick={() => setDeleteTarget(row.original)}>{t('adminAnnouncements.delete')}</Button>
    </div> },
  ], [])
  const confirmPublish = async () => {
    if (!publishTarget) return
    try {
      const result = await publish.mutateAsync({ id: publishTarget.id, sendTelegram: sendTelegramOnPublish })
      toast.success(t('adminAnnouncements.publishedSuccess'))
      if (sendTelegramOnPublish) {
        toast.info(t(result.telegramStatus === 'queued'
          ? 'admin.telegramQueued'
          : 'admin.telegramNotConfigured'))
      }
      setPublishTarget(null)
    }
    catch (error) { toast.error(getErrorMessage(error)) }
  }
  const confirmDelete = async () => {
    if (!deleteTarget) return
    try { await remove.mutateAsync(deleteTarget.id); toast.success(t('adminAnnouncements.deletedSuccess')); setDeleteTarget(null) }
    catch (error) { toast.error(getErrorMessage(error)) }
  }

  return <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
    <PageHeader title={t('adminAnnouncements.title')} description={t('adminAnnouncements.description')} actions={<Button onClick={() => { setSelected(null); setFormOpen(true) }}>{t('adminAnnouncements.create')}</Button>} />
    <FilterBar onReset={() => { setSearchDraft(''); updateParam('published', '') }}>
      <label className="grid gap-1.5 text-sm font-medium">{t('common.search')}<Input value={searchDraft} placeholder={t('adminAnnouncements.search')} onChange={(event) => setSearchDraft(event.target.value)} /></label>
      <label className="grid gap-1.5 text-sm font-medium">{t('adminAnnouncements.publicationFilter')}<Select value={publication || 'all'} onValueChange={(value) => updateParam('published', value === 'all' ? '' : value)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="all">{t('adminAnnouncements.allStatuses')}</SelectItem><SelectItem value="published">{t('adminAnnouncements.publishedOnly')}</SelectItem><SelectItem value="draft">{t('adminAnnouncements.draftsOnly')}</SelectItem></SelectContent></Select></label>
    </FilterBar>
    <div className="space-y-1">
      <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={sendTelegramOnPublish} onChange={(event) => setSendTelegramOnPublish(event.target.checked)} />{t('adminAnnouncements.sendToTelegram')}</label>
      <p className="text-xs text-muted-foreground">{t('admin.telegramNotGuaranteed')}</p>
    </div>
    {announcements.isError ? <ErrorState onRetry={() => void announcements.refetch()} /> : announcements.data?.items.length === 0 && !announcements.isPending ? <EmptyState title={t('adminAnnouncements.noAnnouncements')} description={t('adminAnnouncements.noAnnouncementsDescription')} badge={false} /> :
      <DataTable columns={columns} data={announcements.data?.items ?? []} page={page} totalPages={announcements.data?.totalPages ?? 1} onPageChange={(value) => updateParam('page', String(value))} loading={announcements.isPending} />}
    <AnnouncementFormDialog open={formOpen} announcement={selected} onOpenChange={setFormOpen} />
    <ConfirmDialog open={!!publishTarget} onOpenChange={(open) => { if (!open) setPublishTarget(null) }} title={t('adminAnnouncements.publish')} description={t('adminAnnouncements.publishNow')} confirmLabel={t('adminAnnouncements.publish')} confirmingLabel={t('common.loading')} pending={publish.isPending} onConfirm={() => { void confirmPublish() }} />
    <ConfirmDialog open={!!deleteTarget} onOpenChange={(open) => { if (!open) setDeleteTarget(null) }} title={t('adminAnnouncements.delete')} description={t('adminAnnouncements.deleteConfirm')} confirmLabel={t('adminAnnouncements.delete')} confirmingLabel={t('common.loading')} pending={remove.isPending} onConfirm={() => { void confirmDelete() }} />
  </div>
}
