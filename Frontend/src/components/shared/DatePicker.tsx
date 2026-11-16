import { addDays, addMonths, format, isSameDay, isSameMonth, isValid, parse, parseISO, startOfMonth, startOfWeek, subMonths } from 'date-fns'
import { ru } from 'date-fns/locale'
import { CalendarDays, ChevronLeft, ChevronRight } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { Button } from '@/components/ui/button'
import { t } from '@/i18n/ru'

const weekdayStart = startOfWeek(new Date(2024, 0, 7), { locale: ru })
const weekdays = Array.from({ length: 7 }, (_, index) => format(addDays(weekdayStart, index), 'EEEEE', { locale: ru }))
const YEARS_PER_PAGE = 12
const TEXT_FORMAT = 'dd.MM.yyyy'
const REFERENCE_DATE = new Date(2000, 0, 1)

/** Оставляет только цифры и расставляет точки: 09092009 -> 09.09.2009 */
function maskDate(raw: string) {
  const digits = raw.replace(/\D/g, '').slice(0, 8)
  return [digits.slice(0, 2), digits.slice(2, 4), digits.slice(4, 8)].filter(Boolean).join('.')
}

function toText(value: string) {
  if (!value) return ''
  const date = parseISO(value)
  return isValid(date) ? format(date, TEXT_FORMAT) : ''
}

