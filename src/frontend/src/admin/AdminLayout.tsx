import { Suspense } from 'react'
import { useTranslation } from 'react-i18next'
import { Navigate, NavLink, Outlet, useMatch } from 'react-router'
import { NoAccess } from '@/auth/RequirePermission'
import { boxCellClass } from '@/components/boxCell'
import { PageLoader } from '@/components/PageLoader'
import { cn } from '@/lib/utils'
import { useAdminSections } from './useAdminSections'

const tabClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    boxCellClass,
    'shrink-0 border-l whitespace-nowrap first:border-l-0',
    isActive && 'bg-secondary shadow-[inset_0_-3px_0_var(--primary)]',
  )

export function AdminLayout() {
  const { t } = useTranslation()
  const { sections, isLoading } = useAdminSections()
  const isIndex = useMatch('/admin')

  let content = (
    <Suspense fallback={<PageLoader />}>
      <Outlet />
    </Suspense>
  )
  if (isLoading) {
    content = <p className="text-muted-foreground">{t('auth.loading')}</p>
  } else if (!sections.length) {
    content = <NoAccess />
  } else if (isIndex) {
    content = <Navigate to={sections[0].path} replace />
  }

  return (
    <section className="space-y-8">
      <h1 className="text-4xl font-medium text-heading">{t('admin.title')}</h1>
      {sections.length > 0 && (
        <nav className="flex w-fit max-w-full overflow-x-auto border bg-card">
          {sections.map((section) => (
            <NavLink key={section.path} to={section.path} className={tabClass}>
              {t(section.labelKey)}
            </NavLink>
          ))}
        </nav>
      )}
      {content}
    </section>
  )
}
