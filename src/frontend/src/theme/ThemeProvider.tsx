import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { ThemeContext, themeStorageKey, type Theme } from './themeContext'

const darkQuery = '(prefers-color-scheme: dark)'

function readStoredTheme(): Theme {
  try {
    const stored = localStorage.getItem(themeStorageKey)
    if (stored === 'light' || stored === 'dark' || stored === 'system') {
      return stored
    }
  } catch {
    return 'system'
  }
  return 'system'
}

function storeTheme(theme: Theme) {
  try {
    localStorage.setItem(themeStorageKey, theme)
  } catch {
    return
  }
}

function applyTheme(dark: boolean) {
  document.documentElement.classList.toggle('dark', dark)
  document.documentElement.style.colorScheme = dark ? 'dark' : 'light'
}

interface ThemeProviderProps {
  readonly children: ReactNode
}

export function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(readStoredTheme)

  useEffect(() => {
    if (theme !== 'system') {
      applyTheme(theme === 'dark')
      return
    }
    const media = window.matchMedia(darkQuery)
    applyTheme(media.matches)
    const onChange = (event: MediaQueryListEvent) => applyTheme(event.matches)
    media.addEventListener('change', onChange)
    return () => media.removeEventListener('change', onChange)
  }, [theme])

  const value = useMemo(
    () => ({
      theme,
      setTheme: (next: Theme) => {
        storeTheme(next)
        setTheme(next)
      },
    }),
    [theme],
  )

  return <ThemeContext value={value}>{children}</ThemeContext>
}
