import { useAuth0 } from '@auth0/auth0-react'
import type { components } from '@/api/schema/polls'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { usePermissions } from '@/auth/usePermissions'

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
  const { isLoading: authLoading, isAuthenticated } = useAuth0()
  const { can, isLoading: permissionsLoading } = usePermissions()
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
  if (permissionsLoading || userLoading) {
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
