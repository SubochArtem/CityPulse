import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function usePollWithIdeas(pollId: string) {
  return useQuery({
    queryKey: ['polls', pollId, 'ideas'],
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/polls/{id}/ideas', {
          params: { path: { id: pollId } },
        }),
      ),
  })
}
