import { UserRound } from 'lucide-react'
import { cn } from '@/lib/utils'

/** Аватар пользователя: своя картинка или иконка человека. */
export function UserAvatar({ src, className, iconSize = 18 }: { src?: string | null; className?: string; iconSize?: number }) {
  return (
    <span className={cn('relative grid size-9 shrink-0 place-items-center overflow-hidden rounded-full bg-accent text-primary', className)}>
      {src ? <img src={src} alt="" className="size-full object-cover" /> : <UserRound size={iconSize} aria-hidden="true" />}
    </span>
  )
}
