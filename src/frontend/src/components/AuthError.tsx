import { useAuth0 } from '@auth0/auth0-react'

export function AuthError() {
  const { error } = useAuth0()

  if (!error) {
    return null
  }

  return (
    <p role="alert" className="mx-6 rounded-md bg-red-50 px-4 py-3 text-sm text-red-800">
      Sign-in failed: {error.message}
    </p>
  )
}
