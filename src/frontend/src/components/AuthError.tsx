import { useAuth0 } from '@auth0/auth0-react'
import { CircleAlert } from 'lucide-react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'

export function AuthError() {
  const { error } = useAuth0()

  if (!error) {
    return null
  }

  return (
    <div className="mx-auto w-full max-w-5xl px-6 pt-4">
      <Alert variant="destructive">
        <CircleAlert />
        <AlertTitle>Sign-in failed</AlertTitle>
        <AlertDescription>{error.message}</AlertDescription>
      </Alert>
    </div>
  )
}
