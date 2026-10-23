import type { PropsWithChildren } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { t } from '@/i18n/ru'

export function Sheet({ open, onOpenChange, children, side = 'left' }: PropsWithChildren<{
  open: boolean
  onOpenChange: (open: boolean) => void
  side?: 'left' | 'right'
}>) {
  if (!open) return null
  return <div className="fixed inset-0 z-50"><button aria-label={t('common.close')} className="absolute inset-0 bg-slate-950/45" onClick={() => onOpenChange(false)} /><section role="dialog" aria-modal="true" className={`absolute inset-y-0 w-[min(20rem,88vw)] bg-card shadow-xl ${side === 'left' ? 'left-0' : 'right-0'}`}><Button variant="ghost" size="icon" className="absolute right-3 top-3 z-10" aria-label={t('common.close')} onClick={() => onOpenChange(false)}><X size={18} /></Button>{children}</section></div>
}
