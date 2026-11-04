import { ApiError } from '../api/errors'
import { useCurrentUser } from '../auth/useCurrentUser'
import { usePermissions } from '../auth/usePermissions'

export function ProfilePage() {
  const { data: user, error, isPending } = useCurrentUser()
  const { permissions } = usePermissions()

  if (isPending) {
    return <p className="text-slate-600">Loading profile…</p>
  }

  if (error instanceof ApiError && error.status === 404) {
    return <p className="text-slate-700">Your profile was not found. Please contact support.</p>
  }

  if (error || !user) {
    return (
      <p role="alert" className="text-red-800">
        Could not load your profile.
      </p>
    )
  }

  return (
    <section className="space-y-2">
      <h2 className="text-xl font-semibold text-slate-900">{user.nickname}</h2>
      <p className="text-sm text-slate-600">{permissions.length} permissions</p>
    </section>
  )
}
