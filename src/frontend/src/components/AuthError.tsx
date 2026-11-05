import { CircleAlert } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'

const LOGIN_FAILED = 'login_failed'

export function AuthError() {
  const [searchParams] = useSearchParams()
  const { t } = useTranslation()

  if (searchParams.get('error') !== LOGIN_FAILED) {
    return null
  }

  const description = searchParams.get('error_description')

  return (
    <div className="mx-auto w-full max-w-5xl px-6 pt-4">
      <Alert variant="destructive">
        <CircleAlert />
        <AlertTitle>{t('auth.signInFailed')}</AlertTitle>
        {description && <AlertDescription>{description}</AlertDescription>}
      </Alert>
    </div>
  )
}
