import { useEffect } from 'react'
import { animate, motion, useMotionValue, useReducedMotion, useTransform } from 'framer-motion'
import type { LucideIcon } from 'lucide-react'
import { Card } from '@/components/ui/card'
import { cn } from '@/lib/utils'

export function StatCard({ icon: Icon, label, value, trend, color = 'indigo', numericValue, formatValue }: {
  icon: LucideIcon
  label: string
  value: string
  trend?: string
  color?: 'indigo' | 'emerald' | 'amber' | 'red'
  numericValue?: number
  formatValue?: (value: number) => string
}) {
  const reduceMotion = useReducedMotion()
  const animatedValue = useMotionValue(0)
  const formattedValue = useTransform(animatedValue, (value) => (
    formatValue?.(value) ?? String(Math.round(value))
  ))

  useEffect(() => {
    if (numericValue === undefined) return
    if (reduceMotion) {
      animatedValue.set(numericValue)
      return
    }

    const animation = animate(animatedValue, numericValue, { duration: 0.8, ease: 'easeOut' })
    return () => animation.stop()
  }, [animatedValue, numericValue, reduceMotion])

  return (
    <Card className="p-5">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm text-muted-foreground">{label}</p>
          <p className={cn('mt-2 text-2xl font-semibold tracking-tight', color === 'red' && 'text-red-600 dark:text-red-400')}>
            {numericValue === undefined ? value : <motion.span>{formattedValue}</motion.span>}
          </p>
        </div>
        <span className={cn('grid size-11 place-items-center rounded-xl', color === 'indigo' && 'bg-indigo-50 text-indigo-700 dark:bg-indigo-950 dark:text-indigo-300', color === 'emerald' && 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300', color === 'amber' && 'bg-amber-50 text-amber-800 dark:bg-amber-950 dark:text-amber-300', color === 'red' && 'bg-red-50 text-red-700 dark:bg-red-950 dark:text-red-300')}><Icon aria-hidden="true" size={20} /></span>
      </div>
      {trend && <p className="mt-4 text-xs text-muted-foreground">{trend}</p>}
    </Card>
  )
}
