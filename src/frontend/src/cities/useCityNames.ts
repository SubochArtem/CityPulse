import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

const MAX_CITIES = 100

export function useCityNames(enabled = true) {
  return useQuery({
    queryKey: ['cities', 'names'],
    enabled,
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const page = await unwrap(
        pollsApi.GET('/api/v1/cities', { params: { query: { Page: 1, PageSize: MAX_CITIES } } }),
      )
      const names = new Map<string, string>()
      for (const city of page.items ?? []) {
        if (city.id && city.title) {
          names.set(city.id, city.title)
        }
      }
      return names
    },
  })
}
