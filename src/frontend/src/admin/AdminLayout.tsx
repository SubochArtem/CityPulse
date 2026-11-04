import { useTranslation } from 'react-i18next'
import { Navigate, NavLink, Outlet, useMatch } from 'react-router'
import { NoAccess } from '@/auth/RequirePermission'
import { cn } from '@/lib/utils'
import { useAdminSections } from './useAdminSections'

const tabClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'rounded-md px-3 py-1.5 text-sm font-medium whitespace-nowrap transition-colors',
    isActive ? 'bg-muted text-foreground' : 'text-muted-foreground hover:text-foreground',
  )

export function AdminLayout() {
  const { t } = useTranslation()
  const { sections, isLoading } = useAdminSections()
  const isIndex = useMatch('/admin')

  let content = <Outlet />
  if (isLoading) {
    content = <p className="text-muted-foreground">{t('auth.loading')}</p>
  } else if (!sections.length) {
    content = <NoAccess />
  } else if (isIndex) {
    content = <Navigate to={sections[0].path} replace />
  }

  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold tracking-tight">{t('admin.title')}</h1>
      {sections.length > 0 && (
        <nav className="-mx-1 flex gap-1 overflow-x-auto border-b px-1 pb-2">
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
