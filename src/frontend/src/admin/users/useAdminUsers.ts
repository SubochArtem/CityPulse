import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { usersApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export const ADMIN_USERS_PAGE_SIZE = 20

interface AdminUsersQuery {
  search: string
  cityId: string | undefined
  page: number
}

export function useAdminUsers({ search, cityId, page }: AdminUsersQuery) {
  return useQuery({
    queryKey: ['admin', 'users', { search, cityId, page }],
    placeholderData: keepPreviousData,
    queryFn: () =>
      unwrap(
        usersApi.GET('/api/v1/Users', {
          params: {
            query: {
              Nickname: search || undefined,
              CityId: cityId,
              Page: page,
              PageSize: ADMIN_USERS_PAGE_SIZE,
            },
          },
        }),
      ),
  })
}
