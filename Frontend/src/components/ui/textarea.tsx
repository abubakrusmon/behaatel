import type { TextareaHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Textarea({ className, ...props }: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea className={cn('min-h-24 w-full rounded-xl border border-input bg-background px-3.5 py-3 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:border-ring', className)} {...props} />
}
