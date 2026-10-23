import type { HTMLAttributes, PropsWithChildren } from 'react'
import { cn } from '@/lib/utils'

export function Tooltip({ children, content, className }: PropsWithChildren<{ content: string; className?: string }>) {
  return <span className={cn('group relative inline-flex', className)}>{children}<span role="tooltip" className="pointer-events-none absolute bottom-full left-1/2 z-50 mb-2 -translate-x-1/2 whitespace-nowrap rounded-lg bg-foreground px-2.5 py-1.5 text-xs text-background opacity-0 shadow transition-opacity group-hover:opacity-100 group-focus-within:opacity-100">{content}</span></span>
}

export function TooltipContent({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return <span role="tooltip" className={cn('rounded-lg bg-foreground px-2.5 py-1.5 text-xs text-background shadow', className)} {...props} />
}
