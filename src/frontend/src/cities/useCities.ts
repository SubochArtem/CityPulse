import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export const CITIES_PAGE_SIZE = 12

interface CitiesQuery {
  search: string
  page: number
}

export function useCities({ search, page }: CitiesQuery) {
  return useQuery({
    queryKey: ['cities', { search, page }],
    placeholderData: keepPreviousData,
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/cities', {
          params: {
            query: {
              SearchTerm: search || undefined,
              IncludeImages: true,
              Page: page,
              PageSize: CITIES_PAGE_SIZE,
            },
          },
        }),
      ),
  })
}
