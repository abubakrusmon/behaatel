import { useMutation, useQuery } from '@tanstack/react-query'
import { getSettings, updateSetting } from '@/api/registrations'
import { queryClient } from '@/api/queryClient'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card } from '@/components/ui/card'
import { t } from '@/i18n/ru'

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
    </div>
  )
}
