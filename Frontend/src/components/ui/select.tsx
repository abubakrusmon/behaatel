import type { SelectHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Select({ className, ...props }: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select className={cn('h-10 w-full rounded-xl border border-input bg-background px-3 text-sm text-foreground focus-visible:border-ring', className)} {...props} />
}
