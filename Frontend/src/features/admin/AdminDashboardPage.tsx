import { useMemo, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import {
  Activity, BedDouble, Banknote, CreditCard,
  RefreshCw, Users, Wallet,
} from 'lucide-react'
import {
  Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer,
  Tooltip, XAxis, YAxis,
} from 'recharts'
import {
  useBuildingOccupancy,
  useDashboardPaymentMethods,
  useDashboardSummary,
  useRecentPayments,
  useRevenueByMonth,
  useTopDebtors,
} from '@/api/admin/hooks'
import { Card } from '@/components/ui/card'
import { Select } from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { MoneyText } from '@/components/shared/MoneyText'
import { PageHeader } from '@/components/shared/PageHeader'
import { Skeleton } from '@/components/ui/skeleton'
import { StatCard } from '@/components/shared/StatCard'
import { t } from '@/i18n/ru'
import { useAuth } from '@/hooks/useAuth'
import { formatDateTime, formatMoney, formatPeriod } from '@/lib/format'
import type { BuildingOccupancyDto, MethodShareDto, RecentPaymentDto, RevenuePointDto, TopDebtorDto } from '@/types/api'

const paymentMethodColors = ['#4f46e5', '#10b981', '#f59e0b', '#0ea5e9']
const chartTooltipStyle = {
  backgroundColor: 'var(--card)',
  border: '1px solid var(--border)',
  borderRadius: '0.75rem',
  color: 'var(--foreground)',
}

function utcPeriod(period: 'month' | 'year'): [string, string] {
  const now = new Date()
  const start = period === 'month'
    ? new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), 1))
    : new Date(Date.UTC(now.getUTCFullYear(), 0, 1))
  return [start.toISOString(), now.toISOString()]
}

function BlockLoading({ rows = 4 }: { rows?: number }) {
  return <div className="space-y-3 p-5" aria-label={t('common.loading')}>
    {Array.from({ length: rows }, (_, index) => <Skeleton key={index} className="h-5 w-full" />)}
  </div>
}

function ChartCard({ title, children, className = '' }: { title: string; children: ReactNode; className?: string }) {
  return <Card className={`min-w-0 p-5 ${className}`}><h2 className="mb-4 text-base font-semibold">{title}</h2>{children}</Card>
}

