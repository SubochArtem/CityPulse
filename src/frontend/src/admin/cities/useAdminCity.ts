import { useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export function useAdminCity(cityId: string) {
  return useQuery({
    queryKey: ['admin', 'cities', cityId],
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/admin/cities/{id}', {
          params: { path: { id: cityId } },
        }),
      ),
  })
}
