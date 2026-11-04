import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

export const ADMIN_POLLS_PAGE_SIZE = 20

type PollStatus = components['schemas']['PollStatus']
type PollType = components['schemas']['PollType']

interface AdminPollsQuery {
  search: string
  cityId: string | undefined
  status: PollStatus | undefined
  type: PollType | undefined
  page: number
}

export function useAdminPolls({ search, cityId, status, type, page }: AdminPollsQuery) {
  return useQuery({
    queryKey: ['admin', 'polls', { search, cityId, status, type, page }],
    placeholderData: keepPreviousData,
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/admin/polls', {
          params: {
            query: {
              SearchTerm: search || undefined,
              CityId: cityId,
              Status: status,
              Type: type,
              Page: page,
              PageSize: ADMIN_POLLS_PAGE_SIZE,
            },
          },
        }),
      ),
  })
}
