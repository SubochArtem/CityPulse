import { useAuth0 } from '@auth0/auth0-react'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Outlet, useLocation } from 'react-router'

export function RequireAuth() {
  const { isLoading, isAuthenticated, loginWithRedirect } = useAuth0()
  const location = useLocation()
  const { t, i18n } = useTranslation()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      void loginWithRedirect({
        appState: { returnTo: location.pathname + location.search },
        authorizationParams: { ui_locales: i18n.resolvedLanguage },
      })
    }
  }, [
    isLoading,
    isAuthenticated,
    loginWithRedirect,
    location.pathname,
    location.search,
    i18n.resolvedLanguage,
  ])

  if (isLoading || !isAuthenticated) {
    return <p className="text-muted-foreground">{t('auth.loading')}</p>
  }

  return <Outlet />
}
