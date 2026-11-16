import { useRef, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, Check, ClipboardList, CreditCard, House, Languages, LockKeyhole, LogOut, Megaphone, Camera, Palette, Trash2, UserRound, X } from 'lucide-react'
import type { InputHTMLAttributes } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { changePassword, updateAvatar } from '@/api/auth'
import { useLogout } from '@/api/hooks'
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
import { UserAvatar } from '@/components/shared/UserAvatar'
import { fileToAvatarDataUrl } from '@/lib/image'
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
  const refreshToken = useAuthStore((state) => state.refreshToken)
  const avatarUrl = useAuthStore((state) => state.user?.avatarUrl)
  const setUser = useAuthStore((state) => state.setUser)
  const fileInput = useRef<HTMLInputElement>(null)
  const [avatarBusy, setAvatarBusy] = useState(false)
  const logout = useLogout()
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

  const changeAvatar = async (file: File | undefined) => {
    if (!file) return
    setAvatarBusy(true)
    try {
      setUser(await updateAvatar(await fileToAvatarDataUrl(file)))
      toast.success(t('user.avatarSaved'))
    } catch (error) {
      toast.error(error instanceof Error && !('response' in error) ? error.message : getErrorMessage(error))
    } finally {
      setAvatarBusy(false)
      if (fileInput.current) fileInput.current.value = ''
    }
  }

  const removeAvatar = async () => {
    setAvatarBusy(true)
    try {
      setUser(await updateAvatar(null))
      toast.success(t('user.avatarRemoved'))
    } catch (error) {
      toast.error(getErrorMessage(error))
    } finally {
      setAvatarBusy(false)
    }
  }

  if (profile.isPending) return <PageLoading cards={2} />
  if (profile.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void profile.refetch()} /></div>

  const data = profile.data
  return (
    <div className="mx-auto w-full max-w-3xl space-y-5 p-4 pb-8 sm:p-7">
      <PageHeader title={t('pages.profile.title')} description={t('pages.profile.description')} />
      <Card className="p-5 sm:p-6">
        <div className="flex items-center gap-4">
          <div className="relative shrink-0">
            <UserAvatar src={avatarUrl} className="size-20" iconSize={34} />
            <button
              type="button"
              aria-label={t('user.changePhoto')}
              disabled={avatarBusy}
              onClick={() => fileInput.current?.click()}
              className="absolute -bottom-1 -right-1 grid size-8 place-items-center rounded-full border-2 border-card bg-primary text-primary-foreground shadow transition-opacity hover:opacity-90 disabled:opacity-60"
            >
              <Camera size={15} aria-hidden="true" />
            </button>
            <input ref={fileInput} type="file" accept="image/*" className="sr-only" tabIndex={-1} onChange={(event) => { void changeAvatar(event.target.files?.[0]) }} />
          </div>
          <div className="min-w-0">
            <h2 className="truncate text-lg font-semibold">{data.fullName}</h2>
            <p className="mt-1 truncate text-sm text-muted-foreground">{data.email}</p>
            <p className="truncate text-sm text-muted-foreground">{data.phone || '—'}</p>
            {avatarUrl && (
              <button type="button" disabled={avatarBusy} onClick={() => { void removeAvatar() }} className="mt-1.5 inline-flex items-center gap-1.5 text-xs text-destructive hover:underline disabled:opacity-60">
                <Trash2 size={13} aria-hidden="true" />{t('user.removePhoto')}
              </button>
            )}
          </div>
        </div>
        <div className={`mt-4 inline-flex items-center gap-2 rounded-full px-3 py-1.5 text-xs font-medium ${data.status === 'active' ? 'bg-emerald-50 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-300' : 'bg-amber-50 text-amber-900 dark:bg-amber-950/60 dark:text-amber-300'}`}>
          <span className={`size-2 rounded-full ${data.status === 'active' ? 'bg-emerald-500' : 'bg-amber-500'}`} />
          {data.status === 'active' ? t('user.accountActive') : t(`status.${data.status}`)}
        </div>
        <dl className="mt-5 grid gap-3 border-t border-border pt-4 sm:grid-cols-2">
          <ProfileField label={t('user.passportNumber')} value={data.passportNumber} />
          <ProfileField label={t('user.organization')} value={data.organization || '—'} />
          <ProfileField label={t('user.checkInDate')} value={formatDate(data.checkInDate)} />
        </dl>
      </Card>

      <Card className="overflow-hidden p-2 sm:p-3">
        <h2 className="px-3 pb-2 pt-2 text-sm font-semibold text-muted-foreground">{t('user.profileOptions')}</h2>
        <ProfileLink to="/app/payments" icon={CreditCard} label={t('pages.myPayments.title')} />
        <ProfileLink to="/app/charges" icon={ClipboardList} label={t('pages.myCharges.title')} />
        <ProfileLink to="/app/room" icon={House} label={t('pages.myRoom.title')} />
        <ProfileLink to="/app/announcements" icon={Megaphone} label={t('nav.announcements')} />
        <div className="flex min-h-14 flex-wrap items-center justify-between gap-x-3 gap-y-2 rounded-xl px-3 py-2 hover:bg-muted/60">
          <span className="flex shrink-0 items-center gap-3 text-sm font-medium"><Palette size={18} className="shrink-0 text-muted-foreground" />{t('user.themeSettings')}</span>
          <ThemeToggle className="w-full min-w-0 sm:w-[170px]" />
        </div>
        <div className="flex min-h-14 items-center justify-between gap-3 rounded-xl px-3 py-2 text-sm font-medium">
          <span className="flex items-center gap-3"><Languages size={18} className="text-muted-foreground" />{t('user.language')}</span>
          <span className="text-muted-foreground">{t('user.russian')}</span>
        </div>
        <details className="group rounded-xl open:bg-muted/40">
          <summary className="flex min-h-14 cursor-pointer list-none items-center justify-between gap-3 rounded-xl px-3 py-2 text-sm font-medium hover:bg-muted/60">
            <span className="flex items-center gap-3"><LockKeyhole size={18} className="text-muted-foreground" />{t('common.changePassword')}</span>
            <ArrowRight size={17} className="text-muted-foreground transition-transform group-open:rotate-90" />
          </summary>
          <div className="px-3 pb-4 pt-2">
            <p className="text-sm text-muted-foreground">{t('user.changePasswordHint')}</p>
            <form className="mt-4 grid gap-4" onSubmit={submit} noValidate>
              <PasswordField id="profile-current-password" label={t('auth.currentPassword')} error={errors.currentPassword?.message} {...register('currentPassword')} />
              <PasswordField id="profile-new-password" label={t('auth.newPassword')} error={errors.newPassword?.message} {...register('newPassword')} />
              <PasswordField id="profile-confirm-password" label={t('auth.confirmPasswordLabel')} error={errors.confirmPassword?.message} {...register('confirmPassword')} />
              <ul className="grid gap-2 text-xs sm:grid-cols-2" aria-label={t('auth.newPassword')}>
                {passwordRules.map(({ key, valid }) => <li key={key} className={`flex items-center gap-2 ${valid ? 'text-emerald-700 dark:text-emerald-400' : 'text-muted-foreground'}`}>{valid ? <Check size={14} /> : <X size={14} />}{t(`auth.passwordRules.${key}`)}</li>)}
              </ul>
              {serverError && <p className="rounded-control bg-red-50 p-3 text-sm text-red-800 dark:bg-red-950 dark:text-red-200" role="alert">{serverError}</p>}
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? t('common.loading') : t('auth.updatePassword')}</Button>
            </form>
          </div>
        </details>
        <button
          type="button"
          className="flex min-h-14 w-full items-center gap-3 rounded-xl px-3 py-2 text-left text-sm font-medium text-destructive transition-colors hover:bg-destructive/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          onClick={() => logout.mutate({ refreshToken: refreshToken ?? '' })}
          disabled={logout.isPending}
        >
          <LogOut size={18} aria-hidden="true" />{logout.isPending ? t('common.loading') : t('common.logout')}
        </button>
      </Card>
    </div>
  )
}

function ProfileField({ label, value }: { label: string; value: string }) {
  return <div><dt className="text-xs text-muted-foreground">{label}</dt><dd className="mt-1 break-words text-sm font-medium">{value}</dd></div>
}

function ProfileLink({ to, icon: Icon, label }: { to: string; icon: typeof UserRound; label: string }) {
  return (
    <Link to={to} className="flex min-h-14 items-center justify-between gap-3 rounded-xl px-3 py-2 text-sm font-medium transition-colors hover:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
      <span className="flex min-w-0 items-center gap-3"><Icon size={18} className="shrink-0 text-muted-foreground" aria-hidden="true" />{label}</span>
      <ArrowRight size={17} className="shrink-0 text-muted-foreground" aria-hidden="true" />
    </Link>
  )
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
