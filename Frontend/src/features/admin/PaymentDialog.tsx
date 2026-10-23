import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { useAdminResidents, useCreateAdminPayment } from '@/api/admin/hooks'
import { Dialog } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { MoneyText } from '@/components/shared/MoneyText'
import { toast } from '@/lib/toast'
import { getErrorMessage } from '@/api/errors'
import { t } from '@/i18n/ru'
import type { PaymentMethod } from '@/types/api'
import type { ProblemDetails } from '@/types/api'
import axios from 'axios'

const paymentMethods = ['cash', 'card', 'bankTransfer', 'online'] as const satisfies readonly PaymentMethod[]
const paymentSchema = z.object({
  residentId: z.string().min(1, t('adminFinance.residentRequired')),
  amount: z.string()
    .min(1, t('adminFinance.amountRequired'))
    .refine((value) => Number.isFinite(Number(value)) && Number(value) > 0, t('adminFinance.amountPositive')),
  method: z.enum(paymentMethods),
  paidAt: z.string().min(1, t('adminFinance.dateRequired'))
    .refine((value) => !Number.isNaN(Date.parse(value)), t('adminFinance.dateInvalid')),
  comment: z.string().max(1000, t('adminFinance.commentTooLong')),
})

type PaymentForm = z.infer<typeof paymentSchema>
export interface PaymentResidentOption {
  id: string
  fullName: string
  balance: number
}

function localDateTimeValue(): string {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 16)
}

function residentDebt(balance: number): number {
  return Math.max(0, -balance)
}

