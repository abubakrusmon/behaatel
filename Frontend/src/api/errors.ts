import axios, { type AxiosError } from 'axios'
import type { FieldValues, UseFormSetError } from 'react-hook-form'
import { t } from '@/i18n/ru'
import type { ProblemDetails } from '@/types/api'

export function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError<ProblemDetails>(error)) return t('errors.generic')
  const axiosError = error as AxiosError<ProblemDetails>
  const status = axiosError.response?.status
  const detail = axiosError.response?.data?.detail
  const validationMessage = Object.values(axiosError.response?.data?.errors ?? {}).flat()[0]
  if (status === 401) return detail || t('errors.unauthorized')
  if (status === 403) return detail || t('errors.forbidden')
  if (status === 404) return detail || t('errors.notFound')
  if (status === 429) return t('errors.rateLimited')
  if (!axiosError.response) return t('errors.network')
  return validationMessage || detail || axiosError.response.data?.title || t('errors.generic')
}

export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
): void {
  if (!axios.isAxiosError<ProblemDetails>(error)) return
  const errors = error.response?.data?.errors
  if (!errors) return
  for (const [field, messages] of Object.entries(errors)) {
    if (messages.length > 0) {
      setError(field as Parameters<UseFormSetError<T>>[0], { type: 'server', message: messages[0] })
    }
  }
}
