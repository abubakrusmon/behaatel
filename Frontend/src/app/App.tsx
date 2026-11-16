import { lazy, Suspense, useEffect, type PropsWithChildren } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { refresh, getMe } from '@/api/auth'
import { queryClient } from '@/api/queryClient'
import { Providers } from '@/app/providers'
import { RequireAuth } from '@/app/guards/RequireAuth'
import { RequirePermission } from '@/app/guards/RequirePermission'
import { RequireRole } from '@/app/guards/RequireRole'
import { ForbiddenPage, NotFoundPage } from '@/app/pages/AccessPages'
import { AdminLayout } from '@/app/layouts/AdminLayout'
import { UserLayout } from '@/app/layouts/UserLayout'
import { FullScreenLoader } from '@/components/shared/Spinner'
import { ChangePasswordPage } from '@/features/auth/ChangePasswordPage'
import { LoginPage } from '@/features/auth/LoginPage'
import { ForgotPasswordPage, RegisterPage, ResetPasswordPage, VerifyEmailPage } from '@/features/auth/EmailAuthPages'
import { useAuthStore } from '@/store/authStore'

const HomePage = lazy(() => import('@/features/user/pages/HomePage').then((module) => ({ default: module.HomePage })))
const ChargesPage = lazy(() => import('@/features/user/pages/ChargesPage').then((module) => ({ default: module.ChargesPage })))
const PaymentsPage = lazy(() => import('@/features/user/pages/PaymentsPage').then((module) => ({ default: module.PaymentsPage })))
const RoomPage = lazy(() => import('@/features/user/pages/RoomPage').then((module) => ({ default: module.RoomPage })))
const AnnouncementsPage = lazy(() => import('@/features/user/pages/AnnouncementsPage').then((module) => ({ default: module.AnnouncementsPage })))
const ProfilePage = lazy(() => import('@/features/user/pages/ProfilePage').then((module) => ({ default: module.ProfilePage })))
const RegistrationRequestsPage = lazy(() => import('@/features/admin/RegistrationRequestsPage').then((module) => ({ default: module.RegistrationRequestsPage })))
const AuthSettingsPage = lazy(() => import('@/features/admin/AuthSettingsPage').then((module) => ({ default: module.AuthSettingsPage })))
const AdminDashboardPage = lazy(() => import('@/features/admin/AdminDashboardPage').then((module) => ({ default: module.AdminDashboardPage })))
const AdminDebtorsPage = lazy(() => import('@/features/admin/AdminDebtorsPage').then((module) => ({ default: module.AdminDebtorsPage })))
const AdminPaymentsPage = lazy(() => import('@/features/admin/AdminPaymentsPage').then((module) => ({ default: module.AdminPaymentsPage })))
const AdminResidentsPage = lazy(() => import('@/features/admin/AdminResidentsPage').then((module) => ({ default: module.AdminResidentsPage })))
const AdminRoomsPage = lazy(() => import('@/features/admin/AdminRoomsPage').then((module) => ({ default: module.AdminRoomsPage })))
const AdminAnnouncementsPage = lazy(() => import('@/features/admin/AdminAnnouncementsPage').then((module) => ({ default: module.AdminAnnouncementsPage })))
const AdminNotificationsPage = lazy(() => import('@/features/admin/AdminNotificationsPage').then((module) => ({ default: module.AdminNotificationsPage })))
const AdminChargesPage = lazy(() => import('@/features/admin/AdminChargesPage').then((module) => ({ default: module.AdminChargesPage })))
const AdminAdminsPage = lazy(() => import('@/features/admin/AdminAdminsPage').then((module) => ({ default: module.AdminAdminsPage })))
const AdminAuditPage = lazy(() => import('@/features/admin/AdminAuditPage').then((module) => ({ default: module.AdminAuditPage })))

function AuthBootstrap({ children }: PropsWithChildren) {
  const status = useAuthStore((state) => state.status)
  const initializeGuest = useAuthStore((state) => state.initializeGuest)
  const setSession = useAuthStore((state) => state.setSession)
  const setUser = useAuthStore((state) => state.setUser)
  const clearSession = useAuthStore((state) => state.clearSession)

  useEffect(() => {
    let active = true
    const refreshToken = localStorage.getItem('behaatel-refresh-token')
    if (!refreshToken) {
      initializeGuest()
      return () => { active = false }
    }
    void (async () => {
      try {
        const session = await refresh({ refreshToken })
        if (!active) return
        setSession(session)
        const user = await getMe()
        if (active) setUser(user)
      } catch {
        if (active) clearSession()
      }
    })()
    return () => { active = false }
  }, [clearSession, initializeGuest, setSession, setUser])

  useEffect(() => {
    const clearQueries = () => queryClient.clear()
    window.addEventListener('behaatel:session-expired', clearQueries)
    return () => window.removeEventListener('behaatel:session-expired', clearQueries)
  }, [])

  if (status === 'loading') return <FullScreenLoader />
  return children
}

function HomeRedirect() {
  const user = useAuthStore((state) => state.user)
  if (!user) return <Navigate to="/login" replace />
  if (user.mustChangePassword) return <Navigate to="/change-password" replace />
  return <Navigate to={user.role === 'user' ? '/app' : '/admin'} replace />
}

export function App() {
  return (
    <Providers>
      <BrowserRouter>
        <AuthBootstrap>
          <Suspense fallback={<FullScreenLoader />}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/verify-email" element={<VerifyEmailPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
            <Route path="/403" element={<ForbiddenPage />} />
            <Route path="*" element={<NotFoundPage />} />
            <Route element={<RequireAuth />}>
              <Route path="/change-password" element={<ChangePasswordPage />} />
              <Route path="/" element={<HomeRedirect />} />
              <Route element={<RequireRole roles={['admin', 'superAdmin']} />}>
                <Route path="/admin" element={<AdminLayout />}>
                  <Route element={<RequirePermission permission="reportsView" />}>
                    <Route index element={<AdminDashboardPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="residentsView" />}>
                    <Route path="residents" element={<AdminResidentsPage />} />
                    <Route path="registrations" element={<RegistrationRequestsPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="roomsView" />}>
                    <Route path="rooms" element={<AdminRoomsPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="chargesView" />}>
                    <Route path="charges" element={<AdminChargesPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="paymentsView" />}>
                    <Route path="payments" element={<AdminPaymentsPage />} />
                    <Route path="debtors" element={<AdminDebtorsPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="announcementsManage" />}>
                    <Route path="announcements" element={<AdminAnnouncementsPage />} />
                    <Route path="notifications" element={<AdminNotificationsPage />} />
                  </Route>
                  <Route element={<RequireRole roles={['superAdmin']} />}>
                    <Route path="admins" element={<AdminAdminsPage />} />
                    <Route path="audit" element={<AdminAuditPage />} />
                    <Route path="settings" element={<AuthSettingsPage />} />
                  </Route>
                </Route>
              </Route>
              <Route element={<RequireRole roles={['user']} />}>
                <Route path="/app" element={<UserLayout />}>
                  <Route index element={<HomePage />} />
                  <Route path="charges" element={<ChargesPage />} />
                  <Route path="payments" element={<PaymentsPage />} />
                  <Route path="room" element={<RoomPage />} />
                  <Route path="announcements" element={<AnnouncementsPage />} />
                  <Route path="profile" element={<ProfilePage />} />
                </Route>
              </Route>
            </Route>
          </Routes>
          </Suspense>
        </AuthBootstrap>
      </BrowserRouter>
    </Providers>
  )
}
