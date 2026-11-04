import { CircleAlert } from 'lucide-react'
import { ApiError } from '@/api/errors'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { usePermissions } from '@/auth/usePermissions'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

export function ProfilePage() {
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
        <AlertTitle>{notFound ? 'Profile not found' : 'Could not load your profile'}</AlertTitle>
        {notFound && <AlertDescription>Please contact support.</AlertDescription>}
      </Alert>
    )
  }

  return (
    <Card className="max-w-md">
      <CardHeader>
        <CardTitle className="text-xl">{user.nickname}</CardTitle>
        <CardDescription>{permissions.length} permissions</CardDescription>
      </CardHeader>
    </Card>
  )
}
