import { useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { Eye, EyeOff, LockKeyhole, Mail, ShieldCheck } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Navigate, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { getErrorMessage } from '@/api/errors'
import { useLogin } from '@/api/hooks'
import { resendLoginCode, verifyLoginCode } from '@/api/auth'
import { CodeFields } from '@/features/auth/EmailAuthPages'
import { Logo } from '@/components/shared/Logo'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { t } from '@/i18n/ru'
import { useAuthStore } from '@/store/authStore'
import type { Role } from '@/types/api'

const schema = z.object({
  email: z.email(t('auth.emailRequired')),
  password: z.string().min(1, t('auth.passwordRequired')),
})

type LoginValues = z.infer<typeof schema>

function homeForRole(role: Role): string {
  return role === 'user' ? '/app' : '/admin'
}

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const user = useAuthStore((state) => state.user)
  const status = useAuthStore((state) => state.status)
  const mutation = useLogin()
  const [showPassword, setShowPassword] = useState(false)
  const [challenge, setChallenge] = useState<{ id: string; email: string } | null>(null)
  const [code, setCode] = useState('')
  const [resendMessage, setResendMessage] = useState('')
  const [codeError, setCodeError] = useState('')
  const [codePending, setCodePending] = useState(false)
  const currentYear = useMemo(() => new Date().getFullYear(), [])
  const { register, handleSubmit, formState: { errors } } = useForm<LoginValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  })

  if (status === 'authenticated' && user) {
    return <Navigate to={user.mustChangePassword ? '/change-password' : homeForRole(user.role)} replace />
  }

  const onSubmit = handleSubmit(async (values) => {
    try {
      const result = await mutation.mutateAsync(values)
      if (result.requiresCode && result.challengeId) {
        setChallenge({ id: result.challengeId, email: result.maskedEmail ?? '' })
        return
      }
      if (result.session) {
        navigate(result.session.user.mustChangePassword ? '/change-password' : homeForRole(result.session.user.role), { replace: true })
      }
    } catch {
      // Ошибка отображается ниже без вывода в консоль.
    }
  })

  const submitCode = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!challenge || code.length !== 6) return
    setCodePending(true)
    setCodeError('')
    try {
      const session = await verifyLoginCode(challenge.id, code)
      useAuthStore.getState().setSession(session)
      navigate(session.user.mustChangePassword ? '/change-password' : homeForRole(session.user.role), { replace: true })
    } catch (error) {
      setCode('')
      setCodeError(getErrorMessage(error))
    } finally {
      setCodePending(false)
    }
  }

  const resendCode = async () => {
    if (!challenge) return
    setResendMessage('')
    try {
      const result = await resendLoginCode(challenge.id)
      setResendMessage(result.message)
    } catch {
      setResendMessage(t('errors.rateLimited'))
    }
  }

  return (
    <main className="grid min-h-screen bg-background lg:grid-cols-[minmax(0,1.05fr)_minmax(440px,0.95fr)]">
      <section className="relative hidden overflow-hidden bg-[#312e81] px-12 py-14 text-white lg:flex lg:flex-col lg:justify-between xl:px-20">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,rgba(129,140,248,0.62),transparent_34%),linear-gradient(135deg,#312e81_0%,#4338ca_55%,#4f46e5_100%)]" />
        <div className="absolute -bottom-40 -right-20 size-[32rem] rounded-full border border-white/10" />
        <div className="absolute -bottom-24 -right-4 size-[23rem] rounded-full border border-white/10" />
        <div className="relative"><Logo light /></div>
        <div className="relative max-w-xl">
          <div className="mb-6 inline-flex size-14 items-center justify-center rounded-2xl border border-white/20 bg-white/10 backdrop-blur"><ShieldCheck size={26} /></div>
          <h1 className="text-4xl font-semibold leading-tight tracking-tight xl:text-5xl">{t('auth.loginBrandHeading')}</h1>
          <p className="mt-5 max-w-lg text-base leading-7 text-indigo-100">{t('auth.loginBrandText')}</p>
        </div>
        <p className="relative text-sm text-indigo-200">© {currentYear} BehaAtel</p>
      </section>

      <section className="flex min-h-screen items-center justify-center px-5 py-10 sm:px-10">
        <div className="w-full max-w-md">
          <div className="mb-10 lg:hidden"><Logo /></div>
          <div className="mb-8">
            <p className="text-sm font-semibold text-primary">{t('common.appDescription')}</p>
            <h2 className="mt-3 text-3xl font-semibold tracking-tight">{challenge ? t('auth.enterLoginCode') : t('auth.welcome')}</h2>
            <p className="mt-2 text-sm text-muted-foreground">{challenge ? `${t('auth.verificationDescription')} ${challenge.email}` : t('auth.loginDescription')}</p>
          </div>
          <Card className="border-0 bg-transparent shadow-none">
            {challenge ? (
              <form onSubmit={submitCode} className="space-y-5">
                <CodeFields value={code} onChange={setCode} />
                {codeError && <div className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-800 dark:bg-red-950 dark:text-red-200" role="alert">{codeError}</div>}
                <Button className="w-full" disabled={codePending || code.length !== 6}>{codePending ? t('common.loading') : t('auth.verify')}</Button>
                <Button className="w-full" type="button" variant="outline" onClick={() => void resendCode()}>{t('auth.resendCode')}</Button>
                {resendMessage && <p className="text-center text-xs text-muted-foreground" role="status">{resendMessage}</p>}
                <button type="button" className="w-full text-sm text-primary" onClick={() => { setChallenge(null); setCode('') }}>{t('common.cancel')}</button>
              </form>
            ) : <form onSubmit={onSubmit} noValidate className="space-y-5">
              <div className="space-y-2">
                <Label htmlFor="email">{t('auth.email')}</Label>
                <div className="relative">
                  <Mail aria-hidden="true" className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground" size={17} />
                  <Input id="email" type="email" autoComplete="username" placeholder="name@example.com" className="pl-10" aria-invalid={Boolean(errors.email)} {...register('email')} />
                </div>
                {errors.email && <p className="text-sm text-destructive" role="alert">{errors.email.message}</p>}
              </div>
              <div className="space-y-2">
                <Label htmlFor="password">{t('auth.password')}</Label>
                <div className="relative">
                  <LockKeyhole aria-hidden="true" className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground" size={17} />
                  <Input id="password" type={showPassword ? 'text' : 'password'} autoComplete="current-password" className="pl-10 pr-12" aria-invalid={Boolean(errors.password)} {...register('password')} />
                  <button type="button" className="absolute right-2 top-1/2 grid size-8 -translate-y-1/2 place-items-center rounded-lg text-muted-foreground hover:bg-muted" aria-label={showPassword ? t('auth.hidePassword') : t('auth.showPassword')} onClick={() => setShowPassword((value) => !value)}>
                    {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
                  </button>
                </div>
                {errors.password && <p className="text-sm text-destructive" role="alert">{errors.password.message}</p>}
              </div>
              {mutation.isError && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200" role="alert">{getErrorMessage(mutation.error)}</div>}
              {(location.state as { message?: string } | null)?.message && <p className="text-sm text-emerald-700" role="status">{(location.state as { message: string }).message}</p>}
              <Button type="submit" className="mt-2 w-full" disabled={mutation.isPending}>
                {mutation.isPending && <span aria-hidden="true" className="size-4 animate-spin rounded-full border-2 border-current border-r-transparent" />}
                {mutation.isPending ? t('auth.loginLoading') : t('auth.login')}
              </Button>
            </form>}
          </Card>
          {!challenge && <div className="mt-4 flex flex-wrap justify-between gap-2 text-sm">
            <Link className="text-primary" to="/forgot-password">{t('auth.forgotPassword')}</Link>
            <span className="text-muted-foreground">{t('auth.noAccount')} <Link className="font-medium text-primary" to="/register">{t('auth.register')}</Link></span>
          </div>}
          <p className="mt-8 text-center text-xs text-muted-foreground">{t('common.appName')} · {t('common.appDescription')}</p>
        </div>
      </section>
    </main>
  )
}
