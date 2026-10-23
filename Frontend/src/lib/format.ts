import { format, parse, parseISO, isValid } from 'date-fns'
import { ru } from 'date-fns/locale'

export function formatMoney(value: number, currency = 'TJS'): string {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency,
    maximumFractionDigits: 2,
  }).format(value)
}

export function formatDate(value: string | Date | null | undefined): string {
  if (!value) return '—'
  const date = value instanceof Date ? value : parseISO(value)
  return isValid(date) ? format(date, 'd MMM yyyy', { locale: ru }) : '—'
}

export function formatDateTime(value: string | Date | null | undefined): string {
  if (!value) return '—'
  const date = value instanceof Date ? value : parseISO(value)
  return isValid(date) ? format(date, 'd MMM yyyy, HH:mm', { locale: ru }) : '—'
}

export function formatPeriod(value: string): string {
  const date = parse(value, 'yyyy-MM', new Date())
  return isValid(date) ? format(date, 'LLLL yyyy', { locale: ru }) : value
}
