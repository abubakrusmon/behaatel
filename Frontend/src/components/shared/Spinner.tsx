import { LoaderCircle } from 'lucide-react'
import { t } from '@/i18n/ru'

export function Spinner({ label = t('common.loading') }: { label?: string }) {
  return <span className="inline-flex items-center gap-2 text-sm text-muted-foreground"><LoaderCircle aria-hidden="true" className="animate-spin" size={18} /><span>{label}</span></span>
}

export function FullScreenLoader() {
  return <main className="grid min-h-screen place-items-center bg-background"><Spinner /></main>
}
