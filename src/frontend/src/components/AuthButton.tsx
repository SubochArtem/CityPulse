import { useAuth0 } from '@auth0/auth0-react'
import { LogIn, LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

export function AuthButton() {
  const { isLoading, isAuthenticated, loginWithRedirect, logout } = useAuth0()
  const { t } = useTranslation()

  if (isLoading) {
    return null
  }

  if (isAuthenticated) {
    return (
      <Button
        variant="outline"
        size="sm"
        onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
      >
        <LogOut />
        {t('auth.logOut')}
      </Button>
    )
  }

  return (
    <Button size="sm" onClick={() => loginWithRedirect()}>
      <LogIn />
      {t('auth.logIn')}
    </Button>
  )
}
