import { type InfiniteData, type QueryClient, useInfiniteQuery } from '@tanstack/react-query'
import { feedbackApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/feedback'

export type Activity = components['schemas']['ActivityResponse']
type ActivityPage = components['schemas']['ActivityPage']

export const ACTIVITY_PAGE_SIZE = 20

export function activityKey(cityId: string | null) {
  return ['activity', cityId ?? 'all'] as const
}

export function useActivityFeed(cityId: string | null) {
  return useInfiniteQuery({
    queryKey: activityKey(cityId),
    staleTime: 0,
    initialPageParam: undefined as string | undefined,
    queryFn: ({ pageParam }) =>
      unwrap(
        feedbackApi.GET('/api/v1/activity', {
          params: {
            query: {
              CityId: cityId ?? undefined,
              Cursor: pageParam,
              PageSize: ACTIVITY_PAGE_SIZE,
            },
          },
        }),
      ),
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  })
}

export function prependActivity(
  queryClient: QueryClient,
  cityId: string | null,
  entries: Activity[],
) {
  queryClient.setQueryData<InfiniteData<ActivityPage, string | undefined>>(
    activityKey(cityId),
    (data) => {
      if (!data || data.pages.length === 0) {
        return data
      }
      const [first, ...rest] = data.pages
      const fresh = entries.filter((entry) => !first.items?.some((item) => item.id === entry.id))
      return {
        ...data,
        pages: [{ ...first, items: [...fresh, ...(first.items ?? [])] }, ...rest],
      }
    },
  )
}
