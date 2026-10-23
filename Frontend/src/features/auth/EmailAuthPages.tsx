import { useEffect, useRef, useState, type ClipboardEvent, type KeyboardEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { forgotPassword, register, resendVerification, resetPassword, verifyEmail } from '@/api/auth'
import { getErrorMessage } from '@/api/errors'
import { Logo } from '@/components/shared/Logo'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { t } from '@/i18n/ru'

const inputClass = 'mt-1'

function AuthShell({ title, description, children }: { title: string; description: string; children: React.ReactNode }) {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4 py-10">
      <section className="w-full max-w-lg">
        <div className="mb-8 flex justify-center"><Logo /></div>
        <div className="mb-6 text-center">
          <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
          <p className="mt-2 text-sm leading-6 text-muted-foreground">{description}</p>
        </div>
        <Card className="p-5 sm:p-8">{children}</Card>
      </section>
    </main>
  )
}

export function CodeFields({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  const refs = useRef<Array<HTMLInputElement | null>>([])
  const handlePaste = (event: ClipboardEvent<HTMLInputElement>) => {
    const pasted = event.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6)
    if (!pasted) return
    event.preventDefault()
    onChange(pasted)
    refs.current[Math.min(pasted.length, 5)]?.focus()
  }
  const handleKey = (event: KeyboardEvent<HTMLInputElement>, index: number) => {
    if (event.key === 'Backspace' && !value[index] && index > 0) refs.current[index - 1]?.focus()
    if (event.key === 'ArrowLeft' && index > 0) refs.current[index - 1]?.focus()
    if (event.key === 'ArrowRight' && index < 5) refs.current[index + 1]?.focus()
  }
  return (
    <div className="flex justify-center gap-2" aria-label={t('auth.enterLoginCode')}>
      {Array.from({ length: 6 }, (_, index) => (
        <Input
          key={index}
          ref={(element) => { refs.current[index] = element }}
          aria-label={`${t('auth.enterLoginCode')} ${index + 1}`}
          className="h-12 w-11 px-0 text-center text-xl font-semibold tabular-nums sm:w-14"
          inputMode="numeric"
          autoComplete={index === 0 ? 'one-time-code' : 'off'}
          maxLength={1}
          value={value[index] ?? ''}
          onPaste={handlePaste}
          onKeyDown={(event) => handleKey(event, index)}
          onChange={(event) => {
            const digit = event.target.value.replace(/\D/g, '').slice(-1)
            const next = value.slice(0, index) + digit + value.slice(index + 1)
            onChange(next.slice(0, 6))
            if (digit && index < 5) refs.current[index + 1]?.focus()
          }}
        />
      ))}
    </div>
  )
}

export function RegisterPage() {
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    const values = new FormData(event.currentTarget)
    if (!strongPassword(password)) {
      setError(t('auth.passwordWeak'))
      return
    }
    if (password !== confirm) {
      setError(t('auth.confirmPasswordMismatch'))
      return
    }
    setPending(true)
    try {
      const email = String(values.get('email')).trim()
      await register({
        email,
        fullName: String(values.get('fullName')).trim(),
        phone: String(values.get('phone')).trim() || null,
        password,
        passportNumber: String(values.get('passportNumber')).trim(),
        birthDate: String(values.get('birthDate')),
        organization: String(values.get('organization')).trim() || null,
      })
      navigate('/verify-email', { state: { email }, replace: true })
    } catch (cause) {
      setError(getErrorMessage(cause))
    } finally {
      setPending(false)
    }
  }

  return (
    <AuthShell title={t('auth.registerTitle')} description={t('auth.registerDescription')}>
      <form className="grid gap-4 sm:grid-cols-2" onSubmit={submit}>
        <FormField label={t('user.fullName')} name="fullName" required />
        <FormField label={t('auth.email')} name="email" type="email" required />
        <FormField label={t('auth.phone')} name="phone" type="tel" />
        <FormField label={t('auth.passportNumber')} name="passportNumber" required />
        <FormField label={t('auth.birthDate')} name="birthDate" type="date" required />
        <FormField label={t('auth.organization')} name="organization" />
        <label className="text-sm font-medium sm:col-span-2">{t('auth.password')}
          <Input className={inputClass} type="password" autoComplete="new-password" value={password} onChange={(event) => setPassword(event.target.value)} required />
        </label>
        <ul className="grid gap-1 text-xs text-muted-foreground sm:col-span-2 sm:grid-cols-2">
          {passwordRuleList(password).map((rule) => <li key={rule.label} className={rule.valid ? 'text-emerald-700 dark:text-emerald-400' : ''}>{rule.valid ? '✓' : '•'} {rule.label}</li>)}
        </ul>
        <label className="text-sm font-medium sm:col-span-2">{t('auth.confirmPasswordLabel')}
          <Input className={inputClass} type="password" autoComplete="new-password" value={confirm} onChange={(event) => setConfirm(event.target.value)} required />
        </label>
        {error && <p className="text-sm text-destructive sm:col-span-2" role="alert">{error}</p>}
        <Button className="sm:col-span-2" disabled={pending}>{pending ? t('auth.registering') : t('auth.register')}</Button>
      </form>
      <p className="mt-5 text-center text-sm text-muted-foreground">{t('auth.hasAccount')} <Link className="font-medium text-primary" to="/login">{t('auth.login')}</Link></p>
    </AuthShell>
  )
}

