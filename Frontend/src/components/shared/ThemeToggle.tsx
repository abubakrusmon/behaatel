import { Monitor, Moon, Sun } from 'lucide-react'
import { useThemeStore, type Theme } from '@/store/themeStore'
import { t } from '@/i18n/ru'

const themes: Array<{ value: Theme; label: string; icon: typeof Sun }> = [
  { value: 'light', label: t('common.light'), icon: Sun },
  { value: 'dark', label: t('common.dark'), icon: Moon },
  { value: 'system', label: t('common.system'), icon: Monitor },
]

export function ThemeToggle() {
  const theme = useThemeStore((state) => state.theme)
  const setTheme = useThemeStore((state) => state.setTheme)
  const Icon = themes.find((item) => item.value === theme)?.icon ?? Monitor
  return (
    <label className="relative inline-flex items-center" aria-label={t('common.theme')}>
      <Icon className="pointer-events-none absolute left-3 text-muted-foreground" size={16} aria-hidden="true" />
      <select
        aria-label={t('shared.themeOptions')}
        className="h-10 appearance-none rounded-xl border border-border bg-card pl-9 pr-3 text-sm text-foreground"
        value={theme}
        onChange={(event) => setTheme(event.target.value as Theme)}
      >
        {themes.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
      </select>
    </label>
  )
}
