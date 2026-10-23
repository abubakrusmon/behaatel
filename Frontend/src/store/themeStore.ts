import { create } from 'zustand'

export type Theme = 'light' | 'dark' | 'system'
const THEME_KEY = 'behaatel-theme'

function applyTheme(theme: Theme): void {
  const dark = theme === 'dark' || (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches)
  document.documentElement.classList.toggle('dark', dark)
}

interface ThemeState {
  theme: Theme
  setTheme: (theme: Theme) => void
}

const initialTheme = (localStorage.getItem(THEME_KEY) as Theme | null) ?? 'system'
applyTheme(initialTheme)

export const useThemeStore = create<ThemeState>((set) => ({
  theme: initialTheme,
  setTheme: (theme) => {
    localStorage.setItem(THEME_KEY, theme)
    applyTheme(theme)
    set({ theme })
  },
}))
