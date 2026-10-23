import type { PropsWithChildren } from 'react'
import { Button } from '@/components/ui/button'
import { Dialog } from '@/components/ui/dialog'
import { t } from '@/i18n/ru'

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel,
  confirmingLabel,
  pending = false,
  onConfirm,
  children,
}: PropsWithChildren<{
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description?: string
  confirmLabel: string
  confirmingLabel: string
  pending?: boolean
  onConfirm: () => void
}>) {
  return (
    <Dialog open={open} onClose={() => onOpenChange(false)} title={title}>
      {description && <p className="text-sm leading-6 text-muted-foreground">{description}</p>}
      {children}
      <div className="mt-6 flex justify-end gap-2">
        <Button variant="outline" disabled={pending} onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button>
        <Button variant="destructive" disabled={pending} onClick={onConfirm}>
          {pending ? confirmingLabel : confirmLabel}
        </Button>
      </div>
    </Dialog>
  )
}
