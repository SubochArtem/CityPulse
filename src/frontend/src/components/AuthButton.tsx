import { useAuth0 } from '@auth0/auth0-react'
import { LogIn, LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

export function AuthButton() {
  const { isLoading, isAuthenticated, loginWithRedirect, logout } = useAuth0()
  const { t, i18n } = useTranslation()

  if (isLoading) {
    return null
  }

  if (isAuthenticated) {
    return (
      <Button
        variant="outline"
        size="sm"
        aria-label={t('auth.logOut')}
        onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
      >
        <LogOut />
        <span className="hidden sm:inline">{t('auth.logOut')}</span>
      </Button>
    )
  }

  return (
    <Button
      size="sm"
      aria-label={t('auth.logIn')}
      onClick={() =>
        loginWithRedirect({ authorizationParams: { ui_locales: i18n.resolvedLanguage } })
      }
    >
      <LogIn />
      <span className="hidden sm:inline">{t('auth.logIn')}</span>
    </Button>
  )
}
