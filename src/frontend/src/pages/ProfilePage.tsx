import { CircleAlert } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/api/errors'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { usePermissions } from '@/auth/usePermissions'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

export function ProfilePage() {
  const { t } = useTranslation()
  const { data: user, error, isPending } = useCurrentUser()
  const { permissions } = usePermissions()

  if (isPending) {
    return (
      <Card className="max-w-md">
        <CardHeader>
          <Skeleton className="h-6 w-40" />
          <Skeleton className="h-4 w-24" />
        </CardHeader>
      </Card>
    )
  }

  if (error || !user) {
    const notFound = error instanceof ApiError && error.status === 404
    return (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('profile.notFound') : t('profile.loadFailed')}</AlertTitle>
        {notFound && <AlertDescription>{t('profile.contactSupport')}</AlertDescription>}
      </Alert>
    )
  }

  return (
    <Card className="max-w-md">
      <CardHeader>
        <CardTitle className="text-xl">{user.nickname}</CardTitle>
        <CardDescription>{t('profile.permissions', { count: permissions.length })}</CardDescription>
      </CardHeader>
    </Card>
  )
}
