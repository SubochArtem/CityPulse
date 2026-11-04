import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

export const ADMIN_CITIES_PAGE_SIZE = 20

type CityStatus = components['schemas']['CityStatus']

interface AdminCitiesQuery {
  search: string
  status: CityStatus | undefined
  page: number
}

export function useAdminCities({ search, status, page }: AdminCitiesQuery) {
  return useQuery({
    queryKey: ['admin', 'cities', { search, status, page }],
    placeholderData: keepPreviousData,
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/admin/cities', {
          params: {
            query: {
              SearchTerm: search || undefined,
              Status: status,
              Page: page,
              PageSize: ADMIN_CITIES_PAGE_SIZE,
            },
          },
        }),
      ),
  })
}
