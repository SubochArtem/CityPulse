import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import { MAX_OPTIONS, toOptions } from '@/admin/options'

export function useAdminPollOptions() {
  return useQuery({
    queryKey: ['admin', 'polls', 'options'],
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const page = await unwrap(
        pollsApi.GET('/api/v1/admin/polls', {
          params: { query: { Page: 1, PageSize: MAX_OPTIONS } },
        }),
      )
      return toOptions(page.items)
    },
  })
}
