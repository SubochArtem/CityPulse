import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function usePoll(pollId: string, enabled = true) {
  return useQuery({
    queryKey: ['polls', pollId, 'summary'],
    enabled,
    retry: false,
    staleTime: 60_000,
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/polls/{id}', {
          params: { path: { id: pollId } },
        }),
      ),
  })
}
