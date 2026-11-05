import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { BoxAction } from '@/components/BoxAction'
import { HalftonePoster } from '@/components/HalftonePoster'

const GUEST_SEED = 'citypulse'

export function HomeHero({ cityId }: Readonly<{ cityId: string | null }>) {
  const { t } = useTranslation()

  return (
    <section className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
      <div className="flex flex-col items-center gap-5 text-center">
        <h1 className="max-w-md text-5xl leading-[0.95] font-medium text-heading sm:text-7xl">
          {t('home.heroTitle')}
        </h1>
        <span aria-hidden="true" className="size-2.5 rotate-45 border border-heading" />
        <p className="font-label text-xs tracking-[0.3em] text-heading uppercase">
          <span aria-hidden="true">— </span>
          {t('footer.tagline')}
          <span aria-hidden="true"> —</span>
        </p>
        <p className="max-w-sm text-muted-foreground">{t('home.heroText')}</p>
        {cityId ? (
          <CityAction cityId={cityId} />
        ) : (
          <BoxAction icon={ArrowRight} label={t('home.browseCities')} to="/cities" />
        )}
      </div>
      <HalftonePoster
        seed={cityId ?? GUEST_SEED}
        title="CityPulse"
        className="mx-auto w-full max-w-md"
      />
    </section>
  )
}

function CityAction({ cityId }: Readonly<{ cityId: string }>) {
  const { t } = useTranslation()
  const { data: city } = useCityWithPolls(cityId)
  const label = city?.title ? t('home.openNamedCity', { city: city.title }) : t('home.openCity')

  return <BoxAction icon={ArrowRight} label={label} to={`/cities/${cityId}`} />
}
