import { ArrowLeft, CircleAlert } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { CityPolls } from '@/cities/CityPolls'
import { IsoCityTile } from '@/cities/IsoCityTile'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { BoxAction } from '@/components/BoxAction'
import { ImageGallery } from '@/components/ImageGallery'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'

type City = components['schemas']['CityWithPollsDto']

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
      <Alert variant="destructive" className="mx-auto max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('city.notFound') : t('city.loadFailed')}</AlertTitle>
      </Alert>
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
      <section className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
        <div className="flex flex-col items-center gap-5 text-center">
          <span className="font-heading text-xs tracking-[0.25em] text-coral uppercase">
            {t('city.openPolls', { count: pollCount })}
          </span>
          <h1 className="max-w-md text-5xl leading-[0.95] font-medium text-heading [overflow-wrap:anywhere] sm:text-7xl">
            {city.title}
          </h1>
          <span aria-hidden="true" className="size-2.5 rotate-45 border border-heading" />
          {city.description && (
            <p className="max-w-md whitespace-pre-line text-muted-foreground">{city.description}</p>
          )}
        </div>
        <div className="group mx-auto w-full max-w-xl">
          <IsoCityTile seed={city.id ?? city.title ?? ''} photoUrl={coverImage(city.images)?.url} />
        </div>
      </section>

      {city.id && <CityPolls cityId={city.id} />}

      {hasGallery && (
        <section aria-labelledby="city-photos-title" className="space-y-6">
          <div className="flex justify-center">
            <h2
              id="city-photos-title"
              className="rounded-full border px-8 py-2 text-center text-2xl font-normal text-heading outline outline-offset-4 outline-border"
            >
              {t('city.photos')}
            </h2>
          </div>
          <ImageGallery key={city.id} images={city.images} alt={city.title ?? ''} />
        </section>
      )}
    </div>
  )
}

function CitySkeleton() {
  return (
    <div className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
      <div className="flex flex-col items-center gap-4">
        <Skeleton className="h-4 w-32" />
        <Skeleton className="h-20 w-64" />
        <Skeleton className="h-12 w-80" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-xl" />
    </div>
  )
}
