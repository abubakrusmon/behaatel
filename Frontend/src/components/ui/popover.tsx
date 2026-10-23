import type { DetailsHTMLAttributes, HTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Popover({ className, ...props }: DetailsHTMLAttributes<HTMLDetailsElement>) {
  return <details className={cn('group relative', className)} {...props} />
}

export function PopoverTrigger({ className, ...props }: HTMLAttributes<HTMLElement>) {
  return <summary className={cn('cursor-pointer list-none', className)} {...props} />
}

export function PopoverContent({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('absolute z-40 mt-2 min-w-56 rounded-xl border border-border bg-popover p-4 text-popover-foreground shadow-lg', className)} {...props} />
}
