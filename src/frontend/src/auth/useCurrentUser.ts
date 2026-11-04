import { useAuth0 } from '@auth0/auth0-react'
import { useQuery } from '@tanstack/react-query'
import { usersApi } from '../api/client'
import { unwrap } from '../api/errors'
import { readInternalUserId } from './claims'

export function useCurrentUser() {
  const { user, isAuthenticated } = useAuth0()
  const userId = readInternalUserId(user)

  return useQuery({
    queryKey: ['users', userId],
    enabled: isAuthenticated && Boolean(userId),
    queryFn: () =>
      unwrap(usersApi.GET('/api/v1/Users/{id}', { params: { path: { id: userId! } } })),
  })
}