export function PaymentDialog({
  open,
  onOpenChange,
  initialResident,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  initialResident?: PaymentResidentOption
}) {
  const [residentSearch, setResidentSearch] = useState('')
  const [residentListOpen, setResidentListOpen] = useState(false)
  const residents = useAdminResidents(residentSearch.trim(), open)
  const createPayment = useCreateAdminPayment()
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<PaymentForm>({
    resolver: zodResolver(paymentSchema),
    defaultValues: {
      residentId: '',
      amount: '',
      method: 'cash',
      paidAt: localDateTimeValue(),
      comment: '',
    },
  })
  const selectedResidentId = watch('residentId')
  const selectedResident = residents.data?.items.find((resident) => resident.id === selectedResidentId)
  const resident = selectedResident
    ? { id: selectedResident.id, fullName: selectedResident.fullName, balance: selectedResident.balance }
    : initialResident?.id === selectedResidentId ? initialResident : undefined
  const debt = resident ? residentDebt(resident.balance) : 0

  useEffect(() => {
    if (!open) return
    reset({
      residentId: initialResident?.id ?? '',
      amount: '',
      method: 'cash',
      paidAt: localDateTimeValue(),
      comment: '',
    })
    setResidentSearch(initialResident?.fullName ?? '')
    setResidentListOpen(false)
  }, [initialResident?.balance, initialResident?.fullName, initialResident?.id, open, reset])

  const setServerErrors = (error: unknown) => {
    if (axios.isAxiosError<ProblemDetails>(error)) {
      for (const [key, messages] of Object.entries(error.response?.data?.errors ?? {})) {
        const fieldName = key.split('.').at(-1)?.toLowerCase()
        const field = fieldName === 'residentid' ? 'residentId'
          : fieldName === 'amount' ? 'amount'
            : fieldName === 'method' ? 'method'
              : fieldName === 'paidat' ? 'paidAt'
                : fieldName === 'comment' ? 'comment' : undefined
        if (field && messages[0]) {
          setError(field, { type: 'server', message: messages[0] })
        }
      }
    }
    toast.error(getErrorMessage(error))
  }

  const submit = handleSubmit(async (values) => {
    try {
      const payment = await createPayment.mutateAsync({
        residentId: values.residentId,
        amount: Number(values.amount),
        method: values.method,
        paidAt: new Date(values.paidAt).toISOString(),
        comment: values.comment.trim() || null,
      })
      toast.success(`${t('adminFinance.paymentCreated')} ${payment.receiptNumber}`)
      onOpenChange(false)
    } catch (error) {
      setServerErrors(error)
    }
  })

  const handleResidentSearch = (value: string) => {
    setResidentSearch(value)
    setResidentListOpen(true)
    setValue('residentId', '', { shouldValidate: true })
  }

  const formError = (message?: string) => message
    ? <span className="text-xs text-destructive" role="alert">{message}</span>
    : null

  return (
    <Dialog open={open} onClose={() => onOpenChange(false)} title={t('adminFinance.createPayment')}>
      <form className="max-h-[75vh] space-y-4 overflow-y-auto pr-1" onSubmit={(event) => { void submit(event) }} noValidate>
        <div className="space-y-1.5">
          <label className="text-sm font-medium" htmlFor="payment-resident">{t('adminFinance.resident')}</label>
          <div className="relative">
            <Input
              id="payment-resident"
              autoComplete="off"
              role="combobox"
              aria-autocomplete="list"
              aria-expanded={residentListOpen}
              aria-controls="payment-resident-list"
              aria-invalid={Boolean(errors.residentId)}
              placeholder={t('adminFinance.searchResident')}
              value={residentSearch}
              onFocus={() => setResidentListOpen(true)}
              onChange={(event) => handleResidentSearch(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Escape') setResidentListOpen(false)
                if (event.key === 'ArrowDown') {
                  event.preventDefault()
                  document.querySelector<HTMLButtonElement>('#payment-resident-list button')?.focus()
                }
              }}
            />
            {residentListOpen && (
              <div id="payment-resident-list" role="listbox" className="absolute z-20 mt-1 max-h-56 w-full overflow-y-auto rounded-xl border border-border bg-popover p-1 shadow-lg">
                {residents.isPending ? (
                  <p className="px-3 py-2 text-sm text-muted-foreground">{t('common.loading')}</p>
                ) : residents.isError ? (
                  <button type="button" className="w-full rounded-lg px-3 py-2 text-left text-sm text-destructive hover:bg-muted" onClick={() => void residents.refetch()}>
                    {t('adminFinance.residentsLoadError')}
                  </button>
                ) : residents.data.items.length ? residents.data.items.map((option) => (
                  <button
                    key={option.id}
                    type="button"
                    role="option"
                    aria-selected={option.id === selectedResidentId}
                    className="flex w-full items-center justify-between gap-3 rounded-lg px-3 py-2 text-left text-sm hover:bg-muted focus-visible:bg-muted"
                    onClick={() => {
                      setValue('residentId', option.id, { shouldValidate: true })
                      setResidentSearch(option.fullName)
                      setResidentListOpen(false)
                    }}
                  >
                    <span className="min-w-0">
                      <span className="block truncate font-medium">{option.fullName}</span>
                      <span className="block truncate text-xs text-muted-foreground">{option.buildingName ?? '—'} · {option.roomNumber ?? '—'}</span>
                    </span>
                    <MoneyText value={residentDebt(option.balance)} />
                  </button>
                )) : (
                  <p className="px-3 py-2 text-sm text-muted-foreground">{t('adminFinance.residentsNotFound')}</p>
                )}
              </div>
            )}
          </div>
          {formError(errors.residentId?.message)}
          {resident && <p className="flex items-center gap-1 text-xs text-muted-foreground">{t('adminFinance.debt')}: <MoneyText value={debt} /></p>}
        </div>

        <div className="space-y-1.5">
          <label className="text-sm font-medium" htmlFor="payment-amount">{t('adminFinance.amount')}</label>
          <div className="flex gap-2">
            <Input id="payment-amount" type="number" inputMode="decimal" min="0.01" step="0.01" aria-invalid={Boolean(errors.amount)} {...register('amount')} />
            <Button type="button" variant="outline" disabled={!resident || debt <= 0} onClick={() => setValue('amount', debt.toFixed(2), { shouldValidate: true })}>
              {t('adminFinance.fullDebt')}
            </Button>
          </div>
          {formError(errors.amount?.message)}
        </div>

        <div className="space-y-1.5">
          <label className="text-sm font-medium" htmlFor="payment-method">{t('adminFinance.method')}</label>
          <Select id="payment-method" aria-invalid={Boolean(errors.method)} {...register('method')}>
            {paymentMethods.map((method) => <option key={method} value={method}>{t(`paymentMethod.${method}`)}</option>)}
          </Select>
          {formError(errors.method?.message)}
        </div>

        <div className="space-y-1.5">
          <label className="text-sm font-medium" htmlFor="payment-date">{t('adminFinance.paidAt')}</label>
          <Input id="payment-date" type="datetime-local" aria-invalid={Boolean(errors.paidAt)} {...register('paidAt')} />
          {formError(errors.paidAt?.message)}
        </div>

        <div className="space-y-1.5">
          <label className="text-sm font-medium" htmlFor="payment-comment">{t('adminFinance.comment')}</label>
          <Textarea id="payment-comment" maxLength={1000} aria-invalid={Boolean(errors.comment)} {...register('comment')} />
          {formError(errors.comment?.message)}
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>{t('common.cancel')}</Button>
          <Button type="submit" disabled={isSubmitting || createPayment.isPending}>
            {createPayment.isPending ? t('adminFinance.creatingPayment') : t('adminFinance.createPayment')}
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
