import type { ButtonHTMLAttributes, HTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Tabs({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={className} {...props} />
}

export function TabsList({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div role="tablist" className={cn('inline-flex items-center gap-1 rounded-xl bg-muted p-1', className)} {...props} />
}

export function TabsTrigger({ className, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return <button role="tab" type="button" className={cn('rounded-lg px-3 py-2 text-sm text-muted-foreground transition-colors aria-selected:bg-card aria-selected:text-foreground aria-selected:shadow-sm', className)} {...props} />
}

export function TabsContent({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div role="tabpanel" className={cn('pt-4', className)} {...props} />
}