function RevenueChart({ data, loading, error, retry }: {
  data: RevenuePointDto[] | undefined
  loading: boolean
  error: boolean
  retry: () => void
}) {
  return (
    <ChartCard title={t('adminFinance.revenueChart')}>
      {loading ? <BlockLoading rows={6} /> : error ? <ErrorState onRetry={retry} /> : !data?.length || data.every((point) => point.charged === 0 && point.paid === 0) ? (
        <EmptyState badge={false} title={t('adminFinance.chartEmpty')} description={t('adminFinance.chartEmptyDescription')} />
      ) : (
        <>
          <p id="revenue-chart-description" className="sr-only">{t('adminFinance.revenueChartDescription')}: {data.map((point) => `${formatPeriod(point.period)}: ${t('adminFinance.charged')} ${formatMoney(point.charged)}, ${t('adminFinance.paid')} ${formatMoney(point.paid)}`).join('; ')}.</p>
          <div role="img" aria-label={t('adminFinance.revenueChart')} aria-describedby="revenue-chart-description" className="h-72 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data} margin={{ top: 8, right: 8, left: 0, bottom: 4 }}>
                <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="period" tickFormatter={formatPeriod} tick={{ fill: 'var(--muted-foreground)', fontSize: 12 }} axisLine={false} tickLine={false} />
                <YAxis tickFormatter={(value: number) => new Intl.NumberFormat('ru-RU', { notation: 'compact' }).format(value)} tick={{ fill: 'var(--muted-foreground)', fontSize: 12 }} axisLine={false} tickLine={false} />
                <Tooltip
                  contentStyle={chartTooltipStyle}
                  labelFormatter={(label) => formatPeriod(String(label))}
                  formatter={(value, name) => [
                    formatMoney(Number(value)),
                    name === 'charged' ? t('adminFinance.charged') : t('adminFinance.paid'),
                  ]}
                />
                <Bar dataKey="charged" name="charged" fill="#4f46e5" radius={[5, 5, 0, 0]} />
                <Bar dataKey="paid" name="paid" fill="#10b981" radius={[5, 5, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </>
      )}
    </ChartCard>
  )
}

function MethodsChart({ data, loading, error, retry }: {
  data: MethodShareDto[] | undefined
  loading: boolean
  error: boolean
  retry: () => void
}) {
  const totalAmount = data?.reduce((sum, item) => sum + item.totalAmount, 0) ?? 0
  return (
    <ChartCard title={t('adminFinance.paymentMethods')}>
      {loading ? <BlockLoading rows={5} /> : error ? <ErrorState onRetry={retry} /> : !data?.length ? (
        <EmptyState badge={false} title={t('adminFinance.chartEmpty')} description={t('adminFinance.chartEmptyDescription')} />
      ) : (
        <>
          <p id="payment-methods-description" className="sr-only">{t('adminFinance.paymentMethodsDescription')}: {data.map((item) => `${t(`paymentMethod.${item.method}`)} ${item.percent}% (${formatMoney(item.totalAmount)})`).join('; ')}.</p>
          <div className="grid items-center gap-4 sm:grid-cols-[minmax(150px,1fr)_minmax(0,1fr)]">
            <div role="img" aria-label={t('adminFinance.paymentMethods')} aria-describedby="payment-methods-description" className="h-56 min-w-0">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie data={data} dataKey="totalAmount" nameKey="method" innerRadius="58%" outerRadius="82%" paddingAngle={3}>
                    {data.map((item, index) => <Cell key={item.method} fill={paymentMethodColors[index % paymentMethodColors.length]} />)}
                  </Pie>
                  <Tooltip
                    contentStyle={chartTooltipStyle}
                    formatter={(value, name) => {
                      const method = data.find((item) => item.method === name)
                      return [formatMoney(Number(value)), method ? t(`paymentMethod.${method.method}`) : String(name)]
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <ul className="space-y-3">
              {data.map((item, index) => (
                <li key={item.method} className="flex items-start justify-between gap-3 text-sm">
                  <span className="flex min-w-0 items-center gap-2">
                    <span aria-hidden="true" className="size-2.5 shrink-0 rounded-full" style={{ backgroundColor: paymentMethodColors[index % paymentMethodColors.length] }} />
                    <span className="truncate">{t(`paymentMethod.${item.method}`)}</span>
                  </span>
                  <span className="shrink-0 text-right">
                    <span className="block font-medium">{item.percent}%</span>
                    <span className="text-xs text-muted-foreground">{formatMoney(item.totalAmount)}</span>
                  </span>
                </li>
              ))}
              <li className="border-t border-border pt-2 text-sm font-semibold">{t('adminFinance.total')}: <MoneyText value={totalAmount} /></li>
            </ul>
          </div>
        </>
      )}
    </ChartCard>
  )
}

function OccupancyChart({ data, loading, error, retry }: {
  data: BuildingOccupancyDto[] | undefined
  loading: boolean
  error: boolean
  retry: () => void
}) {
  const chartData = useMemo(() => data?.map((building) => ({
    ...building,
    freeBeds: Math.max(0, building.totalBeds - building.occupiedBeds),
  })) ?? [], [data])
  return (
    <ChartCard title={t('adminFinance.occupancyByBuilding')}>
      {loading ? <BlockLoading rows={5} /> : error ? <ErrorState onRetry={retry} /> : !chartData.length ? (
        <EmptyState badge={false} title={t('adminFinance.chartEmpty')} description={t('adminFinance.chartEmptyDescription')} />
      ) : (
        <>
          <p id="building-occupancy-description" className="sr-only">{t('adminFinance.occupancyDescription')}: {chartData.map((item) => `${item.buildingName}: ${item.occupancyPercent}%, ${item.occupiedBeds} ${t('adminFinance.of')} ${item.totalBeds} ${t('adminFinance.places')}`).join('; ')}.</p>
          <div role="img" aria-label={t('adminFinance.occupancyByBuilding')} aria-describedby="building-occupancy-description" className="h-72 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} layout="vertical" margin={{ top: 4, right: 20, left: 8, bottom: 4 }}>
                <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" horizontal={false} />
                <XAxis type="number" hide />
                <YAxis type="category" dataKey="buildingName" width={92} tick={{ fill: 'var(--muted-foreground)', fontSize: 12 }} axisLine={false} tickLine={false} />
                <Tooltip
                  contentStyle={chartTooltipStyle}
                  formatter={(value, name, item) => {
                    const raw = item.payload as BuildingOccupancyDto & { freeBeds: number }
                    return [
                      `${Number(value)} ${t('adminFinance.places')} · ${raw.occupancyPercent}%`,
                      name === 'occupiedBeds' ? t('adminFinance.occupied') : t('adminFinance.available'),
                    ]
                  }}
                />
                <Bar dataKey="occupiedBeds" name="occupiedBeds" stackId="beds" fill="#4f46e5" radius={[0, 4, 4, 0]} />
                <Bar dataKey="freeBeds" name="freeBeds" stackId="beds" fill="#c7d2fe" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </>
      )}
    </ChartCard>
  )
}

function TopDebtorsBlock({ data, loading, error, retry, showMore }: {
  data: TopDebtorDto[] | undefined
  loading: boolean
  error: boolean
  retry: () => void
  showMore: boolean
}) {
  return (
    <Card className="min-w-0 p-5">
      <div className="mb-4 flex items-center justify-between gap-3"><h2 className="font-semibold">{t('adminFinance.topDebtors')}</h2>{showMore && <Link className="text-sm font-medium text-primary hover:underline" to="/admin/debtors">{t('adminFinance.allDebtors')}</Link>}</div>
      {loading ? <BlockLoading /> : error ? <ErrorState onRetry={retry} /> : !data?.length ? (
        <EmptyState badge={false} title={t('adminFinance.noDebtors')} description={t('adminFinance.noDebtorsDescription')} />
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-muted-foreground"><tr><th className="pb-2 font-medium">{t('user.fullName')}</th><th className="pb-2 font-medium">{t('adminFinance.room')}</th><th className="pb-2 text-right font-medium">{t('adminFinance.debt')}</th></tr></thead>
            <tbody className="divide-y divide-border">{data.map((item) => (
              <tr key={item.residentId}><td className="max-w-44 truncate py-3 font-medium">{item.fullName}</td><td className="py-3 text-muted-foreground">{item.buildingName ?? '—'} · {item.roomNumber ?? '—'}</td><td className="py-3 text-right"><MoneyText value={item.debtAmount} /></td></tr>
            ))}</tbody>
          </table>
        </div>
      )}
    </Card>
  )
}

function RecentPaymentsBlock({ data, loading, error, retry, showMore }: {
  data: RecentPaymentDto[] | undefined
  loading: boolean
  error: boolean
  retry: () => void
  showMore: boolean
}) {
  return (
    <Card className="min-w-0 p-5">
      <div className="mb-4 flex items-center justify-between gap-3"><h2 className="font-semibold">{t('adminFinance.recentPayments')}</h2>{showMore && <Link className="text-sm font-medium text-primary hover:underline" to="/admin/payments">{t('adminFinance.allPayments')}</Link>}</div>
      {loading ? <BlockLoading /> : error ? <ErrorState onRetry={retry} /> : !data?.length ? (
        <EmptyState badge={false} title={t('adminFinance.noPayments')} description={t('adminFinance.noPaymentsDescription')} />
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-muted-foreground"><tr><th className="pb-2 font-medium">{t('user.fullName')}</th><th className="pb-2 font-medium">{t('adminFinance.paidAt')}</th><th className="pb-2 font-medium">{t('adminFinance.method')}</th><th className="pb-2 text-right font-medium">{t('adminFinance.amount')}</th></tr></thead>
            <tbody className="divide-y divide-border">{data.map((payment) => (
              <tr key={payment.id}><td className="max-w-36 truncate py-3 font-medium">{payment.residentName}<span className="block truncate text-xs font-normal text-muted-foreground">{payment.receiptNumber}</span></td><td className="whitespace-nowrap py-3 text-muted-foreground">{formatDateTime(payment.paidAt)}</td><td className="py-3">{t(`paymentMethod.${payment.method}`)}</td><td className="whitespace-nowrap py-3 text-right"><MoneyText value={payment.amount} /></td></tr>
            ))}</tbody>
          </table>
        </div>
      )}
    </Card>
  )
}

export function AdminDashboardPage() {
  const { can } = useAuth()
  const [months, setMonths] = useState(6)
  const [methodPeriod, setMethodPeriod] = useState<'month' | 'year'>('month')
  const [from, to] = useMemo(() => utcPeriod(methodPeriod), [methodPeriod])
  const summary = useDashboardSummary()
  const revenue = useRevenueByMonth(months)
  const methods = useDashboardPaymentMethods(from, to)
  const occupancy = useBuildingOccupancy()
  const debtors = useTopDebtors(10)
  const payments = useRecentPayments(10)
  const refreshing = [summary, revenue, methods, occupancy, debtors, payments].some((query) => query.isFetching)
  const refresh = () => {
    void Promise.all([
      summary.refetch(), revenue.refetch(), methods.refetch(),
      occupancy.refetch(), debtors.refetch(), payments.refetch(),
    ])
  }

  const monthGrowth = summary.data && summary.data.incomeLastMonth !== 0
    ? ((summary.data.incomeThisMonth - summary.data.incomeLastMonth) / Math.abs(summary.data.incomeLastMonth)) * 100
    : undefined
  const trend = monthGrowth === undefined ? t('adminFinance.previousMonthNoData')
    : `${monthGrowth >= 0 ? '↑' : '↓'} ${Math.abs(monthGrowth).toFixed(1)}% ${t('adminFinance.comparedToLastMonth')}`

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 p-4 sm:p-7">
      <PageHeader
        title={t('pages.dashboard.title')}
        description={t('pages.dashboard.description')}
        actions={<Button variant="outline" onClick={refresh} disabled={refreshing} aria-label={t('adminFinance.refresh')}>
          <RefreshCw size={17} className={refreshing ? 'animate-spin' : ''} />{t('adminFinance.refresh')}
        </Button>}
      />

      {summary.isPending ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{Array.from({ length: 6 }, (_, index) => <Skeleton key={index} className="h-36 rounded-2xl" />)}</div>
      ) : summary.isError ? (
        <ErrorState onRetry={() => void summary.refetch()} />
      ) : summary.data ? (
        <section aria-label={t('adminFinance.summary')} className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          <StatCard icon={Users} label={t('adminFinance.activeResidents')} numericValue={summary.data.activeResidents} value="" color="indigo" />
          <StatCard
            icon={BedDouble}
            label={t('adminFinance.occupancy')}
            numericValue={summary.data.occupancyPercent}
            formatValue={(value) => `${value.toFixed(1)}%`}
            value=""
            trend={`${t('adminFinance.occupied')} ${summary.data.occupiedBeds} ${t('adminFinance.of')} ${summary.data.totalBeds} ${t('adminFinance.places')}`}
            color="emerald"
          />
          <StatCard
            icon={Activity}
            label={t('adminFinance.monthlyIncome')}
            numericValue={summary.data.incomeThisMonth}
            formatValue={(value) => formatMoney(value)}
            value=""
            trend={monthGrowth === undefined ? trend : `${monthGrowth >= 0 ? '↑' : '↓'} ${Math.abs(monthGrowth).toFixed(1)}% ${t('adminFinance.comparedToLastMonth')}`}
            color={monthGrowth !== undefined && monthGrowth < 0 ? 'amber' : 'emerald'}
          />
          <StatCard icon={CreditCard} label={t('adminFinance.chargedThisMonth')} numericValue={summary.data.chargedThisMonth} formatValue={(value) => formatMoney(value)} value="" color="indigo" />
          <StatCard icon={Banknote} label={t('adminFinance.totalDebt')} numericValue={summary.data.totalDebt} formatValue={(value) => formatMoney(value)} value="" color="red" />
          <StatCard icon={Wallet} label={t('adminFinance.debtors')} numericValue={summary.data.debtorsCount} value="" color="amber" />
        </section>
      ) : null}

      <section className="grid min-w-0 gap-5 xl:grid-cols-[minmax(0,1.45fr)_minmax(340px,1fr)]">
        <div className="min-w-0 space-y-4">
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-lg font-semibold">{t('adminFinance.reports')}</h2>
            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <span className="sr-only">{t('adminFinance.chartPeriod')}</span>
              <Select className="h-9 w-auto min-w-28" value={months} onChange={(event) => setMonths(Number(event.target.value))}>
                <option value={6}>{t('adminFinance.sixMonths')}</option>
                <option value={12}>{t('adminFinance.twelveMonths')}</option>
              </Select>
            </label>
          </div>
          <RevenueChart data={revenue.data} loading={revenue.isPending} error={revenue.isError} retry={() => void revenue.refetch()} />
        </div>
        <div className="min-w-0 space-y-4">
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-lg font-semibold">{t('adminFinance.breakdown')}</h2>
            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <span className="sr-only">{t('adminFinance.chartPeriod')}</span>
              <Select className="h-9 w-auto min-w-32" value={methodPeriod} onChange={(event) => setMethodPeriod(event.target.value as 'month' | 'year')}>
                <option value="month">{t('adminFinance.thisMonth')}</option>
                <option value="year">{t('adminFinance.thisYear')}</option>
              </Select>
            </label>
          </div>
          <MethodsChart data={methods.data} loading={methods.isPending} error={methods.isError} retry={() => void methods.refetch()} />
        </div>
      </section>

      <OccupancyChart data={occupancy.data} loading={occupancy.isPending} error={occupancy.isError} retry={() => void occupancy.refetch()} />
      <section className="grid min-w-0 gap-5 xl:grid-cols-2">
        <TopDebtorsBlock data={debtors.data} loading={debtors.isPending} error={debtors.isError} retry={() => void debtors.refetch()} showMore={can('paymentsView')} />
        <RecentPaymentsBlock data={payments.data} loading={payments.isPending} error={payments.isError} retry={() => void payments.refetch()} showMore={can('paymentsView')} />
      </section>
    </div>
  )
}
