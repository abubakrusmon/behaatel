import type { DetailsHTMLAttributes, HTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function DropdownMenu({ className, ...props }: DetailsHTMLAttributes<HTMLDetailsElement>) {
  return <details className={cn('relative', className)} {...props} />
}

export function DropdownMenuTrigger({ className, ...props }: HTMLAttributes<HTMLElement>) {
  return <summary className={cn('cursor-pointer list-none', className)} {...props} />
}

export function DropdownMenuContent({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('absolute right-0 z-50 mt-2 min-w-48 rounded-xl border border-border bg-popover p-1.5 text-popover-foreground shadow-lg', className)} {...props} />
}

export function DropdownMenuItem({ className, ...props }: HTMLAttributes<HTMLButtonElement>) {
  return <button className={cn('flex w-full items-center rounded-lg px-3 py-2 text-left text-sm outline-none hover:bg-muted focus-visible:bg-muted', className)} {...props} />
}
