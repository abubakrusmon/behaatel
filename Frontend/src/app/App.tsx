import { useEffect, type PropsWithChildren } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { refresh, getMe } from '@/api/auth'
import { queryClient } from '@/api/queryClient'
import { Providers } from '@/app/providers'
import { RequireAuth } from '@/app/guards/RequireAuth'
import { RequirePermission } from '@/app/guards/RequirePermission'
import { RequireRole } from '@/app/guards/RequireRole'
import { ForbiddenPage, NotFoundPage } from '@/app/pages/AccessPages'
import { PlaceholderPage } from '@/app/pages/PlaceholderPage'
import { AdminLayout } from '@/app/layouts/AdminLayout'
import { UserLayout } from '@/app/layouts/UserLayout'
import { FullScreenLoader } from '@/components/shared/Spinner'
import { ChangePasswordPage } from '@/features/auth/ChangePasswordPage'
import { LoginPage } from '@/features/auth/LoginPage'
import { ForgotPasswordPage, RegisterPage, ResetPasswordPage, VerifyEmailPage } from '@/features/auth/EmailAuthPages'
import { HomePage } from '@/features/user/pages/HomePage'
import { ChargesPage } from '@/features/user/pages/ChargesPage'
import { PaymentsPage } from '@/features/user/pages/PaymentsPage'
import { RoomPage } from '@/features/user/pages/RoomPage'
import { AnnouncementsPage } from '@/features/user/pages/AnnouncementsPage'
import { ProfilePage } from '@/features/user/pages/ProfilePage'
import { RegistrationRequestsPage } from '@/features/admin/RegistrationRequestsPage'
import { AuthSettingsPage } from '@/features/admin/AuthSettingsPage'
import { AdminDashboardPage } from '@/features/admin/AdminDashboardPage'
import { AdminDebtorsPage } from '@/features/admin/AdminDebtorsPage'
import { AdminPaymentsPage } from '@/features/admin/AdminPaymentsPage'
import { useAuthStore } from '@/store/authStore'

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
                    <Route path="residents" element={<PlaceholderPage />} />
                    <Route path="registrations" element={<RegistrationRequestsPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="roomsView" />}>
                    <Route path="rooms" element={<PlaceholderPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="chargesView" />}>
                    <Route path="charges" element={<PlaceholderPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="paymentsView" />}>
                    <Route path="payments" element={<AdminPaymentsPage />} />
                    <Route path="debtors" element={<AdminDebtorsPage />} />
                  </Route>
                  <Route element={<RequirePermission permission="announcementsManage" />}>
                    <Route path="announcements" element={<PlaceholderPage />} />
                  </Route>
                  <Route element={<RequireRole roles={['superAdmin']} />}>
                    <Route path="admins" element={<PlaceholderPage />} />
                    <Route path="audit" element={<PlaceholderPage />} />
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
        </AuthBootstrap>
      </BrowserRouter>
    </Providers>
  )
}
