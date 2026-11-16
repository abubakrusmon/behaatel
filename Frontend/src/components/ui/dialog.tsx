import type { PropsWithChildren } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { t } from '@/i18n/ru'

export function Dialog({ open, onClose, title, children }: PropsWithChildren<{ open: boolean; onClose: () => void; title: string }>) {
  if (!open) return null
  return <div className="fixed inset-0 z-50 grid place-items-end bg-slate-950/50 p-0 sm:place-items-center sm:p-4" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}><section role="dialog" aria-modal="true" aria-label={title} className="max-h-[92dvh] w-full overflow-y-auto rounded-t-card border border-border bg-card p-5 text-card-foreground shadow-xl sm:max-w-lg sm:rounded-card sm:p-6"><div className="mb-5 flex items-center justify-between gap-3"><h2 className="text-lg font-semibold">{title}</h2><Button variant="ghost" size="icon" aria-label={t('common.close')} onClick={onClose}><X size={18} /></Button></div>{children}</section></div>
}
