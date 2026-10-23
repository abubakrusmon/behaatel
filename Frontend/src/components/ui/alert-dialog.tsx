import type { PropsWithChildren } from 'react'
import { Button } from '@/components/ui/button'
import { Dialog } from '@/components/ui/dialog'
import { t } from '@/i18n/ru'

export function AlertDialog({ open, onOpenChange, title, description, confirmLabel = t('common.save'), onConfirm, children }: PropsWithChildren<{
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description?: string
  confirmLabel?: string
  onConfirm: () => void
}>) {
  return (
    <Dialog open={open} onClose={() => onOpenChange(false)} title={title}>
      {description && <p className="text-sm leading-6 text-muted-foreground">{description}</p>}
      {children}
      <div className="mt-6 flex justify-end gap-2"><Button variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button><Button variant="destructive" onClick={onConfirm}>{confirmLabel}</Button></div>
    </Dialog>
  )
}
