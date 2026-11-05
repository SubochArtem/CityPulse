import { useQuery } from '@tanstack/react-query'
import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'

export const authQueryKey = ['bff', 'user'] as const

interface BffUser {
  isAuthenticated: boolean
  userId: string | null
  internalUserId: string | null
  permissions: string[]
  logoutUrl: string | null
}

async function fetchBffUser(): Promise<BffUser> {
  const response = await fetch('/bff/user', { credentials: 'same-origin' })
  if (!response.ok) {
    throw new Error(`Loading the session failed with ${response.status}`)
  }
  return response.json()
}

export function loginUrl(returnTo: string, locale: string | undefined) {
  const query = new URLSearchParams({ returnUrl: returnTo })
  if (locale) {
    query.set('ui_locales', locale)
  }
  return `/bff/login?${query}`
}

function currentPath() {
  return window.location.pathname + window.location.search
}

export function useAuth() {
  const { i18n } = useTranslation()
  const { data, isPending } = useQuery({
    queryKey: authQueryKey,
    queryFn: fetchBffUser,
    staleTime: Infinity,
  })

  const permissions = data?.permissions ?? []
  const logoutUrl = data?.logoutUrl ?? null
  const locale = i18n.resolvedLanguage

  const login = useCallback(
    (returnTo: string = currentPath()) => {
      window.location.assign(loginUrl(returnTo, locale))
    },
    [locale],
  )

  const logout = useCallback(() => {
    if (logoutUrl) {
      window.location.assign(logoutUrl)
    }
  }, [logoutUrl])

  return {
    isLoading: isPending,
    isAuthenticated: data?.isAuthenticated ?? false,
    userId: data?.internalUserId ?? undefined,
    permissions,
    can: (permission: string) => permissions.includes(permission),
    login,
    logout,
  }
}
