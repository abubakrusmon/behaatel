import { LoaderCircle } from 'lucide-react'
import { t } from '@/i18n/ru'

export function Spinner({ label = t('common.loading') }: { label?: string }) {
  return <span className="inline-flex items-center gap-2 text-sm text-muted-foreground"><LoaderCircle aria-hidden="true" className="animate-spin" size={18} /><span>{label}</span></span>
}

export function FullScreenLoader() {
  return (
    <main className="grid min-h-screen place-items-center bg-background">
      <div className="flex flex-col items-center gap-4">
        <img src="/brand/logo-mark.png" alt="BehaAtel" width={60} height={60} className="rounded-xl object-contain" />
        <Spinner />
      </div>
    </main>
  )
}
