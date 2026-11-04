import { ArrowLeft, CircleAlert, Vote } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { ImageGallery } from '@/components/ImageGallery'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { PollFacts } from '@/polls/PollFacts'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { PollTypeBadge } from '@/polls/PollTypeBadge'

type City = components['schemas']['CityWithPollsDto']
type Poll = components['schemas']['PollDto']

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
        <ImageGallery key={city.id} images={city.images} alt={city.title ?? ''} />
        <h1 className="text-3xl font-semibold tracking-tight">{city.title}</h1>
        {city.description && (
          <p className="max-w-prose whitespace-pre-line text-muted-foreground">
            {city.description}
          </p>
        )}
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">{t('city.activePolls')}</h2>
        {polls.length ? (
          <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {polls.map((poll, index) => (
              <li key={poll.id}>
                <PollCard poll={poll} number={index + 1} />
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

function PollCard({ poll, number }: Readonly<{ poll: Poll; number: number }>) {
  const cover = coverImage(poll.images)

  return (
    <Link
      to={`/polls/${poll.id}`}
      className="group block h-full rounded-xl outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
    >
      <Card className="h-full pt-0 transition-shadow group-hover:ring-foreground/25">
        {cover?.url ? (
          <img src={cover.url} alt="" loading="lazy" className="aspect-video w-full object-cover" />
        ) : (
          <div className="flex aspect-video items-center justify-center bg-muted text-muted-foreground">
            <Vote className="size-8" />
          </div>
        )}
        <CardHeader>
          <div className="mb-1 flex items-center gap-2">
            <PollLineBadge pollId={poll.id} number={number} />
            <PollTypeBadge type={poll.type} />
          </div>
          <CardTitle className="text-base">{poll.title}</CardTitle>
          {poll.description && (
            <CardDescription className="line-clamp-2">{poll.description}</CardDescription>
          )}
        </CardHeader>
        <CardContent className="mt-auto">
          <PollFacts endsAt={poll.endsAt} budgetAmount={poll.budgetAmount} />
        </CardContent>
      </Card>
    </Link>
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
