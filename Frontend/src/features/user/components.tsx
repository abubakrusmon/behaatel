import { format } from 'date-fns'
import { ru } from 'date-fns/locale'
import { Link } from 'react-router-dom'
import { Card } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatDateTime } from '@/lib/format'
import { t } from '@/i18n/ru'
import type { MyAnnouncementDto, MyPaymentDto } from '@/types/api'
import { MoneyText } from '@/components/shared/MoneyText'
import { StatusBadge } from '@/components/shared/StatusBadge'

export function PageLoading({ cards = 4 }: { cards?: number }) {
  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <div className="space-y-2">
        <Skeleton className="h-8 w-56" />
        <Skeleton className="h-4 w-72 max-w-full" />
      </div>
      <div className={`grid gap-4 ${cards === 4 ? 'sm:grid-cols-2 xl:grid-cols-4' : 'sm:grid-cols-2'}`}>
        {Array.from({ length: cards }, (_, index) => <Skeleton key={index} className="h-28 rounded-2xl" />)}
      </div>
      <div className="grid gap-5 lg:grid-cols-2">
        <Skeleton className="h-72 rounded-2xl" />
        <Skeleton className="h-72 rounded-2xl" />
      </div>
    </div>
  )
}

export function RecentPayment({ payment }: { payment: MyPaymentDto }) {
  return (
    <div className={`flex items-center justify-between gap-4 py-3 ${payment.status === 'cancelled' ? 'opacity-55' : ''}`}>
      <div className="min-w-0">
        <p className={`truncate text-sm font-medium ${payment.status === 'cancelled' ? 'line-through' : ''}`}>{t(`user.paymentMethod.${payment.method}`)}</p>
        <p className="mt-1 text-xs text-muted-foreground">{formatDateTime(payment.paidAt)}</p>
      </div>
      <div className="flex shrink-0 flex-col items-end gap-1">
        <MoneyText value={payment.amount} />
        <StatusBadge status={payment.status} />
      </div>
    </div>
  )
}

export function AnnouncementCard({ announcement, compact = false }: { announcement: MyAnnouncementDto; compact?: boolean }) {
  return (
    <Card className="p-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="font-semibold">{announcement.title}</h3>
        <time className="text-xs text-muted-foreground" dateTime={announcement.publishedAt}>
          {format(new Date(announcement.publishedAt), 'd MMMM yyyy', { locale: ru })}
        </time>
      </div>
      <p className={`mt-3 whitespace-pre-wrap text-sm leading-6 text-muted-foreground ${compact ? 'line-clamp-3' : ''}`}>
        {announcement.body}
      </p>
    </Card>
  )
}

export function ListSectionHeading({ title, to, linkText }: { title: string; to: string; linkText: string }) {
  return (
    <div className="mb-3 flex items-center justify-between gap-3">
      <h2 className="text-lg font-semibold">{title}</h2>
      <Link to={to} className="rounded-lg px-2 py-1 text-sm font-medium text-primary hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
        {linkText}
      </Link>
    </div>
  )
}
