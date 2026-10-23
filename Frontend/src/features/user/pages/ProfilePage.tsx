import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Check, X } from 'lucide-react'
import type { InputHTMLAttributes } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { changePassword } from '@/api/auth'
import { useMyProfile } from '@/api/meHooks'
import { queryClient } from '@/api/queryClient'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading } from '@/features/user/components'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { ThemeToggle } from '@/components/shared/ThemeToggle'
import { toast } from '@/lib/toast'
import { t } from '@/i18n/ru'
import { formatDate } from '@/lib/format'
import { getErrorMessage } from '@/api/errors'
import { useAuthStore } from '@/store/authStore'

const passwordSchema = z.object({
  currentPassword: z.string().min(1, t('auth.passwordRequired')),
  newPassword: z.string()
    .min(8, t('auth.passwordWeak'))
    .regex(/[A-Z]/, t('auth.passwordWeak'))
    .regex(/[a-z]/, t('auth.passwordWeak'))
    .regex(/\d/, t('auth.passwordWeak'))
    .regex(/[^A-Za-z0-9]/, t('auth.passwordWeak')),
  confirmPassword: z.string().min(1, t('auth.passwordRequired')),
}).refine((values) => values.newPassword === values.confirmPassword, {
  path: ['confirmPassword'],
  message: t('auth.confirmPasswordMismatch'),
})

type PasswordValues = z.infer<typeof passwordSchema>

export function ProfilePage() {
  const navigate = useNavigate()
  const profile = useMyProfile()
  const clearSession = useAuthStore((state) => state.clearSession)
  const [serverError, setServerError] = useState<string | null>(null)
  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<PasswordValues>({
    resolver: zodResolver(passwordSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })
  const newPassword = watch('newPassword', '')
  const passwordRules = [
    { key: 'length', valid: newPassword.length >= 8 },
    { key: 'upper', valid: /[A-Z]/.test(newPassword) },
    { key: 'lower', valid: /[a-z]/.test(newPassword) },
    { key: 'number', valid: /\d/.test(newPassword) },
    { key: 'special', valid: /[^A-Za-z0-9]/.test(newPassword) },
  ] as const

  const submit = handleSubmit(async ({ currentPassword, newPassword: updatedPassword }) => {
    setServerError(null)
    try {
      await changePassword({ currentPassword, newPassword: updatedPassword })
      clearSession()
      queryClient.clear()
      toast.success(t('user.passwordChanged'))
      navigate('/login', { replace: true })
    } catch (error) {
      setServerError(getErrorMessage(error))
    }
  })

  if (profile.isPending) return <PageLoading cards={2} />
  if (profile.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void profile.refetch()} /></div>

  const data = profile.data
  return (
    <div className="mx-auto w-full max-w-4xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.profile.title')} description={t('pages.profile.description')} />
      <Card className="p-5 sm:p-7">
        <h2 className="text-lg font-semibold">{t('user.profileDetails')}</h2>
        <dl className="mt-5 grid gap-5 sm:grid-cols-2">
          <ProfileField label={t('auth.email')} value={data.email} />
          <ProfileField label={t('user.fullName')} value={data.fullName} />
          <ProfileField label={t('user.phone')} value={data.phone || '—'} />
          <ProfileField label={t('user.passportNumber')} value={data.passportNumber} />
          <ProfileField label={t('user.organization')} value={data.organization || '—'} />
          <ProfileField label={t('user.checkInDate')} value={formatDate(data.checkInDate)} />
          <ProfileField label={t('user.residentStatus')} value={t(`status.${data.status}`)} />
        </dl>
      </Card>

      <Card className="flex flex-wrap items-center justify-between gap-4 p-5 sm:p-7">
        <div><h2 className="text-lg font-semibold">{t('user.themeSettings')}</h2><p className="mt-1 text-sm text-muted-foreground">{t('common.theme')}</p></div>
        <ThemeToggle />
      </Card>

      <Card className="p-5 sm:p-7">
        <h2 className="text-lg font-semibold">{t('common.changePassword')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('user.changePasswordHint')}</p>
        <form className="mt-5 grid gap-4 sm:grid-cols-2" onSubmit={submit} noValidate>
          <PasswordField id="profile-current-password" label={t('auth.currentPassword')} error={errors.currentPassword?.message} {...register('currentPassword')} />
          <div className="hidden sm:block" />
          <PasswordField id="profile-new-password" label={t('auth.newPassword')} error={errors.newPassword?.message} {...register('newPassword')} />
          <PasswordField id="profile-confirm-password" label={t('auth.confirmPasswordLabel')} error={errors.confirmPassword?.message} {...register('confirmPassword')} />
          <ul className="grid gap-2 text-xs sm:col-span-2 sm:grid-cols-2" aria-label={t('auth.newPassword')}>
            {passwordRules.map(({ key, valid }) => <li key={key} className={`flex items-center gap-2 ${valid ? 'text-emerald-700 dark:text-emerald-400' : 'text-muted-foreground'}`}>{valid ? <Check size={14} /> : <X size={14} />}{t(`auth.passwordRules.${key}`)}</li>)}
          </ul>
          {serverError && <p className="rounded-xl bg-red-50 p-3 text-sm text-red-800 dark:bg-red-950 dark:text-red-200 sm:col-span-2" role="alert">{serverError}</p>}
          <Button type="submit" className="sm:col-span-2 sm:justify-self-start" disabled={isSubmitting}>{isSubmitting ? t('common.loading') : t('auth.updatePassword')}</Button>
        </form>
      </Card>
    </div>
  )
}

function ProfileField({ label, value }: { label: string; value: string }) {
  return <div><dt className="text-xs text-muted-foreground">{label}</dt><dd className="mt-1 break-words text-sm font-medium">{value}</dd></div>
}

interface PasswordFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  id: string
  label: string
  error?: string
}

function PasswordField({ id, label, error, ...inputProps }: PasswordFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>{label}</Label>
      <Input id={id} type="password" autoComplete="current-password" aria-invalid={Boolean(error)} {...inputProps} />
      {error && <p className="text-sm text-destructive" role="alert">{error}</p>}
    </div>
  )
}
