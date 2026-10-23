import { AlertCircle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { t } from '@/i18n/ru'

export function ErrorState({ onRetry }: { onRetry: () => void }) {
  return (
    <div role="alert" className="flex min-h-56 flex-col items-center justify-center gap-3 rounded-2xl border border-border bg-card p-8 text-center">
      <AlertCircle aria-hidden="true" className="text-destructive" size={28} />
      <p className="font-semibold">{t('shared.errorTitle')}</p>
      <Button variant="outline" onClick={onRetry}>{t('common.retry')}</Button>
    </div>
  )
}
