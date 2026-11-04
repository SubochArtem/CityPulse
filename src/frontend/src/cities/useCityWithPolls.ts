import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function useCityWithPolls(cityId: string) {
  return useQuery({
    queryKey: ['cities', cityId, 'polls'],
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/cities/{id}/polls', {
          params: { path: { id: cityId } },
        }),
      ),
  })
}
