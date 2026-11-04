import { useAuth0 } from '@auth0/auth0-react'
import { LogIn, LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { boxCellClass } from '@/components/boxCell'
import { cn } from '@/lib/utils'

export function AuthButton({ className }: Readonly<{ className?: string }>) {
  const { isLoading, isAuthenticated, loginWithRedirect, logout } = useAuth0()
  const { t, i18n } = useTranslation()

  if (isLoading) {
    return null
  }

  if (isAuthenticated) {
    return (
      <button
        type="button"
        aria-label={t('auth.logOut')}
        className={cn(boxCellClass, className)}
        onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
      >
        <LogOut className="size-4" />
        <span className="hidden md:inline">{t('auth.logOut')}</span>
      </button>
    )
  }

  return (
    <button
      type="button"
      aria-label={t('auth.logIn')}
      className={cn(
        boxCellClass,
        'bg-primary text-primary-foreground hover:bg-primary/80 focus-visible:bg-primary/80',
        className,
      )}
      onClick={() =>
        loginWithRedirect({ authorizationParams: { ui_locales: i18n.resolvedLanguage } })
      }
    >
      <LogIn className="size-4" />
      <span className="hidden md:inline">{t('auth.logIn')}</span>
    </button>
  )
}
