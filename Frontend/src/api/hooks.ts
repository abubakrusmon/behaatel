import { useMutation, useQuery } from '@tanstack/react-query'
import { changePassword, getMe, login, logout } from '@/api/auth'
import { queryClient } from '@/api/queryClient'
import { useAuthStore } from '@/store/authStore'

export function useLogin() {
  return useMutation({
    mutationFn: login,
    onSuccess: (result) => {
      if (result.session) {
        useAuthStore.getState().setSession(result.session)
        queryClient.setQueryData(['auth', 'me'], result.session.user)
      }
    },
  })
}

export function useCurrentUser(enabled = true) {
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: getMe,
    enabled,
  })
}

export function useLogout() {
  return useMutation({
    mutationFn: logout,
    onSettled: () => {
      useAuthStore.getState().clearSession()
      queryClient.clear()
      window.location.assign('/login')
    },
  })
}

export function useChangePassword() {
  return useMutation({
    mutationFn: changePassword,
  })
}
