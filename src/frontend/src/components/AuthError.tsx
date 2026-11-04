import { useAuth0 } from '@auth0/auth0-react'
import { CircleAlert } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'

export function AuthError() {
  const { error } = useAuth0()
  const { t } = useTranslation()

  if (!error) {
    return null
  }

  return (
    <div className="mx-auto w-full max-w-5xl px-6 pt-4">
      <Alert variant="destructive">
        <CircleAlert />
        <AlertTitle>{t('auth.signInFailed')}</AlertTitle>
        <AlertDescription>{error.message}</AlertDescription>
      </Alert>
    </div>
  )
}
