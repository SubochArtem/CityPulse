import { Building2, ShieldCheck, UserRound } from 'lucide-react'
import { Suspense } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet } from 'react-router'
import { useAdminSections } from '@/admin/useAdminSections'
import { cn } from '@/lib/utils'
import { AuthButton } from './AuthButton'
import { AuthError } from './AuthError'
import { boxCellClass, boxIconCellClass } from './boxCell'
import { LanguageSwitcher } from './LanguageSwitcher'
import { PageLoader } from './PageLoader'
import { RansomText } from './RansomText'
import { SiteFooter } from './SiteFooter'
import { ThemeToggle } from './ThemeToggle'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    boxCellClass,
    isActive
      ? 'bg-primary text-primary-foreground hover:bg-primary/85 focus-visible:bg-primary/85'
      : 'text-navy-foreground/80 hover:text-navy-foreground',
  )

export function Layout() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-svh flex-col bg-background text-foreground">
      <header className="dark torn-bottom bg-navy pb-3 text-navy-foreground">
        <div className="mx-auto flex w-full max-w-5xl items-stretch gap-2 px-4 pt-3 sm:px-6">
          <Link
            to="/"
            className="flex items-center pr-2 text-lg outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:text-xl"
          >
            <RansomText text="CityPulse" className="hidden sm:inline-flex" />
            <RansomText text="CP" label="CityPulse" className="sm:hidden" />
          </Link>
          <nav className="flex flex-1 items-stretch justify-end">
            <NavLink to="/cities" aria-label={t('nav.cities')} className={navLinkClass}>
              <Building2 className="size-4 md:hidden" />
              <span className="hidden md:inline">{t('nav.cities')}</span>
            </NavLink>
            <NavLink to="/profile" aria-label={t('nav.profile')} className={navLinkClass}>
              <UserRound className="size-4 md:hidden" />
              <span className="hidden md:inline">{t('nav.profile')}</span>
            </NavLink>
            <AdminNavLink />
          </nav>
          <div className="flex items-stretch">
            <LanguageSwitcher className={boxIconCellClass} />
            <ThemeToggle className={boxIconCellClass} />
            <AuthButton />
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
