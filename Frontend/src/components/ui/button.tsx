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
        'inline-flex items-center justify-center gap-2 rounded-control font-medium transition-[background-color,color,border-color,box-shadow] duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/35 focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:pointer-events-none disabled:opacity-50',
        variant === 'default' && 'bg-primary text-primary-foreground shadow-sm hover:bg-primary/90',
        variant === 'outline' && 'border border-input bg-card text-foreground shadow-sm hover:bg-muted',
        variant === 'ghost' && 'border border-transparent text-foreground hover:bg-muted',
        variant === 'destructive' && 'bg-destructive text-white hover:opacity-90',
        size === 'default' && 'h-11 px-4 text-sm md:h-10',
        size === 'sm' && 'h-11 px-3 text-sm md:h-10',
        size === 'icon' && 'size-11 md:size-10',
        className,
      )}
      {...props}
    />
  )
}
