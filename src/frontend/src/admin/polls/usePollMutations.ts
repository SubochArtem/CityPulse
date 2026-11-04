import { type QueryClient, useMutation, useQueryClient } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

type PollStatus = components['schemas']['PollStatus']

function invalidatePolls(queryClient: QueryClient) {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: ['admin', 'polls'] }),
    queryClient.invalidateQueries({ queryKey: ['polls'] }),
    queryClient.invalidateQueries({ queryKey: ['cities'] }),
    queryClient.invalidateQueries({ queryKey: ['ideas'] }),
  ])
}

export function useChangePollStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: PollStatus }) =>
      unwrap(
        pollsApi.PATCH('/api/v1/admin/polls/{id}/status', {
          params: { path: { id } },
          body: { newStatus: status },
        }),
      ),
    onSuccess: () => invalidatePolls(queryClient),
  })
}

export function useDeletePoll() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(pollsApi.DELETE('/api/v1/admin/polls/{id}', { params: { path: { id } } })),
    onSuccess: () => invalidatePolls(queryClient),
  })
}
