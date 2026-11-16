import { useState } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import {
  Activity, Banknote, Bell, Building2, ChevronLeft, ClipboardList, LayoutDashboard,
  Menu, Megaphone, Settings, Shield, Users, UserRoundPlus, WalletCards, X,
} from 'lucide-react'
import { useLogout } from '@/api/hooks'
import { Logo } from '@/components/shared/Logo'
import { ThemeToggle } from '@/components/shared/ThemeToggle'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/hooks/useAuth'
import { t } from '@/i18n/ru'
import type { Permission, Role } from '@/types/api'

interface AdminNavItem {
  path: string
  label: string
  icon: typeof LayoutDashboard
  permission?: Permission
  roles?: Role[]
}

const items: AdminNavItem[] = [
  { path: '/admin', label: 'nav.dashboard', icon: LayoutDashboard, permission: 'reportsView' },
  { path: '/admin/residents', label: 'nav.residents', icon: Users, permission: 'residentsView' },
  { path: '/admin/registrations', label: 'nav.registrations', icon: UserRoundPlus, permission: 'residentsView' },
  { path: '/admin/rooms', label: 'nav.buildingsRooms', icon: Building2, permission: 'roomsView' },
  { path: '/admin/charges', label: 'nav.charges', icon: ClipboardList, permission: 'chargesView' },
  { path: '/admin/payments', label: 'nav.payments', icon: WalletCards, permission: 'paymentsView' },
  { path: '/admin/debtors', label: 'nav.debtors', icon: Banknote, permission: 'paymentsView' },
  { path: '/admin/announcements', label: 'nav.announcements', icon: Megaphone, permission: 'announcementsManage' },
  { path: '/admin/notifications', label: 'nav.notifications', icon: Bell, permission: 'announcementsManage' },
  { path: '/admin/admins', label: 'nav.admins', icon: Shield, roles: ['superAdmin'] },
  { path: '/admin/audit', label: 'nav.audit', icon: Activity, roles: ['superAdmin'] },
  { path: '/admin/settings', label: 'nav.settings', icon: Settings, roles: ['superAdmin'] },
]

