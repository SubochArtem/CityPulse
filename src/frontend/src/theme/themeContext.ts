import { createContext } from 'react'

export type Theme = 'light' | 'dark' | 'system'

export interface ThemeContextValue {
  readonly theme: Theme
  readonly setTheme: (theme: Theme) => void
}

export const themeStorageKey = 'citypulse-theme'

export const ThemeContext = createContext<ThemeContextValue | undefined>(undefined)
