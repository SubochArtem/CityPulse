import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet } from 'react-router'
import { cn } from '@/lib/utils'
import { AuthButton } from './AuthButton'
import { AuthError } from './AuthError'
import { LanguageSwitcher } from './LanguageSwitcher'
import { ThemeToggle } from './ThemeToggle'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'text-sm transition-colors hover:text-foreground',
    isActive ? 'font-medium text-foreground' : 'text-muted-foreground',
  )

export function Layout() {
  const { t } = useTranslation()

  return (
    <div className="min-h-svh bg-background text-foreground">
      <header className="border-b">
        <div className="mx-auto flex h-14 w-full max-w-5xl items-center justify-between px-6">
          <Link to="/" className="text-lg font-semibold tracking-tight">
            CityPulse
          </Link>
          <nav className="flex items-center gap-4">
            <NavLink to="/cities" className={navLinkClass}>
              {t('nav.cities')}
            </NavLink>
            <NavLink to="/profile" className={navLinkClass}>
              {t('nav.profile')}
            </NavLink>
            <LanguageSwitcher />
            <ThemeToggle />
            <AuthButton />
          </nav>
        </div>
      </header>
      <AuthError />
      <main className="mx-auto w-full max-w-5xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  )
}
