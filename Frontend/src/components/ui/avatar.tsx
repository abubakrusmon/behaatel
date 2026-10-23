import type { ImgHTMLAttributes, HTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Avatar({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return <span className={cn('relative flex size-10 shrink-0 overflow-hidden rounded-full bg-muted', className)} {...props} />
}

export function AvatarImage(props: ImgHTMLAttributes<HTMLImageElement>) {
  return <img className="aspect-square size-full object-cover" {...props} />
}

export function AvatarFallback({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return <span className={cn('grid size-full place-items-center bg-accent text-sm font-medium text-primary', className)} {...props} />
}
