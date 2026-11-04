import { ShieldCheck, UserRound } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet } from 'react-router'
import { useAdminSections } from '@/admin/useAdminSections'
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
        <div className="mx-auto flex h-14 w-full max-w-5xl items-center justify-between gap-2 px-4 sm:gap-4 sm:px-6">
          <Link to="/" className="text-base font-semibold tracking-tight sm:text-lg">
            CityPulse
          </Link>
          <div className="flex items-center gap-2 sm:gap-4">
            <nav className="flex items-center gap-2 sm:gap-4">
              <NavLink to="/cities" className={navLinkClass}>
                {t('nav.cities')}
              </NavLink>
              <NavLink
                to="/profile"
                aria-label={t('nav.profile')}
                className={(state) => cn(navLinkClass(state), 'inline-flex items-center gap-1.5')}
              >
                <UserRound className="size-4 sm:hidden" />
                <span className="hidden sm:inline">{t('nav.profile')}</span>
              </NavLink>
              <AdminNavLink />
            </nav>
            <div className="flex items-center sm:gap-2">
              <LanguageSwitcher />
              <ThemeToggle />
              <AuthButton />
            </div>
          </div>
        </div>
      </header>
      <AuthError />
      <main className="mx-auto w-full max-w-5xl px-4 py-8 sm:px-6">
        <Outlet />
      </main>
    </div>
  )
}

function AdminNavLink() {
  const { t } = useTranslation()
  const { sections } = useAdminSections()

  if (!sections.length) {
    return null
  }

  return (
    <NavLink
      to="/admin"
      aria-label={t('nav.admin')}
      className={(state) => cn(navLinkClass(state), 'inline-flex items-center gap-1.5')}
    >
      <ShieldCheck className="size-4" />
      <span className="hidden sm:inline">{t('nav.admin')}</span>
    </NavLink>
  )
}
