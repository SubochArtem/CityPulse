import { ArrowUp } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { cn } from '@/lib/utils'
import { boxCellClass } from './boxCell'
import { RansomText } from './RansomText'

const cellClass = cn(boxCellClass, 'flex-1 text-navy-foreground/80 hover:text-navy-foreground')

export function SiteFooter() {
  const { t } = useTranslation()

  return (
    <footer className="dark torn-top mt-6 bg-navy pt-5 text-navy-foreground">
      <div className="mx-auto w-full max-w-5xl px-4 pb-8 sm:px-6">
        <div className="flex items-stretch">
          <Link to="/" className={cellClass}>
            {t('nav.home')}
          </Link>
          <Link to="/cities" className={cellClass}>
            {t('nav.cities')}
          </Link>
          <div className="hidden items-center px-6 text-base sm:flex">
            <RansomText text="CityPulse" />
          </div>
          <Link to="/profile" className={cellClass}>
            {t('nav.profile')}
          </Link>
          <button
            type="button"
            aria-label={t('footer.toTop')}
            className={cellClass}
            onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
          >
            <ArrowUp className="size-4" />
            <span className="hidden md:inline">{t('footer.toTop')}</span>
          </button>
        </div>
        <div className="mt-4 flex flex-col gap-2 border-t border-dashed border-navy-foreground/30 pt-4 font-label text-[0.7rem] tracking-[0.2em] text-muted-foreground uppercase sm:flex-row sm:items-center sm:justify-between">
          <span>{t('footer.copyright', { year: new Date().getFullYear() })}</span>
          <span>{t('footer.tagline')}</span>
        </div>
      </div>
    </footer>
  )
}