export function VerifyEmailPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const stateEmail = (location.state as { email?: string } | null)?.email ?? ''
  const searchEmail = new URLSearchParams(location.search).get('email') ?? ''
  const [email, setEmail] = useState(stateEmail || searchEmail)
  const [code, setCode] = useState('')
  const [seconds, setSeconds] = useState(60)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [done, setDone] = useState(false)
  const [pending, setPending] = useState(false)

  useEffect(() => {
    if (seconds <= 0) return
    const timer = window.setTimeout(() => setSeconds((value) => value - 1), 1000)
    return () => window.clearTimeout(timer)
  }, [seconds])

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setPending(true)
    try {
      const result = await verifyEmail(email, code)
      setMessage(result.message)
      setDone(true)
    } catch (cause) {
      setError(getErrorMessage(cause))
    } finally {
      setPending(false)
    }
  }
  const resend = async () => {
    setError('')
    try {
      const result = await resendVerification(email)
      setMessage(result.message)
      setSeconds(60)
    } catch (cause) {
      setError(getErrorMessage(cause))
    }
  }

  return (
    <AuthShell title={done ? t('auth.verificationTitle') : t('auth.verificationTitle')} description={done ? message : t('auth.verificationDescription')}>
      {done ? (
        <div className="space-y-5 text-center">
          <p className="text-sm leading-6 text-emerald-700 dark:text-emerald-400">{t('auth.verificationComplete')}</p>
          <Button onClick={() => navigate('/login', { replace: true })}>{t('auth.login')}</Button>
        </div>
      ) : (
        <form className="space-y-5" onSubmit={submit}>
          <label className="block text-sm font-medium">{t('auth.email')}<Input className={inputClass} type="email" value={email} onChange={(event) => setEmail(event.target.value)} required /></label>
          <p className="text-sm text-muted-foreground">{t('auth.verificationSpamHint')}</p>
          <CodeFields value={code} onChange={setCode} />
          {error && <p className="text-center text-sm text-destructive" role="alert">{error}</p>}
          <Button className="w-full" disabled={pending || code.length !== 6}>{pending ? t('common.loading') : t('auth.verify')}</Button>
          <Button className="w-full" type="button" variant="outline" disabled={seconds > 0} onClick={() => void resend()}>
            {seconds ? t('auth.resendAvailableIn').replace('{seconds}', String(seconds)) : t('auth.resendCode')}
          </Button>
        </form>
      )}
    </AuthShell>
  )
}

export function ForgotPasswordPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setPending(true)
    setError('')
    try {
      const result = await forgotPassword(email)
      setMessage(result.message)
      navigate('/reset-password', { state: { email }, replace: true })
    } catch (cause) {
      setError(getErrorMessage(cause))
    } finally {
      setPending(false)
    }
  }
  return (
    <AuthShell title={t('auth.forgotTitle')} description={message || t('auth.forgotDescription')}>
      <form className="space-y-4" onSubmit={submit}>
        <label className="block text-sm font-medium">{t('auth.email')}<Input className={inputClass} type="email" value={email} onChange={(event) => setEmail(event.target.value)} required /></label>
        {error && <p className="text-sm text-destructive" role="alert">{error}</p>}
        <Button className="w-full" disabled={pending}>{pending ? t('common.loading') : t('auth.sendResetCode')}</Button>
      </form>
      <p className="mt-5 text-center text-sm"><Link className="text-primary" to="/login">{t('auth.login')}</Link></p>
    </AuthShell>
  )
}

export function ResetPasswordPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const initialEmail = (location.state as { email?: string } | null)?.email ?? ''
  const [email, setEmail] = useState(initialEmail)
  const [code, setCode] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    if (!strongPassword(password)) return setError(t('auth.passwordWeak'))
    if (confirm !== password) return setError(t('auth.confirmPasswordMismatch'))
    setPending(true)
    try {
      const result = await resetPassword(email, code, password)
      setError('')
      navigate('/login', { replace: true, state: { message: result.message } })
    } catch (cause) {
      setError(getErrorMessage(cause))
    } finally {
      setPending(false)
    }
  }
  return (
    <AuthShell title={t('auth.resetTitle')} description={t('auth.resetDescription')}>
      <form className="space-y-4" onSubmit={submit}>
        <label className="block text-sm font-medium">{t('auth.email')}<Input className={inputClass} type="email" value={email} onChange={(event) => setEmail(event.target.value)} required /></label>
        <CodeFields value={code} onChange={setCode} />
        <label className="block text-sm font-medium">{t('auth.newPassword')}<Input className={inputClass} type="password" autoComplete="new-password" value={password} onChange={(event) => setPassword(event.target.value)} required /></label>
        <label className="block text-sm font-medium">{t('auth.confirmPasswordLabel')}<Input className={inputClass} type="password" autoComplete="new-password" value={confirm} onChange={(event) => setConfirm(event.target.value)} required /></label>
        {error && <p className="text-sm text-destructive" role="alert">{error}</p>}
        <Button className="w-full" disabled={pending || code.length !== 6}>{pending ? t('common.loading') : t('auth.resetPassword')}</Button>
      </form>
    </AuthShell>
  )
}

function FormField({ label, name, type = 'text', required = false }: { label: string; name: string; type?: string; required?: boolean }) {
  return <label className="block text-sm font-medium">{label}<Input className={inputClass} name={name} type={type} required={required} /></label>
}

function passwordRuleList(value: string) {
  return [
    { label: t('auth.passwordRules.length'), valid: value.length >= 8 },
    { label: t('auth.passwordRules.upper'), valid: /[A-Z]/.test(value) },
    { label: t('auth.passwordRules.lower'), valid: /[a-z]/.test(value) },
    { label: t('auth.passwordRules.number'), valid: /\d/.test(value) },
    { label: t('auth.passwordRules.special'), valid: /[^A-Za-z0-9]/.test(value) },
  ]
}

function strongPassword(value: string): boolean {
  return passwordRuleList(value).every((rule) => rule.valid)
}
