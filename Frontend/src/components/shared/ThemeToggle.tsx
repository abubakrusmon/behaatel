import { Monitor, Moon, Sun } from 'lucide-react'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useThemeStore, type Theme } from '@/store/themeStore'
import { t } from '@/i18n/ru'
import { cn } from '@/lib/utils'

const themes: Array<{ value: Theme; label: string; icon: typeof Sun }> = [
  { value: 'light', label: t('common.light'), icon: Sun },
  { value: 'dark', label: t('common.dark'), icon: Moon },
  { value: 'system', label: t('common.system'), icon: Monitor },
]

export function ThemeToggle({ compact = false, className }: { compact?: boolean; className?: string }) {
  const theme = useThemeStore((state) => state.theme)
  const setTheme = useThemeStore((state) => state.setTheme)
  const Icon = themes.find((item) => item.value === theme)?.icon ?? Monitor
  return (
    <Select value={theme} onValueChange={(value) => setTheme(value as Theme)}>
      <SelectTrigger className={className ? cn('relative pl-9', className) : compact ? 'relative w-11 justify-center px-0 sm:w-[170px] sm:justify-between sm:pl-9 sm:pr-3' : 'relative w-[170px] pl-9'} aria-label={t('shared.themeOptions')}>
        <Icon className={compact ? 'text-muted-foreground sm:absolute sm:left-3' : 'pointer-events-none absolute left-3 text-muted-foreground'} size={16} aria-hidden="true" />
        <SelectValue className={compact ? 'hidden sm:inline' : undefined} placeholder={t('common.theme')} />
      </SelectTrigger>
      <SelectContent>
        {themes.map((item) => <SelectItem key={item.value} value={item.value}>{item.label}</SelectItem>)}
      </SelectContent>
    </Select>
  )
}
