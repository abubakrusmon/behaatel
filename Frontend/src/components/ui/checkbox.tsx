import type { InputHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Checkbox({ className, ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return <input type="checkbox" className={cn('size-4 rounded border-input accent-primary focus-visible:ring-2 focus-visible:ring-ring', className)} {...props} />
}
