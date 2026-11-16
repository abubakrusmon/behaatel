import type { ReactNode } from 'react'
import { ArrowLeft, LockKeyhole, SearchX } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Logo } from '@/components/shared/Logo'
import { t } from '@/i18n/ru'

export function ForbiddenPage() {
  return <AccessPage code="403" icon={<LockKeyhole size={27} />} title={t('shared.noAccessTitle')} description={t('shared.noAccessDescription')} />
}

export function NotFoundPage() {
  return <AccessPage code="404" icon={<SearchX size={27} />} title={t('shared.notFoundTitle')} description={t('shared.notFoundDescription')} />
}

function AccessPage({ code, icon, title, description }: { code: string; icon: ReactNode; title: string; description: string }) {
  return (
    <main className="grid min-h-screen place-items-center bg-background px-5 py-10">
      <div className="flex max-w-md flex-col items-center text-center">
        <Logo size={64} layout="vertical" />
        <div className="mt-14 grid size-16 place-items-center rounded-2xl bg-accent text-primary">{icon}</div>
        <p className="mt-6 text-sm font-semibold tracking-[0.2em] text-primary">{code}</p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight">{title}</h1>
        <p className="mt-3 text-sm leading-6 text-muted-foreground">{description}</p>
        <Button className="mt-7" onClick={() => window.location.assign('/')}><ArrowLeft size={16} />{t('shared.backDashboard')}</Button>
      </div>
    </main>
  )
}
