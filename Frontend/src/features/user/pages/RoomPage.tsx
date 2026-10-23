import { Building2, MapPin, Users } from 'lucide-react'
import { useMyRoom } from '@/api/meHooks'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading } from '@/features/user/components'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Card } from '@/components/ui/card'
import { t } from '@/i18n/ru'

export function RoomPage() {
  const room = useMyRoom()

  if (room.isPending) return <PageLoading cards={2} />
  if (room.isError) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void room.refetch()} /></div>

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader title={t('pages.myRoom.title')} description={t('pages.myRoom.description')} />
      {!room.data ? (
        <EmptyState title={t('user.roomNotAssignedTitle')} description={t('user.roomNotAssignedDescription')} badge={false} />
      ) : (
        <div className="grid gap-5 lg:grid-cols-[minmax(0,1.15fr)_minmax(280px,0.85fr)]">
          <Card className="overflow-hidden">
            <div className="bg-gradient-to-br from-indigo-600 to-indigo-800 p-7 text-white sm:p-9">
              <div className="grid size-12 place-items-center rounded-2xl bg-white/15"><Building2 aria-hidden="true" size={24} /></div>
              <p className="mt-6 text-sm text-indigo-100">{t('user.building')}</p>
              <h2 className="mt-1 text-2xl font-semibold">{room.data.buildingName}</h2>
              <p className="mt-3 flex items-center gap-2 text-sm text-indigo-100"><MapPin aria-hidden="true" size={16} />{room.data.address}</p>
            </div>
            <div className="grid gap-4 p-6 sm:grid-cols-3">
              <RoomDetail label={t('user.roomNumber')} value={room.data.roomNumber} />
              <RoomDetail label={t('user.floor')} value={String(room.data.floor)} />
              <RoomDetail label={t('user.roomType')} value={t(`roomType.${room.data.type}`)} />
            </div>
          </Card>
          <Card className="p-6">
            <h2 className="flex items-center gap-2 text-lg font-semibold"><Users aria-hidden="true" size={19} />{t('user.neighbors')}</h2>
            {room.data.neighbors.length ? (
              <ul className="mt-5 space-y-4">
                {room.data.neighbors.map((name) => (
                  <li key={name} className="flex items-center gap-3">
                    <Avatar className="size-11"><AvatarFallback>{initials(name)}</AvatarFallback></Avatar>
                    <span className="text-sm font-medium">{name}</span>
                  </li>
                ))}
              </ul>
            ) : <p className="mt-5 text-sm text-muted-foreground">{t('user.noNeighbors')}</p>}
          </Card>
        </div>
      )}
    </div>
  )
}

function RoomDetail({ label, value }: { label: string; value: string }) {
  return <div className="rounded-xl bg-muted/60 p-4"><p className="text-xs text-muted-foreground">{label}</p><p className="mt-1 font-semibold">{value}</p></div>
}

function initials(name: string): string {
  return name.split(/\s+/).slice(0, 2).map((part) => part[0]?.toUpperCase() ?? '').join('')
}
