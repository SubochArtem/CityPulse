import { ArrowLeft, CalendarClock, CircleAlert, MapPin, Vote, Wallet } from 'lucide-react'
import { type ReactNode, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage, sortImages } from '@/lib/images'

type City = components['schemas']['CityWithPollsDto']
type Poll = components['schemas']['PollDto']

const pollTypeKeys = {
  1: 'city.pollType.userVote',
  2: 'city.pollType.managerVote',
} as const

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
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('city.notFound') : t('city.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else {
    content = <CityDetails city={city} />
  }

  return (
    <section className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to="/cities">
          <ArrowLeft />
          {t('city.back')}
        </Link>
      </Button>
      {content}
    </section>
  )
}

function CityDetails({ city }: Readonly<{ city: City }>) {
  const { t } = useTranslation()
  const polls = city.polls ?? []

  return (
    <>
      <div className="space-y-4">
        <CityGallery key={city.id} city={city} />
        <h1 className="text-3xl font-semibold tracking-tight">{city.title}</h1>
        {city.description && (
          <p className="max-w-prose whitespace-pre-line text-muted-foreground">
            {city.description}
          </p>
        )}
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">
          {t('city.activePolls', { count: polls.length })}
        </h2>
        {polls.length ? (
          <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {polls.map((poll) => (
              <li key={poll.id}>
                <PollCard poll={poll} />
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-muted-foreground">{t('city.noPolls')}</p>
        )}
      </div>
    </>
  )
}

function CityGallery({ city }: Readonly<{ city: City }>) {
  const images = sortImages(city.images).filter((image) => image.url)
  const [selected, setSelected] = useState(0)
  const current = images[selected]

  if (!current?.url) {
    return (
      <div className="flex aspect-21/9 items-center justify-center rounded-xl bg-muted text-muted-foreground">
        <MapPin className="size-10" />
      </div>
    )
  }

  return (
    <div className="space-y-2">
      <img
        src={current.url}
        alt={city.title ?? ''}
        className="aspect-21/9 w-full rounded-xl object-cover"
      />
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto">
          {images.map((image, index) => (
            <button
              key={image.id}
              type="button"
              onClick={() => setSelected(index)}
              aria-pressed={index === selected}
              className="shrink-0 overflow-hidden rounded-lg opacity-60 ring-offset-background transition-opacity outline-none hover:opacity-100 focus-visible:ring-3 focus-visible:ring-ring/50 aria-pressed:opacity-100 aria-pressed:ring-2 aria-pressed:ring-primary"
            >
              <img src={image.url ?? ''} alt="" className="aspect-video h-16 object-cover" />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

function PollCard({ poll }: Readonly<{ poll: Poll }>) {
  const { t, i18n } = useTranslation()
  const cover = coverImage(poll.images)
  const typeKey = pollTypeKeys[poll.type as keyof typeof pollTypeKeys]
  const endsAt = poll.endsAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(new Date(poll.endsAt))
    : null
  const budget = poll.budgetAmount
    ? new Intl.NumberFormat(i18n.language).format(poll.budgetAmount)
    : null

  return (
    <Card className="h-full pt-0">
      {cover?.url ? (
        <img src={cover.url} alt="" loading="lazy" className="aspect-video w-full object-cover" />
      ) : (
        <div className="flex aspect-video items-center justify-center bg-muted text-muted-foreground">
          <Vote className="size-8" />
        </div>
      )}
      <CardHeader>
        {typeKey && (
          <Badge variant="secondary" className="mb-1">
            {t(typeKey)}
          </Badge>
        )}
        <CardTitle className="text-base">{poll.title}</CardTitle>
        {poll.description && (
          <CardDescription className="line-clamp-2">{poll.description}</CardDescription>
        )}
      </CardHeader>
      {(endsAt || budget) && (
        <CardContent className="mt-auto flex flex-wrap gap-x-4 gap-y-1 text-muted-foreground">
          {endsAt && (
            <span className="inline-flex items-center gap-1.5">
              <CalendarClock className="size-4" />
              {t('city.endsAt', { date: endsAt })}
            </span>
          )}
          {budget && (
            <span className="inline-flex items-center gap-1.5">
              <Wallet className="size-4" />
              {t('city.budget', { amount: budget })}
            </span>
          )}
        </CardContent>
      )}
    </Card>
  )
}

function CitySkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="aspect-21/9 w-full rounded-xl" />
      <Skeleton className="h-9 w-64" />
      <Skeleton className="h-4 w-full max-w-prose" />
      <Skeleton className="h-4 w-2/3 max-w-prose" />
    </div>
  )
}
