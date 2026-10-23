import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Check, Eye, EyeOff, X } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { changePassword } from '@/api/auth'
import { getErrorMessage } from '@/api/errors'
import { queryClient } from '@/api/queryClient'
import { Logo } from '@/components/shared/Logo'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { toast } from '@/lib/toast'
import { t } from '@/i18n/ru'
import { useAuthStore } from '@/store/authStore'

const schema = z.object({
  currentPassword: z.string().min(1, t('auth.passwordRequired')),
  newPassword: z.string()
    .min(8, t('auth.passwordWeak'))
    .regex(/[A-Z]/, t('auth.passwordWeak'))
    .regex(/[a-z]/, t('auth.passwordWeak'))
    .regex(/\d/, t('auth.passwordWeak'))
    .regex(/[^A-Za-z0-9]/, t('auth.passwordWeak')),
  confirmNewPassword: z.string().min(1, t('auth.passwordRequired')),
}).refine((data) => data.newPassword === data.confirmNewPassword, {
  path: ['confirmNewPassword'],
  message: t('auth.confirmPasswordMismatch'),
})

type PasswordValues = z.infer<typeof schema>

export function ChangePasswordPage() {
  const navigate = useNavigate()
  const [showPasswords, setShowPasswords] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const clearSession = useAuthStore((state) => state.clearSession)
  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<PasswordValues>({
    resolver: zodResolver(schema),
  })
  const newPassword = watch('newPassword', '')
  const rules = [
    { key: 'length', valid: newPassword.length >= 8 },
    { key: 'upper', valid: /[A-Z]/.test(newPassword) },
    { key: 'lower', valid: /[a-z]/.test(newPassword) },
    { key: 'number', valid: /\d/.test(newPassword) },
    { key: 'special', valid: /[^A-Za-z0-9]/.test(newPassword) },
  ] as const

  const onSubmit = handleSubmit(async ({ currentPassword, newPassword: nextPassword }) => {
    setSubmitError(null)
    try {
      await changePassword({ currentPassword, newPassword: nextPassword })
      clearSession()
      queryClient.clear()
      toast.success(t('auth.passwordChanged'))
      navigate('/login', { replace: true })
    } catch (error) {
      setSubmitError(getErrorMessage(error))
    }
  })

  return (
    <main className="grid min-h-screen place-items-center bg-background px-4 py-10">
      <div className="w-full max-w-md">
        <div className="mb-7 flex items-center justify-between"><Logo /><Link className="rounded-xl px-3 py-2 text-sm text-muted-foreground hover:bg-muted" to="/login">{t('common.logout')}</Link></div>
        <Card className="p-6 sm:p-8">
          <h1 className="text-2xl font-semibold tracking-tight">{t('auth.changePasswordTitle')}</h1>
          <p className="mt-2 text-sm leading-6 text-muted-foreground">{t('auth.changePasswordDescription')}</p>
          <form onSubmit={onSubmit} noValidate className="mt-7 space-y-5">
            {(['currentPassword', 'newPassword', 'confirmNewPassword'] as const).map((name) => {
              const label = name === 'currentPassword' ? t('auth.currentPassword') : name === 'newPassword' ? t('auth.newPassword') : t('auth.confirmPasswordLabel')
              return (
                <div key={name} className="space-y-2">
                  <Label htmlFor={name}>{label}</Label>
                  <div className="relative">
                    <Input id={name} type={showPasswords ? 'text' : 'password'} autoComplete={name === 'currentPassword' ? 'current-password' : 'new-password'} className="pr-12" aria-invalid={Boolean(errors[name])} {...register(name)} />
                    {name === 'currentPassword' && <button type="button" className="absolute right-2 top-1/2 grid size-8 -translate-y-1/2 place-items-center rounded-lg text-muted-foreground hover:bg-muted" aria-label={showPasswords ? t('auth.hidePassword') : t('auth.showPassword')} onClick={() => setShowPasswords((value) => !value)}>{showPasswords ? <EyeOff size={17} /> : <Eye size={17} />}</button>}
                  </div>
                  {errors[name] && <p role="alert" className="text-sm text-destructive">{errors[name]?.message}</p>}
                </div>
              )
            })}
            <ul className="grid grid-cols-1 gap-2 text-xs sm:grid-cols-2" aria-label={t('auth.newPassword')}>
              {rules.map(({ key, valid }) => <li key={key} className={`flex items-center gap-2 ${valid ? 'text-emerald-700 dark:text-emerald-400' : 'text-muted-foreground'}`}>{valid ? <Check size={14} /> : <X size={14} />}{t(`auth.passwordRules.${key}`)}</li>)}
            </ul>
            {(submitError || errors.confirmNewPassword?.message) && <p role="alert" className="rounded-xl bg-red-50 p-3 text-sm text-red-800 dark:bg-red-950 dark:text-red-200">{submitError ?? errors.confirmNewPassword?.message}</p>}
            <Button type="submit" className="w-full" disabled={isSubmitting}>{isSubmitting ? t('common.loading') : t('auth.updatePassword')}</Button>
          </form>
        </Card>
      </div>
    </main>
  )
}
