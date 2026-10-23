import type { ButtonHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

type ButtonVariant = 'default' | 'outline' | 'ghost' | 'destructive'
type ButtonSize = 'default' | 'sm' | 'icon'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  size?: ButtonSize
}

export function Button({ className, variant = 'default', size = 'default', ...props }: ButtonProps) {
  return (
    <button
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-xl font-medium transition-colors duration-200 disabled:pointer-events-none disabled:opacity-50',
        variant === 'default' && 'bg-primary text-primary-foreground shadow-sm hover:opacity-90',
        variant === 'outline' && 'border border-border bg-card text-foreground hover:bg-muted',
        variant === 'ghost' && 'text-foreground hover:bg-muted',
        variant === 'destructive' && 'bg-destructive text-white hover:opacity-90',
        size === 'default' && 'min-h-11 px-4 text-sm',
        size === 'sm' && 'min-h-9 px-3 text-sm',
        size === 'icon' && 'size-10',
        className,
      )}
      {...props}
    />
  )
}
