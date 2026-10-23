import { useAuthStore } from '@/store/authStore'
import type { Permission, Role } from '@/types/api'

export function useAuth() {
  const user = useAuthStore((state) => state.user)
  const status = useAuthStore((state) => state.status)
  return {
    user,
    status,
    can: (permission: Permission) => user?.role === 'superAdmin' || Boolean(user?.permissions.includes(permission)),
    hasRole: (...roles: Role[]) => Boolean(user && roles.includes(user.role)),
  }
}
