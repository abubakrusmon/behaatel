import { useEffect, useState, type FormEvent } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { testTelegram } from '@/api/admin/management'
import { getSettings, updateSetting } from '@/api/registrations'
import { queryClient } from '@/api/queryClient'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { t } from '@/i18n/ru'
import { toast } from '@/lib/toast'
import type { SettingDto } from '@/types/api'

function BillingSettings({ settings }: { settings: SettingDto[] }) {
  const read = (key: string, fallback: string) => settings.find((setting) => setting.key === key)?.value ?? fallback
  const [dueDay, setDueDay] = useState(read('Billing.DueDay', '10'))
  const [currency, setCurrency] = useState(read('Billing.Currency', 'TJS'))
  const [penalty, setPenalty] = useState(read('Billing.PenaltyPercent', '0'))
  const [error, setError] = useState('')
  useEffect(() => {
    setDueDay(read('Billing.DueDay', '10'))
    setCurrency(read('Billing.Currency', 'TJS'))
    setPenalty(read('Billing.PenaltyPercent', '0'))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [settings])
  const save = useMutation({
    mutationFn: async () => {
      await updateSetting('Billing.DueDay', dueDay.trim())
      await updateSetting('Billing.Currency', currency.trim().toUpperCase())
      await updateSetting('Billing.PenaltyPercent', penalty.trim().replace(',', '.'))
    },
    onSuccess: () => { void queryClient.invalidateQueries({ queryKey: ['admin', 'settings'] }); toast.success(t('adminSettings.saved')) },
    onError: () => toast.error(t('errors.generic')),
  })
  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const day = Number(dueDay)
    const percent = Number(penalty.replace(',', '.'))
    if (!Number.isInteger(day) || day < 1 || day > 28) return setError(t('adminSettings.invalidDueDay'))
    if (!/^[A-Za-z]{3}$/.test(currency.trim())) return setError(t('adminSettings.invalidCurrency'))
    if (!Number.isFinite(percent) || percent < 0 || percent > 100) return setError(t('adminSettings.invalidPenalty'))
    setError('')
    save.mutate()
  }
  return (
    <Card className="p-5 sm:p-7">
      <h2 className="font-semibold">{t('adminSettings.billingTitle')}</h2>
      <p className="mt-1 text-sm text-muted-foreground">{t('adminSettings.billingDescription')}</p>
      <form className="mt-5 grid gap-4 sm:grid-cols-3" onSubmit={submit} noValidate>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminSettings.dueDay')}<Input inputMode="numeric" value={dueDay} onChange={(event) => setDueDay(event.target.value)} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminSettings.currency')}<Input maxLength={3} value={currency} onChange={(event) => setCurrency(event.target.value)} /></label>
        <label className="grid gap-1.5 text-sm font-medium">{t('adminSettings.penalty')}<Input inputMode="decimal" value={penalty} onChange={(event) => setPenalty(event.target.value)} /></label>
        {error && <p className="text-sm text-destructive sm:col-span-3" role="alert">{error}</p>}
        <div className="sm:col-span-3"><Button disabled={save.isPending}>{save.isPending ? t('common.loading') : t('adminSettings.save')}</Button></div>
      </form>
    </Card>
  )
}

const twoFactorSettings = [
  { key: 'Auth.TwoFactorEmail.Admins', label: 'admin.twoFactorAdmins' },
  { key: 'Auth.TwoFactorEmail.Residents', label: 'admin.twoFactorResidents' },
] as const

export function AuthSettingsPage() {
  const settings = useQuery({ queryKey: ['admin', 'settings'], queryFn: getSettings })
  const update = useMutation({
    mutationFn: ({ key, value }: { key: string; value: string }) => updateSetting(key, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'settings'] }),
  })
  const telegramTest = useMutation({
    mutationFn: testTelegram,
    onSuccess: (result) => toast.info(t(result.telegramStatus === 'queued'
      ? 'admin.telegramTestQueued'
      : 'admin.telegramTestNotConfigured')),
  })
  if (settings.isPending) return <div className="mx-auto max-w-4xl space-y-5 p-4 sm:p-7"><div className="h-8 w-56 animate-pulse rounded bg-muted" /><div className="h-40 animate-pulse rounded-2xl bg-muted" /></div>
  if (settings.isError) return <div className="mx-auto max-w-4xl p-4 sm:p-7"><ErrorState onRetry={() => void settings.refetch()} /></div>
  return (
    <div className="mx-auto max-w-4xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.settings.title')} description={t('admin.authSettingsDescription')} />
      <Card className="divide-y divide-border p-5 sm:p-7">
        {twoFactorSettings.map(({ key, label }) => {
          const checked = settings.data.find((setting) => setting.key === key)?.value === 'true'
          return (
            <label key={key} className="flex items-center justify-between gap-4 py-4 first:pt-0 last:pb-0">
              <span className="text-sm font-medium">{t(label)}</span>
              <input type="checkbox" aria-label={t(label)} checked={checked} disabled={update.isPending}
                className="size-5 accent-indigo-600"
                onChange={(event) => update.mutate({ key, value: String(event.target.checked) })} />
            </label>
          )
        })}
        {update.isError && <p className="pt-4 text-sm text-destructive" role="alert">{t('errors.generic')}</p>}
      </Card>
      <BillingSettings settings={settings.data} />
      <Card className="space-y-3 p-5 sm:p-7">
        <div>
          <h2 className="font-semibold">{t('admin.testTelegram')}</h2>
          <p className="mt-1 text-sm text-muted-foreground">{t('admin.telegramNotGuaranteed')}</p>
        </div>
        <Button type="button" variant="outline" disabled={telegramTest.isPending} onClick={() => telegramTest.mutate()}>
          {telegramTest.isPending ? t('common.loading') : t('admin.testTelegram')}
        </Button>
        {telegramTest.isError && <p className="text-sm text-destructive" role="alert">{t('errors.generic')}</p>}
      </Card>
    </div>
  )
}
