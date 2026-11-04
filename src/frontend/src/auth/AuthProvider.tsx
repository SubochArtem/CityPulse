import { Auth0Provider, useAuth0 } from '@auth0/auth0-react'
import type { ReactNode } from 'react'
import { useNavigate } from 'react-router'
import { setTokenGetter } from '../api/client'
import { authConfig } from './config'

interface AuthProviderProps {
  readonly children: ReactNode
}

function TokenBridge({ children }: AuthProviderProps) {
  const { getAccessTokenSilently } = useAuth0()

  setTokenGetter(() => getAccessTokenSilently().catch(() => undefined))

  return children
}

export function AuthProvider({ children }: AuthProviderProps) {
  const navigate = useNavigate()

  return (
    <Auth0Provider
      domain={authConfig.domain}
      clientId={authConfig.clientId}
      authorizationParams={{
        audience: authConfig.audience,
        redirect_uri: window.location.origin,
      }}
      useRefreshTokens
      onRedirectCallback={(appState) => navigate(appState?.returnTo ?? '/', { replace: true })}
    >
      <TokenBridge>{children}</TokenBridge>
    </Auth0Provider>
  )
}
