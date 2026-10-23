import { Link, NavLink, Outlet } from 'react-router-dom'
import { Bell, CreditCard, House, LayoutDashboard, LogOut, UserRound } from 'lucide-react'
import { useLogout } from '@/api/hooks'
import { Logo } from '@/components/shared/Logo'
import { ThemeToggle } from '@/components/shared/ThemeToggle'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import { useAuthStore } from '@/store/authStore'
import { Button } from '@/components/ui/button'

const tabs = [
  { to: '/app', label: 'nav.home', icon: LayoutDashboard, end: true },
  { to: '/app/charges', label: 'nav.charges', icon: CreditCard },
  { to: '/app/payments', label: 'nav.payments', icon: CreditCard },
  { to: '/app/room', label: 'nav.myRoom', icon: House },
  { to: '/app/announcements', label: 'nav.announcements', icon: Bell },
  { to: '/app/profile', label: 'nav.profile', icon: UserRound },
]

export function UserLayout() {
  const { user } = useAuth()
  const logout = useLogout()
  const refreshToken = useAuthStore((state) => state.refreshToken)
  return (
    <div className="min-h-screen bg-background pb-20 md:pb-0">
      <header className="sticky top-0 z-20 border-b border-border bg-background/90 backdrop-blur">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6">
          <Link to="/app"><Logo /></Link>
          <nav className="hidden items-center gap-1 lg:flex" aria-label={t('nav.home')}>
            {tabs.map(({ to, label, icon: Icon, end }) => (
              <NavLink key={to} to={to} end={end} className={({ isActive }) => `inline-flex items-center gap-2 rounded-xl px-3 py-2 text-sm font-medium transition-colors ${isActive ? 'bg-accent text-primary' : 'text-muted-foreground hover:bg-muted hover:text-foreground'}`}>
                <Icon size={17} /><span>{t(label as Parameters<typeof t>[0])}</span>
              </NavLink>
            ))}
          </nav>
          <div className="flex items-center gap-2">
            <ThemeToggle />
            <Link to="/change-password" className="hidden items-center gap-2 rounded-xl px-2 py-1.5 text-sm hover:bg-muted sm:flex" title={t('common.changePassword')}>
              <span className="grid size-8 place-items-center rounded-full bg-primary text-xs font-semibold text-primary-foreground">{user?.fullName.slice(0, 1).toUpperCase()}</span>
              <span className="max-w-32 truncate font-medium">{user?.fullName}</span>
            </Link>
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
      <nav className="fixed inset-x-0 bottom-0 z-30 grid grid-cols-6 border-t border-border bg-card/95 px-1 pb-[env(safe-area-inset-bottom)] pt-2 backdrop-blur lg:hidden" aria-label={t('nav.home')}>
        {tabs.map(({ to, label, icon: Icon, end }) => (
          <NavLink key={to} to={to} end={end} className={({ isActive }) => `flex min-w-0 flex-col items-center gap-1 rounded-lg px-1 py-1.5 text-[10px] leading-tight ${isActive ? 'text-primary' : 'text-muted-foreground'}`}>
            <Icon size={19} /><span className="max-w-full truncate">{t(label as Parameters<typeof t>[0])}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
