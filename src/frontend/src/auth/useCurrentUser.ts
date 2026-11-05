import { useQuery } from '@tanstack/react-query'
import { usersApi } from '../api/client'
import { unwrap } from '../api/errors'
import { useAuth } from './useAuth'

export function useCurrentUser() {
  const { isAuthenticated, userId } = useAuth()

  return useQuery({
    queryKey: ['users', userId],
    enabled: isAuthenticated && Boolean(userId),
    queryFn: () =>
      unwrap(usersApi.GET('/api/v1/Users/{id}', { params: { path: { id: userId! } } })),
  })
}
