import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { BoxAction } from '@/components/BoxAction'
import { HalftonePoster } from '@/components/HalftonePoster'
import { RansomText } from '@/components/RansomText'
import { coverImage } from '@/lib/images'

const GUEST_SEED = 'citypulse'

export function HomeHero({ cityId }: Readonly<{ cityId: string | null }>) {
  const { t } = useTranslation()

  return (
    <section className="grid items-center gap-10 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)]">
      <div className="paper poster tape relative -rotate-[0.6deg] px-6 pt-10 pb-8 sm:px-10">
        <span
          aria-hidden="true"
          className="halftone pointer-events-none absolute right-0 bottom-0 h-2/3 w-1/2 text-riot/45 [mask-image:linear-gradient(315deg,black,transparent_75%)]"
        />
        <div className="relative flex flex-col items-start gap-5">
          <p className="bg-ink px-2 py-0.5 font-label text-xs tracking-[0.25em] text-paper uppercase">
            {t('footer.tagline')}
          </p>
          <h1 className="text-4xl sm:text-6xl">
            <RansomText text={t('home.heroTitle')} />
          </h1>
          <p className="max-w-md text-muted-foreground">{t('home.heroText')}</p>
          {cityId ? (
            <CityAction cityId={cityId} />
          ) : (
            <BoxAction icon={ArrowRight} label={t('home.browseCities')} to="/cities" />
          )}
        </div>
      </div>
      {cityId ? (
        <CityPoster cityId={cityId} />
      ) : (
        <HalftonePoster seed={GUEST_SEED} title="CityPulse" className="mx-auto w-full max-w-sm" />
      )}
    </section>
  )
}

function CityAction({ cityId }: Readonly<{ cityId: string }>) {
  const { t } = useTranslation()
  const { data: city } = useCityWithPolls(cityId)
  const label = city?.title ? t('home.openNamedCity', { city: city.title }) : t('home.openCity')

  return <BoxAction icon={ArrowRight} label={label} to={`/cities/${cityId}`} />
}

function CityPoster({ cityId }: Readonly<{ cityId: string }>) {
  const { data: city } = useCityWithPolls(cityId)

  return (
    <Link
      to={`/cities/${cityId}`}
      aria-label={city?.title ?? undefined}
      className="group mx-auto block w-full max-w-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
    >
      <HalftonePoster seed={cityId} title={city?.title} photoUrl={coverImage(city?.images)?.url} />
    </Link>
  )
}
