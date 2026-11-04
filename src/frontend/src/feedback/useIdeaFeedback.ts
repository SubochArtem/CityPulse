import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { feedbackApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export const COMMENTS_PAGE_SIZE = 10
const LIKE = 0
const COMMENT = 1

function feedbackKey(ideaId: string) {
  return ['feedback', ideaId]
}

export function useIdeaLikes(ideaId: string, userId: string | undefined, enabled: boolean) {
  return useQuery({
    queryKey: [...feedbackKey(ideaId), 'likes', userId],
    enabled,
    queryFn: async () => {
      const [all, mine] = await Promise.all([
        unwrap(
          feedbackApi.GET('/api/v1/feedback', {
            params: { query: { IdeaId: ideaId, Kind: LIKE, PageSize: 1 } },
          }),
        ),
        userId
          ? unwrap(
              feedbackApi.GET('/api/v1/feedback', {
                params: { query: { IdeaId: ideaId, UserId: userId, Kind: LIKE, PageSize: 1 } },
              }),
            )
          : null,
      ])
      return { count: all.totalCount ?? 0, myLikeId: mine?.items?.[0]?.id }
    },
  })
}

export function useIdeaComments(ideaId: string, enabled: boolean) {
  return useInfiniteQuery({
    queryKey: [...feedbackKey(ideaId), 'comments'],
    enabled,
    initialPageParam: 1,
    queryFn: ({ pageParam }) =>
      unwrap(
        feedbackApi.GET('/api/v1/feedback', {
          params: {
            query: { IdeaId: ideaId, Kind: COMMENT, Page: pageParam, PageSize: COMMENTS_PAGE_SIZE },
          },
        }),
      ),
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? (lastPage.page ?? 1) + 1 : undefined),
  })
}

export function useAddFeedback(ideaId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (text: string | null) =>
      unwrap(
        feedbackApi.POST('/api/v1/ideas/{ideaId}/feedback', {
          params: { path: { ideaId } },
          body: { text },
        }),
      ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: feedbackKey(ideaId) }),
  })
}

export function useEditComment(ideaId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, text }: { id: string; text: string }) =>
      unwrap(
        feedbackApi.PUT('/api/v1/feedback/{id}', {
          params: { path: { id } },
          body: { text },
        }),
      ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: feedbackKey(ideaId) }),
  })
}

export function useDeleteFeedback(ideaId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, asAdmin }: { id: string; asAdmin: boolean }) =>
      asAdmin
        ? unwrap(feedbackApi.DELETE('/api/v1/admin/feedback/{id}', { params: { path: { id } } }))
        : unwrap(feedbackApi.DELETE('/api/v1/feedback/{id}', { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: feedbackKey(ideaId) }),
  })
}
