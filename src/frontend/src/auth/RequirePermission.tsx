import { ShieldAlert } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Outlet } from 'react-router'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { usePermissions } from './usePermissions'

export function NoAccess() {
  const { t } = useTranslation()

  return (
    <Alert className="max-w-md">
      <ShieldAlert />
      <AlertTitle>{t('admin.noAccess')}</AlertTitle>
    </Alert>
  )
}

export function RequirePermission({ permission }: Readonly<{ permission: string }>) {
  const { t } = useTranslation()
  const { can, isLoading } = usePermissions()

  if (isLoading) {
    return <p className="text-muted-foreground">{t('auth.loading')}</p>
  }

  if (!can(permission)) {
    return <NoAccess />
  }

  return <Outlet />
}
