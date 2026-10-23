import { formatMoney } from '@/lib/format'
import { cn } from '@/lib/utils'

export function MoneyText({ value, currency = 'TJS', balance = false }: { value: number; currency?: string; balance?: boolean }) {
  return <span className={cn('font-semibold tabular-nums', balance && value < 0 ? 'text-red-600 dark:text-red-400' : balance && value > 0 ? 'text-emerald-700 dark:text-emerald-400' : '')}>{formatMoney(value, currency)}</span>
}
