import { type QueryClient, useMutation, useQueryClient } from '@tanstack/react-query'
import { usersApi } from '@/api/client'
import { unwrap } from '@/api/errors'

function invalidateUsers(queryClient: QueryClient) {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: ['admin', 'users'] }),
    queryClient.invalidateQueries({ queryKey: ['users'] }),
  ])
}

export function useActivateUser() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(usersApi.POST('/api/v1/Users/{id}/activate', { params: { path: { id } } })),
    onSuccess: () => invalidateUsers(queryClient),
  })
}

export function useDeactivateUser() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(usersApi.POST('/api/v1/Users/{id}/deactivate', { params: { path: { id } } })),
    onSuccess: () => invalidateUsers(queryClient),
  })
}

export function useDeleteUser() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(usersApi.DELETE('/api/v1/Users/{id}', { params: { path: { id } } })),
    onSuccess: () => invalidateUsers(queryClient),
  })
}
