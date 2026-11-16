import type { ReactNode } from 'react'
import { RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { t } from '@/i18n/ru'
import { cn } from '@/lib/utils'

export function FilterBar({
  children,
  onReset,
  className,
}: {
  children: ReactNode
  onReset?: () => void
  className?: string
}) {
  return (
    <Card className={cn('p-4 sm:p-5', className)}>
      <div className="grid grid-cols-1 items-end gap-3 sm:grid-cols-2 sm:gap-4 xl:grid-cols-4 2xl:grid-cols-6">
        {children}
      </div>
      {onReset && (
        <div className="mt-4 flex justify-end">
          <Button type="button" variant="ghost" className="border border-border" onClick={onReset}>
            <RotateCcw aria-hidden="true" size={16} />
            {t('common.reset')}
          </Button>
        </div>
      )}
    </Card>
  )
}
