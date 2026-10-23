import type { ButtonHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

interface SwitchProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'onChange'> {
  checked: boolean
  onCheckedChange: (checked: boolean) => void
}

export function Switch({ checked, onCheckedChange, className, ...props }: SwitchProps) {
  return <button type="button" role="switch" aria-checked={checked} onClick={() => onCheckedChange(!checked)} className={cn('relative h-6 w-11 rounded-full bg-muted transition-colors aria-checked:bg-primary', className)} {...props}><span className={cn('absolute left-0.5 top-0.5 size-5 rounded-full bg-white shadow transition-transform', checked && 'translate-x-5')} /></button>
}
