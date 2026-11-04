import { ArrowUp } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { cn } from '@/lib/utils'
import { boxCellClass } from './boxCell'
import { Emblem } from './Emblem'

const cellClass = cn(boxCellClass, 'flex-1 text-foreground/80')

export function SiteFooter() {
  const { t } = useTranslation()

  return (
    <footer className="mx-auto w-full max-w-5xl px-4 pt-6 pb-8 sm:px-6">
      <div className="flex items-stretch border bg-secondary">
        <Link to="/" className={cellClass}>
          {t('nav.home')}
        </Link>
        <Link to="/cities" className={cn(cellClass, 'border-l')}>
          {t('nav.cities')}
        </Link>
        <div className="hidden items-center border-l px-6 sm:flex">
          <Emblem className="-my-4 bg-background" />
        </div>
        <Link to="/profile" className={cn(cellClass, 'border-l')}>
          {t('nav.profile')}
        </Link>
        <button
          type="button"
          aria-label={t('footer.toTop')}
          className={cn(cellClass, 'border-l')}
          onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
        >
          <ArrowUp className="size-4" />
          <span className="hidden md:inline">{t('footer.toTop')}</span>
        </button>
      </div>
      <div className="mt-4 flex flex-wrap items-center justify-between gap-2 font-heading text-[0.7rem] tracking-[0.2em] text-muted-foreground uppercase">
        <span>{t('footer.copyright', { year: new Date().getFullYear() })}</span>
        <span>{t('footer.tagline')}</span>
      </div>
    </footer>
  )
}
