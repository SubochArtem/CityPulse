import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Outlet, useLocation } from 'react-router'
import { useAuth } from './useAuth'

export function RequireAuth() {
  const { isLoading, isAuthenticated, login } = useAuth()
  const location = useLocation()
  const { t } = useTranslation()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      login(location.pathname + location.search)
    }
  }, [isLoading, isAuthenticated, login, location.pathname, location.search])

  if (isLoading || !isAuthenticated) {
    return <p className="text-muted-foreground">{t('auth.loading')}</p>
  }

  return <Outlet />
}
