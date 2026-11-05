import { useAuth } from '@/auth/useAuth'
import { useCurrentUser } from '@/auth/useCurrentUser'

export type FeedbackWriteAccess = 'loading' | 'guest' | 'noPermission' | 'otherCity' | 'allowed'

export function useFeedbackAccess(cityId: string | undefined) {
  const { isLoading: authLoading, isAuthenticated, userId, can } = useAuth()
  const { data: currentUser, isLoading: userLoading } = useCurrentUser()

  let write: FeedbackWriteAccess = 'allowed'
  if (authLoading) {
    write = 'loading'
  } else if (!isAuthenticated) {
    write = 'guest'
  } else if (userLoading) {
    write = 'loading'
  } else if (!can('feedback.create.city')) {
    write = 'noPermission'
  } else if (!currentUser?.cityId || currentUser.cityId !== cityId) {
    write = 'otherCity'
  }

  return {
    userId,
    write,
    canRead: isAuthenticated && can('feedback.read.any'),
    canUpdateOwn: can('feedback.update.own'),
    canDeleteOwn: can('feedback.delete.own'),
    canDeleteAny: can('feedback.delete.any'),
  }
}
