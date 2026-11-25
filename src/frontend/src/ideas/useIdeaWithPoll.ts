import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function useIdeaWithPoll(ideaId: string, enabled = true) {
  return useQuery({
    queryKey: ['ideas', ideaId, 'poll'],
    enabled,
    staleTime: 30_000,
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/ideas/{id}/poll', {
          params: { path: { id: ideaId } },
        }),
      ),
  })
}