export function DatePicker({
  value,
  onChange,
  placeholder,
  min,
  max,
  ariaLabel,
  id,
}: {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  min?: string
  max?: string
  ariaLabel: string
  id?: string
}) {
  const selected = value ? parseISO(value) : undefined
  const minimum = min ? parseISO(min) : undefined
  const maximum = max ? parseISO(max) : undefined
  const [text, setText] = useState(() => toText(value))
  const [open, setOpen] = useState(false)
  const [yearView, setYearView] = useState(false)
  const [yearPageStart, setYearPageStart] = useState(0)
  const [visibleMonth, setVisibleMonth] = useState(() => selected ?? maximum ?? new Date())
  const rootRef = useRef<HTMLDivElement>(null)
  const monthStart = startOfMonth(visibleMonth)
  const days = useMemo(() => {
    const first = startOfWeek(monthStart, { locale: ru })
    return Array.from({ length: 42 }, (_, index) => addDays(first, index))
  }, [monthStart])
  const monthLabel = format(visibleMonth, 'LLLL', { locale: ru })
  const monthTitle = monthLabel.charAt(0).toLocaleUpperCase('ru-RU') + monthLabel.slice(1)
  const outOfRange = (date: Date) => Boolean((minimum && date < minimum) || (maximum && date > maximum))

  // Внешнее изменение значения (сброс формы, выбор в календаре) обновляет текст.
  useEffect(() => {
    if (value) setText((current) => (toText(value) === current ? current : toText(value)))
  }, [value])

  useEffect(() => {
    if (!open) return
    const close = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false)
    }
    const escape = (event: KeyboardEvent) => { if (event.key === 'Escape') setOpen(false) }
    document.addEventListener('mousedown', close)
    document.addEventListener('keydown', escape)
    return () => {
      document.removeEventListener('mousedown', close)
      document.removeEventListener('keydown', escape)
    }
  }, [open])

  const parsedText = text.length === 10 ? parse(text, TEXT_FORMAT, REFERENCE_DATE) : undefined
  const invalid = text.length === 10 && (!parsedText || !isValid(parsedText) || format(parsedText, TEXT_FORMAT) !== text || outOfRange(parsedText))

  const handleType = (raw: string) => {
    const masked = maskDate(raw)
    setText(masked)
    if (masked.length < 10) {
      if (value) onChange('')
      return
    }
    const date = parse(masked, TEXT_FORMAT, REFERENCE_DATE)
    if (isValid(date) && format(date, TEXT_FORMAT) === masked && !outOfRange(date)) {
      onChange(format(date, 'yyyy-MM-dd'))
      setVisibleMonth(date)
    } else if (value) {
      onChange('')
    }
  }

  const chooseDate = (date: Date) => {
    onChange(format(date, 'yyyy-MM-dd'))
    setText(format(date, TEXT_FORMAT))
    setOpen(false)
  }

  const openYears = () => {
    setYearPageStart(visibleMonth.getFullYear() - (visibleMonth.getFullYear() % YEARS_PER_PAGE))
    setYearView(true)
  }

  return (
    <div ref={rootRef} className="relative w-full">
      <div className={`flex h-11 w-full items-center gap-1 rounded-control border bg-background pl-3 pr-1 text-sm shadow-sm transition-colors focus-within:ring-2 focus-within:ring-ring/30 md:h-10 ${invalid ? 'border-destructive' : 'border-input'}`}>
        <input
          id={id}
          type="text"
          inputMode="numeric"
          autoComplete="off"
          aria-label={ariaLabel}
          aria-invalid={invalid}
          maxLength={10}
          value={text}
          placeholder={placeholder ?? 'ДД.ММ.ГГГГ'}
          onChange={(event) => handleType(event.target.value)}
          className="w-0 min-w-0 flex-1 bg-transparent outline-none placeholder:text-muted-foreground"
        />
        <Button
          type="button"
          size="icon"
          variant="ghost"
          aria-label={t('common.selectDate')}
          aria-expanded={open}
          onClick={() => { setYearView(false); setOpen((current) => !current) }}
          className="size-8 shrink-0"
        >
          <CalendarDays aria-hidden="true" className="size-4 text-muted-foreground" />
        </Button>
      </div>
      {invalid && <span className="mt-1 block text-xs font-normal text-destructive" role="alert">Введите корректную дату: ДД.ММ.ГГГГ</span>}
      {open && (
        <div className="absolute left-0 top-full z-50 mt-2 w-[min(20rem,calc(100vw-2rem))] rounded-card border border-border bg-popover p-3 text-popover-foreground shadow-soft">
          {yearView ? (
            <>
              <div className="mb-3 flex items-center justify-between">
                <Button type="button" size="icon" variant="ghost" aria-label={t('common.previousMonth')} onClick={() => setYearPageStart((year) => year - YEARS_PER_PAGE)}><ChevronLeft size={17} /></Button>
                <p className="text-sm font-semibold">{yearPageStart} – {yearPageStart + YEARS_PER_PAGE - 1}</p>
                <Button type="button" size="icon" variant="ghost" aria-label={t('common.nextMonth')} onClick={() => setYearPageStart((year) => year + YEARS_PER_PAGE)}><ChevronRight size={17} /></Button>
              </div>
              <div className="grid grid-cols-3 gap-2">
                {Array.from({ length: YEARS_PER_PAGE }, (_, index) => yearPageStart + index).map((year) => {
                  const disabled = Boolean((minimum && year < minimum.getFullYear()) || (maximum && year > maximum.getFullYear()))
                  return (
                    <button
                      key={year}
                      type="button"
                      disabled={disabled}
                      className={`rounded-lg py-2 text-sm transition-colors hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-30 ${year === visibleMonth.getFullYear() ? 'bg-primary font-semibold text-primary-foreground hover:bg-primary hover:text-primary-foreground' : ''}`}
                      onClick={() => { setVisibleMonth(new Date(year, visibleMonth.getMonth(), 1)); setYearView(false) }}
                    >
                      {year}
                    </button>
                  )
                })}
              </div>
            </>
          ) : (
            <>
              <div className="mb-3 flex items-center justify-between">
                <Button type="button" size="icon" variant="ghost" aria-label={t('common.previousMonth')} onClick={() => setVisibleMonth((month) => subMonths(month, 1))}><ChevronLeft size={17} /></Button>
                <button type="button" onClick={openYears} className="rounded-lg px-3 py-1.5 text-sm font-semibold hover:bg-accent" aria-label="Выбрать год">
                  {monthTitle} {visibleMonth.getFullYear()}
                </button>
                <Button type="button" size="icon" variant="ghost" aria-label={t('common.nextMonth')} onClick={() => setVisibleMonth((month) => addMonths(month, 1))}><ChevronRight size={17} /></Button>
              </div>
              <div className="grid grid-cols-7 gap-1 text-center text-xs text-muted-foreground">
                {weekdays.map((day, index) => <span key={`${day}-${index}`} className="py-1 font-medium">{day}</span>)}
                {days.map((day) => (
                  <button
                    key={day.toISOString()}
                    type="button"
                    disabled={outOfRange(day)}
                    aria-pressed={selected ? isSameDay(day, selected) : false}
                    className={`grid aspect-square place-items-center rounded-lg text-sm transition-colors hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-30 ${selected && isSameDay(day, selected) ? 'bg-primary font-semibold text-primary-foreground hover:bg-primary hover:text-primary-foreground' : ''} ${!isSameMonth(day, visibleMonth) ? 'text-muted-foreground/60' : ''}`}
                    onClick={() => chooseDate(day)}
                  >
                    {format(day, 'd')}
                  </button>
                ))}
              </div>
            </>
          )}
        </div>
      )}
    </div>
  )
}

export function DateRangeFields({
  from,
  to,
  onFromChange,
  onToChange,
  fromLabel,
  toLabel,
}: {
  from: string
  to: string
  onFromChange: (value: string) => void
  onToChange: (value: string) => void
  fromLabel: string
  toLabel: string
}) {
  return (
    <div className="contents">
      <label className="grid min-w-0 gap-1.5 text-sm font-medium">{fromLabel}<DatePicker value={from} onChange={onFromChange} ariaLabel={fromLabel} max={to || undefined} /></label>
      <label className="grid min-w-0 gap-1.5 text-sm font-medium">{toLabel}<DatePicker value={to} onChange={onToChange} ariaLabel={toLabel} min={from || undefined} /></label>
    </div>
  )
}
