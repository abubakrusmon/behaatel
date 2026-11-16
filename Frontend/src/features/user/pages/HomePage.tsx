import { useMemo, useState } from 'react'
import { differenceInCalendarDays, parseISO, startOfDay } from 'date-fns'
import { ArrowUpRight, CalendarClock, CircleDollarSign, CreditCard, House, Megaphone, UserRound, Wallet } from 'lucide-react'
import { Link } from 'react-router-dom'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { useMyAnnouncements, useMyCharges, useMyMonthlyChart, useMyPayments, useMyRoom, useMySummary } from '@/api/meHooks'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageLoading, RecentPayment, AnnouncementCard, ListSectionHeading } from '@/features/user/components'
import { StatCard } from '@/components/shared/StatCard'
import { Card } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { formatDate, formatMoney, formatPeriod } from '@/lib/format'
import type { ChargeType } from '@/types/api'

const chargeColors: Record<ChargeType, string> = {
  rent: '#4f46e5',
  utilities: '#10b981',
  penalty: '#f59e0b',
  other: '#64748b',
}

export function HomePage() {
  const { user } = useAuth()
  const [months, setMonths] = useState(6)
  const summary = useMySummary()
  const room = useMyRoom()
  const monthly = useMyMonthlyChart(months)
  const recentCharges = useMyCharges({ page: 1, pageSize: 100 })
  const payments = useMyPayments({ page: 1, pageSize: 5 })
  const announcements = useMyAnnouncements({ page: 1, pageSize: 3 })
  const queries = [summary, room, monthly, recentCharges, payments, announcements]
  const failure = queries.find((query) => query.isError)
  const typeTotals = useMemo(() => {
    const cutoff = new Date()
    cutoff.setMonth(cutoff.getMonth() - 11, 1)
    return (recentCharges.data?.items ?? [])
      .filter((charge) => charge.status !== 'cancelled' && parseISO(charge.period) >= cutoff)
      .reduce<Partial<Record<ChargeType, number>>>((totals, charge) => {
        totals[charge.type] = (totals[charge.type] ?? 0) + charge.amount
        return totals
      }, {})
  }, [recentCharges.data?.items])

  if (failure) return <div className="mx-auto max-w-7xl p-4 sm:p-7"><ErrorState onRetry={() => void failure.refetch()} /></div>
  if (queries.some((query) => query.isPending)) return <PageLoading />
  if (!summary.data || room.data === undefined || !monthly.data || !recentCharges.data || !payments.data || !announcements.data) return <PageLoading />

  const today = startOfDay(new Date())
  const daysToDue = summary.data.nextDueDate
    ? differenceInCalendarDays(parseISO(summary.data.nextDueDate), today)
    : null
  const overdue = daysToDue !== null && daysToDue < 0 && summary.data.totalDebt > 0
  const chartData = monthly.data.map((point) => ({
    ...point,
    label: formatPeriod(point.period),
  }))
  const pieData = Object.entries(typeTotals).map(([type, value]) => ({
    name: t(`user.chargeType.${type as ChargeType}`),
    value: value ?? 0,
    color: chargeColors[type as ChargeType],
  }))
  const chartSummary = chartData.map((point) => `${point.label}: ${formatMoney(point.charged)} / ${formatMoney(point.paid)}`).join('; ')
  const hasMonthlyData = chartData.some((point) => point.charged > 0 || point.paid > 0)

  return (
    <div className="mx-auto w-full max-w-7xl space-y-7 p-4 sm:p-7">
      <PageHeader
        title={t('user.greeting').replace('{name}', user?.fullName.split(' ')[0] ?? '')}
        description={t('user.homeDescription')}
      />

      <section className={`overflow-hidden rounded-card bg-gradient-to-br p-5 text-white shadow-soft sm:p-7 xl:hidden ${summary.data.totalDebt > 0 ? 'from-indigo-700 via-indigo-700 to-violet-800' : 'from-emerald-700 via-teal-700 to-cyan-800'}`}>
        <p className="text-sm font-medium text-white/75">{summary.data.totalDebt > 0 ? t('user.totalDebt') : t('user.balance')}</p>
        <div className="mt-2 flex flex-wrap items-end justify-between gap-4">
          <p className="text-3xl font-semibold tracking-tight"><MoneyText value={summary.data.totalDebt > 0 ? summary.data.totalDebt : summary.data.balance} /></p>
          <Link to="/app/charges" className="inline-flex h-11 items-center gap-2 rounded-control bg-white/15 px-4 text-sm font-semibold text-white transition-colors hover:bg-white/25 focus-visible:outline-white">
            {t('user.goToCharges')}<ArrowUpRight size={17} aria-hidden="true" />
          </Link>
        </div>
        <div className="mt-5 border-t border-white/20 pt-4 text-sm text-white/85">
          {daysToDue === null || summary.data.totalDebt <= 0
            ? t('user.noDebt')
            : `${t('user.nextDueDate')}: ${formatDate(summary.data.nextDueDate)} · ${overdue ? t('user.overdueDays').replace('{days}', String(Math.abs(daysToDue))) : daysToDue === 0 ? t('user.dueToday') : t('user.daysUntilDue').replace('{days}', String(daysToDue))}`}
        </div>
      </section>

      <section className="grid grid-cols-4 gap-2.5 xl:hidden" aria-label={t('user.quickActions')}>
        {[
          { to: '/app/payments', label: 'nav.payments', icon: CreditCard },
          { to: '/app/room', label: 'nav.myRoom', icon: House },
          { to: '/app/announcements', label: 'nav.announcements', icon: Megaphone },
          { to: '/app/profile', label: 'nav.profile', icon: UserRound },
        ].map(({ to, label, icon: Icon }) => (
          <Link key={to} to={to} className="flex min-w-0 flex-col items-center gap-2 rounded-card border border-border bg-card p-3 text-center text-xs font-medium text-foreground shadow-soft transition-colors hover:bg-muted">
            <span className="grid size-10 place-items-center rounded-xl bg-accent text-primary"><Icon size={19} aria-hidden="true" /></span>
            <span className="truncate">{t(label as Parameters<typeof t>[0])}</span>
          </Link>
        ))}
      </section>

      <section className="hidden gap-4 xl:grid xl:grid-cols-4" aria-label={t('pages.home.title')}>
        <StatCard icon={Wallet} label={t('user.dueThisMonth')} value={formatMoney(summary.data.dueThisMonth)} numericValue={summary.data.dueThisMonth} formatValue={formatMoney} />
        <StatCard
          icon={CircleDollarSign}
          label={t('user.balance')}
          value={formatMoney(summary.data.balance)}
          numericValue={summary.data.balance}
          formatValue={(value) => formatMoney(value)}
          color={summary.data.balance >= 0 ? 'emerald' : 'red'}
        />
        <StatCard icon={CreditCard} label={t('user.totalDebt')} value={formatMoney(summary.data.totalDebt)} numericValue={summary.data.totalDebt} formatValue={formatMoney} color={summary.data.totalDebt > 0 ? 'red' : 'emerald'} />
        <StatCard
          icon={CalendarClock}
          label={t('user.nextDueDate')}
          value={daysToDue === null || summary.data.totalDebt <= 0 ? t('user.noDebt') : formatDate(summary.data.nextDueDate)}
          trend={daysToDue === null || summary.data.totalDebt <= 0 ? undefined : overdue
            ? t('user.overdueDays').replace('{days}', String(Math.abs(daysToDue)))
            : daysToDue === 0 ? t('user.dueToday') : t('user.daysUntilDue').replace('{days}', String(daysToDue))}
          color={overdue ? 'red' : daysToDue !== null && daysToDue <= 3 && summary.data.totalDebt > 0 ? 'amber' : 'indigo'}
        />
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:hidden">
        <Link to="/app/room" className="group min-w-0">
          <Card className="h-full p-4 transition-transform group-hover:-translate-y-0.5 sm:p-5">
            <div className="flex items-center gap-3">
              <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-accent text-primary"><House size={20} aria-hidden="true" /></span>
              <div className="min-w-0">
                <p className="text-xs text-muted-foreground">{t('user.myRoomCard')}</p>
                <p className="mt-1 truncate font-semibold">{room.data ? `${room.data.buildingName} · ${room.data.roomNumber}` : t('user.roomNotAssignedTitle')}</p>
              </div>
            </div>
          </Card>
        </Link>
        <Link to="/app/charges" className="group min-w-0">
          <Card className="h-full p-4 transition-transform group-hover:-translate-y-0.5 sm:p-5">
            <p className="text-xs text-muted-foreground">{t('user.nextPayment')}</p>
            <div className="mt-2 flex items-center justify-between gap-3">
              <p className="truncate font-semibold">{daysToDue === null || summary.data.totalDebt <= 0 ? t('user.noDebt') : formatDate(summary.data.nextDueDate)}</p>
              {summary.data.totalDebt > 0 && <MoneyText value={summary.data.dueThisMonth} />}
            </div>
          </Card>
        </Link>
      </section>

      <section className="grid min-w-0 gap-5 xl:grid-cols-[minmax(0,1.6fr)_minmax(280px,1fr)]">
        <Card className="min-w-0 p-4 sm:p-6">
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-lg font-semibold">{t('user.chargedAndPaid')}</h2>
            <Select value={String(months)} onValueChange={(value) => setMonths(Number(value))}>
              <SelectTrigger className="h-9 w-auto min-w-28" aria-label={t('user.chargedAndPaid')}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="6">{t('user.months6')}</SelectItem>
                <SelectItem value="12">{t('user.months12')}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <p className="sr-only">{t('user.chartDescription').replace('{summary}', chartSummary || t('user.noChartData'))}</p>
          {hasMonthlyData ? (
            <div className="h-56 w-full min-w-0 sm:h-72" role="img" aria-label={t('user.chargedAndPaid')}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData} margin={{ top: 8, right: 4, left: -16, bottom: 4 }}>
                  <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="label" tick={{ fill: 'var(--muted-foreground)', fontSize: 11 }} tickLine={false} axisLine={false} />
                  <YAxis tick={{ fill: 'var(--muted-foreground)', fontSize: 11 }} tickLine={false} axisLine={false} />
                  <Tooltip formatter={(value) => formatMoney(Number(value))} contentStyle={{ background: 'var(--card)', borderColor: 'var(--border)', borderRadius: 12, color: 'var(--foreground)' }} />
                  <Bar dataKey="charged" name={t('user.charged')} fill="#4f46e5" radius={[5, 5, 0, 0]} />
                  <Bar dataKey="paid" name={t('user.paid')} fill="#10b981" radius={[5, 5, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : <EmptyState title={t('user.noChartData')} description={t('user.noChartData')} badge={false} />}
          <div className="mt-2 flex justify-center gap-5 text-xs text-muted-foreground">
            <span className="flex items-center gap-2"><i className="size-2.5 rounded-sm bg-indigo-600" />{t('user.charged')}</span>
            <span className="flex items-center gap-2"><i className="size-2.5 rounded-sm bg-emerald-500" />{t('user.paid')}</span>
          </div>
        </Card>

        <Card className="min-w-0 p-4 sm:p-6">
          <h2 className="mb-4 text-lg font-semibold">{t('user.chargeBreakdown')}</h2>
          {pieData.length ? (
            <>
              <div className="h-56 w-full min-w-0" role="img" aria-label={t('user.chargeBreakdown')}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie data={pieData} dataKey="value" nameKey="name" innerRadius="58%" outerRadius="82%" paddingAngle={3}>
                      {pieData.map((item) => <Cell key={item.name} fill={item.color} />)}
                    </Pie>
                    <Tooltip formatter={(value) => formatMoney(Number(value))} contentStyle={{ background: 'var(--card)', borderColor: 'var(--border)', borderRadius: 12, color: 'var(--foreground)' }} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
              <ul className="space-y-2">
                {pieData.map((item) => <li key={item.name} className="flex items-center justify-between gap-3 text-sm"><span className="flex min-w-0 items-center gap-2 truncate"><i className="size-2.5 shrink-0 rounded-full" style={{ backgroundColor: item.color }} />{item.name}</span><MoneyText value={item.value} /></li>)}
              </ul>
            </>
          ) : <EmptyState title={t('user.noChartData')} description={t('user.noChartData')} badge={false} />}
        </Card>
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <div>
          <ListSectionHeading title={t('user.recentPayments')} to="/app/payments" linkText={t('user.allPayments')} />
          <Card className="divide-y divide-border px-5">
            {payments.data.items.length ? payments.data.items.map((payment) => <RecentPayment key={payment.id} payment={payment} />)
              : <EmptyState title={t('user.emptyPaymentsTitle')} description={t('user.emptyPaymentsDescription')} badge={false} />}
          </Card>
        </div>
        <div>
          <ListSectionHeading title={t('user.recentAnnouncements')} to="/app/announcements" linkText={t('user.allAnnouncements')} />
          <div className="space-y-3">
            {announcements.data.items.length ? announcements.data.items.map((announcement) => <AnnouncementCard key={announcement.id} announcement={announcement} compact />)
              : <EmptyState title={t('user.emptyAnnouncementsTitle')} description={t('user.emptyAnnouncementsDescription')} badge={false} />}
          </div>
        </div>
      </section>

      {recentCharges.data.items.length > 0 && (
        <section aria-label={t('user.chargeBreakdown')} className="sr-only">
          {recentCharges.data.items.slice(0, 3).map((charge) => <span key={charge.id}>{charge.type} {charge.amount}</span>)}
        </section>
      )}
    </div>
  )
}
