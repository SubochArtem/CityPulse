import { ShieldCheck, UserRound } from 'lucide-react'
import { Suspense } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet } from 'react-router'
import { useAdminSections } from '@/admin/useAdminSections'
import { cn } from '@/lib/utils'
import { AuthButton } from './AuthButton'
import { AuthError } from './AuthError'
import { boxCellClass, boxIconCellClass } from './boxCell'
import { Emblem } from './Emblem'
import { LanguageSwitcher } from './LanguageSwitcher'
import { PageLoader } from './PageLoader'
import { SiteFooter } from './SiteFooter'
import { ThemeToggle } from './ThemeToggle'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    boxCellClass,
    'border-l',
    isActive
      ? 'bg-secondary text-foreground shadow-[inset_0_-3px_0_var(--primary)]'
      : 'text-foreground/80',
  )

export function Layout() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-svh flex-col bg-background text-foreground">
      <header className="mx-auto w-full max-w-5xl px-4 pt-4 sm:px-6">
        <div className="flex items-stretch border bg-card">
          <Link
            to="/"
            className="flex items-center gap-3 bg-secondary px-3 py-2 outline-none focus-visible:ring-3 focus-visible:ring-ring/50 focus-visible:ring-inset sm:px-4"
          >
            <Emblem />
            <span className="hidden font-heading text-xl font-medium text-heading sm:inline">
              CityPulse
            </span>
          </Link>
          <nav className="flex flex-1 items-stretch justify-end">
            <NavLink to="/cities" className={navLinkClass}>
              {t('nav.cities')}
            </NavLink>
            <NavLink to="/profile" aria-label={t('nav.profile')} className={navLinkClass}>
              <UserRound className="size-4 md:hidden" />
              <span className="hidden md:inline">{t('nav.profile')}</span>
            </NavLink>
            <AdminNavLink />
          </nav>
          <div className="flex items-stretch">
            <div className="border-l">
              <LanguageSwitcher className={boxIconCellClass} />
            </div>
            <div className="border-l">
              <ThemeToggle className={boxIconCellClass} />
            </div>
            <AuthButton className="border-l" />
          </div>
        </div>
      </header>
      <AuthError />
      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8 sm:px-6">
        <Suspense fallback={<PageLoader />}>
          <Outlet />
        </Suspense>
      </main>
      <SiteFooter />
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
    <NavLink to="/admin" aria-label={t('nav.admin')} className={navLinkClass}>
      <ShieldCheck className="size-4" />
      <span className="hidden md:inline">{t('nav.admin')}</span>
    </NavLink>
  )
}
