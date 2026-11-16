import { Link, NavLink, Outlet } from 'react-router-dom'
import { Bell, CreditCard, House, LayoutDashboard, LogOut, Megaphone } from 'lucide-react'
import { useLogout } from '@/api/hooks'
import { Logo } from '@/components/shared/Logo'
import { ThemeToggle } from '@/components/shared/ThemeToggle'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { useAuthStore } from '@/store/authStore'
import { Button } from '@/components/ui/button'

const tabs = [
  { to: '/app', label: 'nav.home', icon: LayoutDashboard, end: true },
  { to: '/app/charges', label: 'nav.charges', icon: CreditCard },
  { to: '/app/payments', label: 'nav.payments', icon: CreditCard },
  { to: '/app/room', label: 'nav.myRoom', mobileLabel: 'nav.room', icon: House },
  { to: '/app/announcements', label: 'nav.announcements', icon: Megaphone },
]

const mobileTabs = tabs.map((tab) => ({ ...tab, label: 'mobileLabel' in tab ? tab.mobileLabel : tab.label }))

export function UserLayout() {
  const { user } = useAuth()
  const logout = useLogout()
  const refreshToken = useAuthStore((state) => state.refreshToken)
  return (
    <div className="min-h-screen bg-background pb-[calc(5rem+env(safe-area-inset-bottom))] md:pb-0">
      <header className="sticky top-0 z-20 border-b border-border bg-background/90 backdrop-blur">
        <div className="mx-auto flex h-14 sm:h-16 max-w-7xl items-center justify-between gap-3 px-3 sm:px-6">
          <Link to="/app" className="shrink-0"><Logo size={36} /></Link>
          <nav className="hidden min-w-0 items-center gap-0.5 whitespace-nowrap lg:flex" aria-label={t('nav.home')}>
            {tabs.map(({ to, label, icon: Icon, end }) => (
              <NavLink key={to} to={to} end={end} className={({ isActive }) => `inline-flex shrink-0 items-center gap-2 whitespace-nowrap rounded-xl px-2.5 py-2 text-sm font-medium transition-colors xl:px-3 ${isActive ? 'bg-accent text-primary' : 'text-muted-foreground hover:bg-muted hover:text-foreground'}`}>
                <Icon size={17} /><span>{t(label as Parameters<typeof t>[0])}</span>
              </NavLink>
            ))}
          </nav>
          <div className="flex shrink-0 items-center gap-1 sm:gap-2">
            <Link
              to="/app/announcements"
              className="relative grid size-10 place-items-center rounded-control text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
              aria-label={t('nav.announcements')}
              title={t('nav.announcements')}
            >
              <Bell size={19} aria-hidden="true" />
            </Link>
            <div className="hidden lg:block"><ThemeToggle compact /></div>
            <NavLink
              to="/app/profile"
              aria-label={t('nav.profile')}
              title={user?.fullName ?? t('nav.profile')}
              className={({ isActive }) => `grid size-10 place-items-center rounded-full transition-shadow focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring ${isActive ? 'ring-2 ring-primary' : 'hover:ring-2 hover:ring-border'}`}
            >
              <UserAvatar src={user?.avatarUrl} className="size-9" />
            </NavLink>
            <Button
              variant="ghost"
              size="icon"
              aria-label={t('common.logout')}
              title={t('common.logout')}
              onClick={() => logout.mutate({ refreshToken: refreshToken ?? '' })}
              disabled={logout.isPending}
            >
              <LogOut size={18} />
            </Button>
          </div>
        </div>
      </header>
      <main><Outlet /></main>
      <nav className="fixed inset-x-0 bottom-0 z-30 grid grid-cols-5 border-t border-border bg-card/95 px-2 pb-[max(env(safe-area-inset-bottom),0.5rem)] pt-2 backdrop-blur md:hidden" aria-label={t('nav.home')}>
        {mobileTabs.map(({ to, label, icon: Icon, end }) => (
          <NavLink key={to} to={to} end={end} className={({ isActive }) => `flex min-w-0 flex-col items-center gap-1 rounded-xl px-1 py-1.5 text-[11px] leading-tight transition-colors ${isActive ? 'bg-accent text-primary' : 'text-muted-foreground hover:bg-muted'}`}>
            <Icon size={19} aria-hidden="true" /><span className="max-w-full truncate text-[11px]">{t(label as Parameters<typeof t>[0])}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
