import { useAuth0 } from '@auth0/auth0-react'
import { useEffect } from 'react'
import { Outlet, useLocation } from 'react-router'

export function RequireAuth() {
  const { isLoading, isAuthenticated, loginWithRedirect } = useAuth0()
  const location = useLocation()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      void loginWithRedirect({
        appState: { returnTo: location.pathname + location.search },
      })
    }
  }, [isLoading, isAuthenticated, loginWithRedirect, location.pathname, location.search])

  if (isLoading || !isAuthenticated) {
    return <p className="p-6 text-slate-600">Loading…</p>
  }

  return <Outlet />
}
