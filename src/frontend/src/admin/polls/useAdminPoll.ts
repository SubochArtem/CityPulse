import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function useAdminPoll(pollId: string) {
  return useQuery({
    queryKey: ['admin', 'polls', pollId],
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/admin/polls/{id}', {
          params: { path: { id: pollId } },
        }),
      ),
  })
}
