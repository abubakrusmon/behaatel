import { useMemo, useState, type FormEvent } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { z } from 'zod'
import { createManagementNotification, getNotificationRecipients } from '@/api/admin/management'
import { getErrorMessage } from '@/api/errors'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { t } from '@/i18n/ru'
import { toast } from '@/lib/toast'
import type { NotificationAudienceKind, NotificationSendResultDto } from '@/types/admin'

const notificationSchema = z.object({
  title: z.string().trim().min(3, t('adminNotifications.titleRequired')).max(150),
  body: z.string().trim().min(1, t('adminNotifications.bodyRequired')).max(5000),
  audienceKind: z.enum(['allResidents', 'building', 'selectedResidents']),
  buildingId: z.string(),
  residentIds: z.array(z.string()),
  sendTelegram: z.boolean(),
}).superRefine((value, context) => {
  if (value.audienceKind === 'building' && !value.buildingId) {
    context.addIssue({ code: 'custom', path: ['buildingId'], message: t('adminNotifications.buildingRequired') })
  }
  if (value.audienceKind === 'selectedResidents' && value.residentIds.length === 0) {
    context.addIssue({ code: 'custom', path: ['residentIds'], message: t('adminNotifications.recipientsRequired') })
  }
})

export function AdminNotificationsPage() {
  const [title, setTitle] = useState('')
  const [body, setBody] = useState('')
  const [audienceKind, setAudienceKind] = useState<NotificationAudienceKind>('allResidents')
  const [buildingId, setBuildingId] = useState('')
  const [residentIds, setResidentIds] = useState<string[]>([])
  const [sendTelegram, setSendTelegram] = useState(false)
  const [formError, setFormError] = useState('')
  const [result, setResult] = useState<NotificationSendResultDto | null>(null)
  const recipients = useQuery({ queryKey: ['admin', 'notification-recipients'], queryFn: getNotificationRecipients })
  const create = useMutation({ mutationFn: createManagementNotification })
  const buildings = useMemo(() => {
    const values = recipients.data ?? []
    return [...new Map(values.filter((recipient) => recipient.buildingId !== '00000000-0000-0000-0000-000000000000').map((recipient) => [recipient.buildingId, recipient.buildingName])).entries()]
      .map(([id, name]) => ({ id, name }))
  }, [recipients.data])

  const toggleResident = (id: string, checked: boolean) => {
    setResidentIds((current) => checked
      ? [...new Set([...current, id])]
      : current.filter((value) => value !== id))
  }

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError('')
    setResult(null)
    const parsed = notificationSchema.safeParse({
      title,
      body,
      audienceKind,
      buildingId,
      residentIds,
      sendTelegram,
    })
    if (!parsed.success) {
      setFormError(parsed.error.issues[0]?.message ?? t('errors.badRequest'))
      return
    }

    try {
      const response = await create.mutateAsync({
        ...parsed.data,
        buildingId: audienceKind === 'building' ? buildingId : null,
        residentIds: audienceKind === 'selectedResidents' ? residentIds : [],
      })
      setResult(response)
      setTitle('')
      setBody('')
      setResidentIds([])
      toast.success(t('adminNotifications.sent').replace('{count}', String(response.recipientCount)))
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  if (recipients.isPending) {
    return <div className="mx-auto max-w-4xl space-y-5 p-4 sm:p-7" aria-label={t('common.loading')}>
      <div className="h-8 w-64 animate-pulse rounded bg-muted" />
      <div className="h-72 animate-pulse rounded-2xl bg-muted" />
    </div>
  }
  if (recipients.isError) {
    return <div className="mx-auto max-w-4xl p-4 sm:p-7"><ErrorState onRetry={() => void recipients.refetch()} /></div>
  }

  const visibleRecipients = audienceKind === 'building'
    ? recipients.data.filter((recipient) => recipient.buildingId === buildingId)
    : recipients.data

  return (
    <div className="mx-auto w-full max-w-4xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('adminNotifications.title')} description={t('adminNotifications.description')} />
      <Card className="p-5 sm:p-7">
        <form className="space-y-5" onSubmit={(event) => { void submit(event) }} noValidate>
          <label className="grid gap-1.5 text-sm font-medium">
            {t('adminNotifications.titleField')}
            <Input value={title} maxLength={150} onChange={(event) => setTitle(event.target.value)} />
          </label>
          <label className="grid gap-1.5 text-sm font-medium">
            {t('adminNotifications.body')}
            <Textarea value={body} maxLength={5000} rows={7} onChange={(event) => setBody(event.target.value)} />
          </label>
          <label className="grid gap-1.5 text-sm font-medium">
            {t('adminNotifications.audience')}
            <Select value={audienceKind} onValueChange={(value) => {
              const parsedValue = z.enum(['allResidents', 'building', 'selectedResidents']).safeParse(value)
              if (!parsedValue.success) return
              setAudienceKind(parsedValue.data)
              setBuildingId('')
              setResidentIds([])
            }}>
              <SelectTrigger aria-label={t('adminNotifications.audience')}><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="allResidents">{t('adminNotifications.allResidents')}</SelectItem>
                <SelectItem value="building">{t('adminNotifications.building')}</SelectItem>
                <SelectItem value="selectedResidents">{t('adminNotifications.selectedResidents')}</SelectItem>
              </SelectContent>
            </Select>
          </label>
          {audienceKind === 'building' && <label className="grid gap-1.5 text-sm font-medium">
            {t('adminNotifications.building')}
            <Select value={buildingId} onValueChange={setBuildingId}>
              <SelectTrigger aria-label={t('adminNotifications.selectBuilding')}><SelectValue placeholder={t('adminNotifications.selectBuilding')} /></SelectTrigger>
              <SelectContent>
                {buildings.map((building) => <SelectItem key={building.id} value={building.id}>{building.name}</SelectItem>)}
              </SelectContent>
            </Select>
          </label>}
          {audienceKind === 'selectedResidents' && <fieldset className="space-y-3">
            <legend className="text-sm font-medium">{t('adminNotifications.selectResidents')}</legend>
            <div className="max-h-64 space-y-2 overflow-y-auto rounded-xl border border-border p-3">
              {visibleRecipients.map((recipient) => (
                <label key={recipient.residentId} className="flex items-center gap-3 rounded-lg px-2 py-2 text-sm hover:bg-muted">
                  <Checkbox
                    checked={residentIds.includes(recipient.residentId)}
                    onChange={(event) => toggleResident(recipient.residentId, event.target.checked)}
                    aria-label={recipient.fullName}
                  />
                  <span className="min-w-0 flex-1">
                    <span className="block truncate font-medium">{recipient.fullName}</span>
                    <span className="block truncate text-xs text-muted-foreground">{recipient.buildingName}</span>
                  </span>
                  {recipient.telegramConnected && <span className="shrink-0 rounded-full bg-accent px-2 py-1 text-xs text-primary">{t('adminNotifications.telegramConnected')}</span>}
                </label>
              ))}
              {visibleRecipients.length === 0 && <p className="py-4 text-center text-sm text-muted-foreground">{t('adminNotifications.noRecipients')}</p>}
            </div>
          </fieldset>}
          <div className="space-y-1">
            <label className="flex items-center gap-2 text-sm">
              <Checkbox checked={sendTelegram} onChange={(event) => setSendTelegram(event.target.checked)} />
              {t('adminNotifications.sendTelegram')}
            </label>
            <p className="text-xs text-muted-foreground">{t('admin.telegramNotGuaranteed')}</p>
          </div>
          {formError && <p className="text-sm text-destructive" role="alert">{formError}</p>}
          {create.isError && <p className="text-sm text-destructive" role="alert">{getErrorMessage(create.error)}</p>}
          <Button disabled={create.isPending}>
            {create.isPending ? t('adminNotifications.sending') : t('adminNotifications.send')}
          </Button>
        </form>
      </Card>
      {result?.telegramStatus && (
        <p className="text-sm text-muted-foreground" role="status">
          {t(result.telegramStatus === 'queued' ? 'admin.telegramQueued' : 'admin.telegramNotConfigured')}
        </p>
      )}
    </div>
  )
}
