import { type QueryClient, useMutation, useQueryClient } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

type IdeaAccessStatus = components['schemas']['IdeaAccessStatus']
type IdeaApprovalStatus = components['schemas']['IdeaApprovalStatus']

function invalidateIdeas(queryClient: QueryClient) {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: ['admin', 'ideas'] }),
    queryClient.invalidateQueries({ queryKey: ['ideas'] }),
    queryClient.invalidateQueries({ queryKey: ['polls'] }),
  ])
}

export function useChangeIdeaApproval() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: IdeaApprovalStatus }) =>
      unwrap(
        pollsApi.PATCH('/api/v1/admin/ideas/{id}/approval-status', {
          params: { path: { id } },
          body: { newApprovalStatus: status },
        }),
      ),
    onSuccess: () => invalidateIdeas(queryClient),
  })
}

export function useChangeIdeaAccess() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: IdeaAccessStatus }) =>
      unwrap(
        pollsApi.PATCH('/api/v1/admin/ideas/{id}/access-status', {
          params: { path: { id } },
          body: { newIdeaAccessStatus: status },
        }),
      ),
    onSuccess: () => invalidateIdeas(queryClient),
  })
}

export function useDeleteIdea() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(pollsApi.DELETE('/api/v1/admin/ideas/{id}', { params: { path: { id } } })),
    onSuccess: () => invalidateIdeas(queryClient),
  })
}
