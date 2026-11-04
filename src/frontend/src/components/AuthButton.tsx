import { useAuth0 } from '@auth0/auth0-react'
import { LogIn, LogOut } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function AuthButton() {
  const { isLoading, isAuthenticated, loginWithRedirect, logout } = useAuth0()

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
        Log out
      </Button>
    )
  }

  return (
    <Button size="sm" onClick={() => loginWithRedirect()}>
      <LogIn />
      Log in
    </Button>
  )
}
