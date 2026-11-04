import { LoaderCircle } from 'lucide-react'
import { useTranslation } from 'react-i18next'

export function PageLoader() {
  const { t } = useTranslation()

  return (
    <div
      role="status"
      className="flex justify-center py-16 text-muted-foreground animate-in delay-300 duration-300 fade-in-0 fill-mode-backwards"
    >
      <LoaderCircle className="animate-spin" aria-hidden />
      <span className="sr-only">{t('common.loading')}</span>
    </div>
  )
}
