import { ArrowLeft, CircleAlert } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { CityPolls } from '@/cities/CityPolls'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { BoxAction } from '@/components/BoxAction'
import { HalftonePoster } from '@/components/HalftonePoster'
import { ImageGallery } from '@/components/ImageGallery'
import { PillHeading } from '@/components/PillHeading'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'

type City = components['schemas']['CityWithPollsDto']

const heroGrid = 'grid items-center gap-10 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)]'

export function CityPage() {
  const { t } = useTranslation()
  const { cityId = '' } = useParams()
  const { data: city, error, isPending } = useCityWithPolls(cityId)

  let content: ReactNode
  if (isPending) {
    content = <CitySkeleton />
  } else if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    content = (
      <div className="paper poster tape mx-auto max-w-md rotate-1 p-4 pt-6">
        <Alert variant="destructive" className="border-none bg-transparent">
          <CircleAlert />
          <AlertTitle>{notFound ? t('city.notFound') : t('city.loadFailed')}</AlertTitle>
        </Alert>
      </div>
    )
  } else {
    content = <CityDetails city={city} />
  }

  return (
    <div className="space-y-10">
      <BoxAction icon={ArrowLeft} label={t('city.back')} to="/cities" />
      {content}
    </div>
  )
}

function CityDetails({ city }: Readonly<{ city: City }>) {
  const { t } = useTranslation()
  const pollCount = city.polls?.length ?? 0
  const hasGallery = (city.images?.length ?? 0) > 1

  return (
    <div className="space-y-20">
      <section className={heroGrid}>
        <div className="paper poster tape relative -rotate-[0.6deg] px-6 pt-10 pb-8 sm:px-10">
          <span
            aria-hidden="true"
            className="halftone pointer-events-none absolute right-0 bottom-0 h-2/3 w-1/2 text-riot/45 [mask-image:linear-gradient(315deg,black,transparent_75%)]"
          />
          <div className="relative flex flex-col items-start gap-5">
            <p className="bg-ink px-2 py-0.5 font-label text-xs tracking-[0.25em] text-paper uppercase">
              {t('city.openPolls', { count: pollCount })}
            </p>
            <h1 className="text-5xl leading-[0.95] text-foreground [overflow-wrap:anywhere] sm:text-7xl">
              {city.title}
            </h1>
            {city.description && (
              <p className="max-w-md whitespace-pre-line text-muted-foreground">
                {city.description}
              </p>
            )}
          </div>
        </div>
        <div className="group mx-auto w-full max-w-sm">
          <HalftonePoster
            seed={city.id ?? city.title ?? ''}
            title={city.title}
            photoUrl={coverImage(city.images)?.url}
          />
        </div>
      </section>

      {city.id && <CityPolls cityId={city.id} />}

      {hasGallery && (
        <section aria-labelledby="city-photos-title" className="space-y-8">
          <PillHeading id="city-photos-title">{t('city.photos')}</PillHeading>
          <ImageGallery key={city.id} images={city.images} alt={city.title ?? ''} />
        </section>
      )}
    </div>
  )
}

function CitySkeleton() {
  return (
    <div className={heroGrid}>
      <div className="paper poster flex flex-col items-start gap-5 px-6 pt-10 pb-8 sm:px-10">
        <Skeleton className="h-5 w-36" />
        <Skeleton className="h-16 w-64" />
        <Skeleton className="h-12 w-full max-w-md" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-sm" />
    </div>
  )
}
