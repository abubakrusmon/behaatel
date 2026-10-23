import { useLocation } from 'react-router-dom'
import { EmptyState } from '@/components/shared/EmptyState'
import { PageHeader } from '@/components/shared/PageHeader'
import { t, type TranslationKey } from '@/i18n/ru'

const pageByPath: Record<string, { title: TranslationKey; description: TranslationKey }> = {
  '/admin': { title: 'pages.dashboard.title', description: 'pages.dashboard.description' },
  '/admin/residents': { title: 'pages.residents.title', description: 'pages.residents.description' },
  '/admin/rooms': { title: 'pages.rooms.title', description: 'pages.rooms.description' },
  '/admin/charges': { title: 'pages.charges.title', description: 'pages.charges.description' },
  '/admin/payments': { title: 'pages.payments.title', description: 'pages.payments.description' },
  '/admin/debtors': { title: 'pages.debtors.title', description: 'pages.debtors.description' },
  '/admin/announcements': { title: 'pages.announcements.title', description: 'pages.announcements.description' },
  '/admin/admins': { title: 'pages.admins.title', description: 'pages.admins.description' },
  '/admin/audit': { title: 'pages.audit.title', description: 'pages.audit.description' },
  '/admin/settings': { title: 'pages.settings.title', description: 'pages.settings.description' },
  '/app': { title: 'pages.home.title', description: 'pages.home.description' },
  '/app/charges': { title: 'pages.myCharges.title', description: 'pages.myCharges.description' },
  '/app/payments': { title: 'pages.myPayments.title', description: 'pages.myPayments.description' },
  '/app/room': { title: 'pages.myRoom.title', description: 'pages.myRoom.description' },
  '/app/announcements': { title: 'pages.myAnnouncements.title', description: 'pages.myAnnouncements.description' },
  '/app/profile': { title: 'pages.profile.title', description: 'pages.profile.description' },
}

export function PlaceholderPage() {
  const { pathname } = useLocation()
  const page = pageByPath[pathname] ?? pageByPath['/admin']
  return (
    <div className="mx-auto w-full max-w-7xl p-5 sm:p-8">
      <PageHeader title={t(page.title)} description={t(page.description)} />
      <EmptyState />
    </div>
  )
}
