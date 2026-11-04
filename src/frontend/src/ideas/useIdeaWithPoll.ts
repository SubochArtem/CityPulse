import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function useIdeaWithPoll(ideaId: string) {
  return useQuery({
    queryKey: ['ideas', ideaId, 'poll'],
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/ideas/{id}/poll', {
          params: { path: { id: ideaId } },
        }),
      ),
  })
}