export function AdminLayout() {
  const { user, can, hasRole } = useAuth()
  const logout = useLogout()
  const [collapsed, setCollapsed] = useState(localStorage.getItem('behaatel-sidebar-collapsed') === 'true')
  const [mobileOpen, setMobileOpen] = useState(false)
  const location = useLocation()
  const title = items.find((item) => item.path === location.pathname)?.label ?? 'nav.dashboard'
  const visibleItems = items.filter((item) => (!item.permission || can(item.permission)) && (!item.roles || hasRole(...item.roles)))

  const toggleCollapsed = () => {
    const value = !collapsed
    setCollapsed(value)
    localStorage.setItem('behaatel-sidebar-collapsed', String(value))
  }

  const sidebar = (mobile = false) => (
    <div className="flex h-full flex-col">
      <div className={`flex h-20 items-center ${collapsed && !mobile ? 'justify-center px-0' : 'justify-between px-5'}`}>
        <Link to="/admin" aria-label={t('common.appName')} onClick={() => setMobileOpen(false)}><Logo size={collapsed && !mobile ? 36 : 40} showText={!collapsed || mobile} /></Link>
        {mobile && <Button variant="ghost" size="icon" aria-label={t('common.close')} onClick={() => setMobileOpen(false)}><X size={19} /></Button>}
      </div>
      <nav className="flex-1 space-y-1 overflow-y-auto px-3 pb-4" aria-label={t('nav.dashboard')}>
        {visibleItems.map(({ path, label, icon: Icon }) => (
          <NavLink
            key={path}
            to={path}
            end={path === '/admin'}
            onClick={() => setMobileOpen(false)}
            title={collapsed && !mobile ? t(label as Parameters<typeof t>[0]) : undefined}
            className={({ isActive }) => `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors ${isActive ? 'bg-accent text-primary' : 'text-muted-foreground hover:bg-muted hover:text-foreground'}`}
          >
            <Icon size={19} strokeWidth={1.9} /><span className={collapsed && !mobile ? 'sr-only' : ''}>{t(label as Parameters<typeof t>[0])}</span>
          </NavLink>
        ))}
      </nav>
      <div className="border-t border-border p-4">
        <div className={`flex items-center gap-3 ${collapsed && !mobile ? 'justify-center' : ''}`}>
          <UserAvatar src={user?.avatarUrl} className="size-10" />
          {(!collapsed || mobile) && <div className="min-w-0 flex-1"><p className="truncate text-sm font-medium">{user?.fullName}</p><p className="text-xs text-muted-foreground">{user && t(`common.role.${user.role}`)}</p></div>}
        </div>
      </div>
    </div>
  )

  return (
    <div className="min-h-screen bg-background">
      <aside className={`fixed inset-y-0 left-0 z-30 hidden border-r border-border bg-card transition-[width] duration-200 lg:block ${collapsed ? 'w-[76px]' : 'w-64'}`}>{sidebar()}</aside>
      {mobileOpen && <div className="fixed inset-0 z-40 lg:hidden"><button className="absolute inset-0 bg-slate-950/45" aria-label={t('common.close')} onClick={() => setMobileOpen(false)} /><aside className="absolute inset-y-0 left-0 w-[min(19rem,85vw)] bg-card shadow-xl">{sidebar(true)}</aside></div>}
      <div className={`min-h-screen transition-[padding] duration-200 ${collapsed ? 'lg:pl-[76px]' : 'lg:pl-64'}`}>
        <header className="sticky top-0 z-20 flex h-16 min-w-0 items-center justify-between gap-2 border-b border-border bg-background/90 px-3 backdrop-blur sm:px-7">
          <div className="flex min-w-0 flex-1 items-center gap-2 sm:gap-3">
            <Button className="lg:hidden" variant="ghost" size="icon" aria-label={t('common.showSidebar')} onClick={() => setMobileOpen(true)}><Menu size={20} /></Button>
            <Button className="hidden lg:inline-flex" variant="ghost" size="icon" aria-label={t(collapsed ? 'common.expandSidebar' : 'common.hideSidebar')} onClick={toggleCollapsed}><ChevronLeft className={collapsed ? 'rotate-180' : ''} size={19} /></Button>
            <h1 className="truncate text-sm font-semibold sm:text-base">{t(title as Parameters<typeof t>[0])}</h1>
          </div>
          <div className="flex shrink-0 items-center gap-1 sm:gap-2">
            <ThemeToggle />
            <details className="relative">
              <summary className="flex h-10 cursor-pointer list-none items-center gap-2 rounded-xl px-2 hover:bg-muted" aria-label={t('shared.accountMenu')}>
                <UserAvatar src={user?.avatarUrl} className="size-8" iconSize={16} />
                <span className="hidden max-w-32 truncate text-sm font-medium sm:inline">{user?.fullName}</span>
              </summary>
              <div className="absolute right-0 mt-2 w-56 rounded-xl border border-border bg-popover p-2 shadow-lg">
                <div className="px-3 py-2"><p className="truncate text-sm font-medium">{user?.fullName}</p><p className="text-xs text-muted-foreground">{user?.email}</p></div>
                <Link className="block rounded-lg px-3 py-2 text-sm hover:bg-muted" to="/change-password">{t('common.changePassword')}</Link>
                <button className="w-full rounded-lg px-3 py-2 text-left text-sm text-destructive hover:bg-muted" onClick={() => logout.mutate({ refreshToken: user ? localStorage.getItem('behaatel-refresh-token') ?? '' : '' })}>{t('common.logout')}</button>
              </div>
            </details>
          </div>
        </header>
        <main><Outlet /></main>
      </div>
    </div>
  )
}
