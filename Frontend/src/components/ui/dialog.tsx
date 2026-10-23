import type { PropsWithChildren } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function Dialog({ open, onClose, title, children }: PropsWithChildren<{ open: boolean; onClose: () => void; title: string }>) {
  if (!open) return null
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/45 p-4" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}><section role="dialog" aria-modal="true" aria-label={title} className="w-full max-w-lg rounded-2xl border border-border bg-card p-6 shadow-xl"><div className="mb-5 flex items-center justify-between"><h2 className="text-lg font-semibold">{title}</h2><Button variant="ghost" size="icon" aria-label="Закрыть" onClick={onClose}><X size={18} /></Button></div>{children}</section></div>
}
