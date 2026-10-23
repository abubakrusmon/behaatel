import type { PropsWithChildren } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import type { Permission } from '@/types/api'

export function RequirePermission({ permission, children }: PropsWithChildren<{ permission: Permission }>) {
  const { can } = useAuth()
  const location = useLocation()
  if (!can(permission)) {
    return <Navigate to="/403" replace state={{ from: location.pathname }} />
  }
  return children ?? <Outlet />
}
