import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

export const ADMIN_IDEAS_PAGE_SIZE = 20

type IdeaAccessStatus = components['schemas']['IdeaAccessStatus']
type IdeaApprovalStatus = components['schemas']['IdeaApprovalStatus']

interface AdminIdeasQuery {
  search: string
  pollId: string | undefined
  accessStatus: IdeaAccessStatus | undefined
  approvalStatus: IdeaApprovalStatus | undefined
  page: number
}

export function useAdminIdeas({
  search,
  pollId,
  accessStatus,
  approvalStatus,
  page,
}: AdminIdeasQuery) {
  return useQuery({
    queryKey: ['admin', 'ideas', { search, pollId, accessStatus, approvalStatus, page }],
    placeholderData: keepPreviousData,
    queryFn: () =>
      unwrap(
        pollsApi.GET('/api/v1/admin/ideas', {
          params: {
            query: {
              SearchTerm: search || undefined,
              PollId: pollId,
              AccessStatus: accessStatus,
              ApprovalStatus: approvalStatus,
              Page: page,
              PageSize: ADMIN_IDEAS_PAGE_SIZE,
            },
          },
        }),
      ),
  })
}
