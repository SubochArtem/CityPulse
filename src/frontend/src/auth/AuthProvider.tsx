import { Auth0Provider, useAuth0 } from '@auth0/auth0-react'
import type { ReactNode } from 'react'
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
  return (
    <Auth0Provider
      domain={authConfig.domain}
      clientId={authConfig.clientId}
      authorizationParams={{
        audience: authConfig.audience,
        redirect_uri: window.location.origin,
      }}
      useRefreshTokens
    >
      <TokenBridge>{children}</TokenBridge>
    </Auth0Provider>
  )
}
