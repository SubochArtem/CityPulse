import { useAuth0 } from '@auth0/auth0-react'

const buttonClass =
  'rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-700'

export function AuthButton() {
  const { isLoading, isAuthenticated, loginWithRedirect, logout } = useAuth0()

  if (isLoading) {
    return null
  }

  if (isAuthenticated) {
    return (
      <button
        className={buttonClass}
        onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
      >
        Log out
      </button>
    )
  }

  return (
    <button className={buttonClass} onClick={() => loginWithRedirect()}>
      Log in
    </button>
  )
}
