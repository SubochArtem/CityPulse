import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

const MAX_CITY_OPTIONS = 100

export function useAdminCityOptions() {
  return useQuery({
    queryKey: ['admin', 'cities', 'options'],
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const page = await unwrap(
        pollsApi.GET('/api/v1/admin/cities', {
          params: { query: { Page: 1, PageSize: MAX_CITY_OPTIONS } },
        }),
      )
      return [...(page.items ?? [])]
        .filter((city) => city.id)
        .map((city) => ({ id: city.id ?? '', title: city.title ?? '' }))
        .sort((a, b) => a.title.localeCompare(b.title))
    },
  })
}
