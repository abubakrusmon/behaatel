import { Badge } from '@/components/ui/badge'
import { t } from '@/i18n/ru'
import type { ChargeStatus, PaymentStatus, ResidentStatus } from '@/types/api'

type Status = ChargeStatus | PaymentStatus | ResidentStatus | 'pending' | 'approved' | 'rejected' | 'inactive' | 'published' | 'draft'

const colors: Record<Status, string> = {
  unpaid: 'border-red-200 bg-red-50 text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-300',
  partiallyPaid: 'border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-900 dark:bg-amber-950 dark:text-amber-300',
  paid: 'border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300',
  cancelled: 'bg-muted text-muted-foreground',
  completed: 'border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300',
  active: 'border-indigo-200 bg-indigo-50 text-indigo-700 dark:border-indigo-900 dark:bg-indigo-950 dark:text-indigo-300',
  inactive: 'bg-muted text-muted-foreground',
  published: 'border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300',
  draft: 'bg-muted text-muted-foreground',
  checkedOut: 'bg-muted text-muted-foreground',
  pending: 'border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-900 dark:bg-amber-950 dark:text-amber-300',
  approved: 'border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300',
  rejected: 'bg-muted text-muted-foreground',
}

export function StatusBadge({ status }: { status: Status }) {
  return <Badge className={colors[status]}>{t(`status.${status}`)}</Badge>
}
