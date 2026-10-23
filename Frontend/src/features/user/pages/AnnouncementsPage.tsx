import { useState } from 'react'
import { useMyAnnouncements } from '@/api/meHooks'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading, AnnouncementCard } from '@/features/user/components'
import { Button } from '@/components/ui/button'
import { t } from '@/i18n/ru'

export function AnnouncementsPage() {
  const [page, setPage] = useState(1)
  const announcements = useMyAnnouncements({ page, pageSize: 10 })

  if (announcements.isPending) return <PageLoading cards={2} />
  if (announcements.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void announcements.refetch()} /></div>

  return (
    <div className="mx-auto w-full max-w-4xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.myAnnouncements.title')} description={t('pages.myAnnouncements.description')} />
      {announcements.data.items.length ? (
        <>
          <div className="space-y-4">{announcements.data.items.map((item) => <AnnouncementCard key={item.id} announcement={item} />)}</div>
          <div className="flex items-center justify-between gap-3 text-sm text-muted-foreground">
            <span>{t('common.page')} {announcements.data.page} {t('common.of')} {Math.max(1, announcements.data.totalPages)}</span>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" disabled={!announcements.data.hasPrevious} onClick={() => setPage((value) => value - 1)}>{t('common.previousPage')}</Button>
              <Button variant="outline" size="sm" disabled={!announcements.data.hasNext} onClick={() => setPage((value) => value + 1)}>{t('common.nextPage')}</Button>
            </div>
          </div>
        </>
      ) : <EmptyState title={t('user.emptyAnnouncementsTitle')} description={t('user.emptyAnnouncementsDescription')} badge={false} />}
    </div>
  )
}
