import { type QueryClient, useMutation, useQueryClient } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

type CityStatus = components['schemas']['CityStatus']

function invalidateCities(queryClient: QueryClient) {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: ['admin', 'cities'] }),
    queryClient.invalidateQueries({ queryKey: ['cities'] }),
    queryClient.invalidateQueries({ queryKey: ['polls'] }),
    queryClient.invalidateQueries({ queryKey: ['ideas'] }),
  ])
}

export function useChangeCityStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: CityStatus }) =>
      unwrap(
        pollsApi.PATCH('/api/v1/admin/cities/{id}/status', {
          params: { path: { id } },
          body: { newStatus: status },
        }),
      ),
    onSuccess: () => invalidateCities(queryClient),
  })
}

export function useDeleteCity() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(pollsApi.DELETE('/api/v1/admin/cities/{id}', { params: { path: { id } } })),
    onSuccess: () => invalidateCities(queryClient),
  })
}
