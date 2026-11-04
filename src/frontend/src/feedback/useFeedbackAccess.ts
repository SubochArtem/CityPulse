import { useAuth0 } from '@auth0/auth0-react'
import { readInternalUserId } from '@/auth/claims'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { usePermissions } from '@/auth/usePermissions'

export type FeedbackWriteAccess = 'loading' | 'guest' | 'noPermission' | 'otherCity' | 'allowed'

export function useFeedbackAccess(cityId: string | undefined) {
  const { isLoading: authLoading, isAuthenticated, user } = useAuth0()
  const { can, isLoading: permissionsLoading } = usePermissions()
  const { data: currentUser, isLoading: userLoading } = useCurrentUser()

  let write: FeedbackWriteAccess = 'allowed'
  if (authLoading) {
    write = 'loading'
  } else if (!isAuthenticated) {
    write = 'guest'
  } else if (permissionsLoading || userLoading) {
    write = 'loading'
  } else if (!can('feedback.create.city')) {
    write = 'noPermission'
  } else if (!currentUser?.cityId || currentUser.cityId !== cityId) {
    write = 'otherCity'
  }

  return {
    userId: readInternalUserId(user),
    write,
    canRead: isAuthenticated && can('feedback.read.any'),
    canUpdateOwn: can('feedback.update.own'),
    canDeleteOwn: can('feedback.delete.own'),
    canDeleteAny: can('feedback.delete.any'),
  }
}
