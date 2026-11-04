import { useAuth0 } from '@auth0/auth0-react'
import { useQuery } from '@tanstack/react-query'
import { readPermissions } from './claims'

export function usePermissions() {
  const { user, isAuthenticated, getAccessTokenSilently } = useAuth0()

  const { data, isLoading } = useQuery({
    queryKey: ['permissions', user?.sub],
    enabled: isAuthenticated,
    queryFn: async () => {
      const token = await getAccessTokenSilently()
      return token ? readPermissions(token) : []
    },
  })

  const permissions = data ?? []

  return {
    permissions,
    isLoading,
    can: (permission: string) => permissions.includes(permission),
  }
}
