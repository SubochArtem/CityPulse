import type { components } from '@/api/schema/polls'
import { useAuth } from '@/auth/useAuth'
import { useCurrentUser } from '@/auth/useCurrentUser'

type Poll = components['schemas']['PollDto']

export type IdeaCreationAccess =
  'loading' | 'closed' | 'guest' | 'noPermission' | 'otherCity' | 'allowed'

const ACTIVE_POLL_STATUS = 1
const CREATE_IDEA_PERMISSION = 'ideas.create.city'

function isPollOpen(poll: Pick<Poll, 'status' | 'endsAt'>) {
  return (
    poll.status === ACTIVE_POLL_STATUS &&
    poll.endsAt !== undefined &&
    new Date(poll.endsAt) > new Date()
  )
}

export function useIdeaCreationAccess(poll: Poll | undefined): IdeaCreationAccess {
  const { isLoading: authLoading, isAuthenticated, can } = useAuth()
  const { data: user, isLoading: userLoading } = useCurrentUser()

  if (!poll) {
    return 'loading'
  }
  if (!isPollOpen(poll)) {
    return 'closed'
  }
  if (authLoading) {
    return 'loading'
  }
  if (!isAuthenticated) {
    return 'guest'
  }
  if (userLoading) {
    return 'loading'
  }
  if (!can(CREATE_IDEA_PERMISSION)) {
    return 'noPermission'
  }
  if (!user?.cityId || user.cityId !== poll.cityId) {
    return 'otherCity'
  }
  return 'allowed'
}
