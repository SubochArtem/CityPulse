import { LogIn, LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/auth/useAuth'
import { boxCellClass } from '@/components/boxCell'
import { cn } from '@/lib/utils'

export function AuthButton({ className }: Readonly<{ className?: string }>) {
  const { isLoading, isAuthenticated, login, logout } = useAuth()
  const { t } = useTranslation()

  if (isLoading) {
    return null
  }

  if (isAuthenticated) {
    return (
      <button
        type="button"
        aria-label={t('auth.logOut')}
        className={cn(boxCellClass, className)}
        onClick={logout}
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
      onClick={() => login()}
    >
      <LogIn className="size-4" />
      <span className="hidden md:inline">{t('auth.logIn')}</span>
    </button>
  )
}
