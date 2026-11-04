import { ArrowLeft, CircleAlert } from 'lucide-react'
import { type ReactNode, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { ImageGallery } from '@/components/ImageGallery'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { cn } from '@/lib/utils'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { PollLineTile } from '@/polls/PollLineTile'
import { pollDaysLeft } from '@/polls/pollTimeline'

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

function Tile({ className, children }: Readonly<{ className?: string; children: ReactNode }>) {
  return (
    <div
      className={cn(
        'flex min-w-0 flex-col gap-3 rounded-xl bg-card p-5 text-card-foreground ring-1 ring-foreground/10',
        className,
      )}
    >
      {children}
    </div>
  )
}

function CityDetails({ city }: Readonly<{ city: City }>) {
  const { t } = useTranslation()
  const polls = [...(city.polls ?? [])].sort((a, b) =>
    (a.endsAt ?? '').localeCompare(b.endsAt ?? ''),
  )
  const [featured, ...others] = polls
  const hasGallery = (city.images?.length ?? 0) > 1

  return (
    <>
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <CityTile city={city} pollCount={polls.length} />
        {featured ? (
          <>
            <PollLineTile poll={featured} number={1} className="col-span-2" />
            <DaysLeftTile poll={featured} wide={!featured.budgetAmount} />
            {featured.budgetAmount ? <BudgetTile amount={featured.budgetAmount} /> : null}
          </>
        ) : (
          <Tile className="col-span-2 justify-center lg:row-span-2">
            <p className="text-muted-foreground">{t('city.noPolls')}</p>
          </Tile>
        )}
        {others.length > 0 && <OtherPollsTile polls={others} />}
        {city.description && (
          <Tile className="col-span-2">
            <h2 className="text-base font-bold">{t('city.about')}</h2>
            <p className="whitespace-pre-line text-muted-foreground">{city.description}</p>
          </Tile>
        )}
      </div>

      {hasGallery && (
        <div className="space-y-4">
          <h2 className="text-xl font-bold">{t('city.photos')}</h2>
          <ImageGallery key={city.id} images={city.images} alt={city.title ?? ''} />
        </div>
      )}
    </>
  )
}

function CityTile({ city, pollCount }: Readonly<{ city: City; pollCount: number }>) {
  const { t } = useTranslation()
  const cover = coverImage(city.images)

  return (
    <div className="relative col-span-2 flex min-h-64 flex-col justify-end overflow-hidden rounded-xl bg-navy p-5 text-navy-foreground lg:row-span-2 lg:min-h-80">
      {cover?.url && (
        <>
          <img src={cover.url} alt="" className="absolute inset-0 size-full object-cover" />
          <div className="absolute inset-0 bg-linear-to-t from-navy via-navy/60 to-navy/10" />
        </>
      )}
      <div className="relative space-y-3">
        <h1 className="text-4xl font-extrabold tracking-tight [overflow-wrap:anywhere] lg:text-6xl">
          {city.title}
        </h1>
        <span className="inline-flex rounded-md bg-primary px-2.5 py-1 text-sm font-semibold text-primary-foreground">
          {t('city.openPolls', { count: pollCount })}
        </span>
      </div>
    </div>
  )
}

function DaysLeftTile({ poll, wide }: Readonly<{ poll: Poll; wide: boolean }>) {
  const { t, i18n } = useTranslation()
  const [now] = useState(() => Date.now())
  const daysLeft = pollDaysLeft(poll.endsAt, now)

  return (
    <Tile
      className={cn(
        'justify-between bg-primary text-primary-foreground ring-0',
        wide && 'col-span-2',
      )}
    >
      <span className="font-heading text-5xl leading-none font-extrabold tabular-nums">
        {new Intl.NumberFormat(i18n.language).format(daysLeft)}
      </span>
      <span className="text-sm font-medium opacity-80">
        {t('city.daysLeft', { count: daysLeft })}
      </span>
    </Tile>
  )
}

function BudgetTile({ amount }: Readonly<{ amount: number }>) {
  const { t, i18n } = useTranslation()

  return (
    <Tile className="justify-between">
      <span className="font-heading text-5xl leading-none font-extrabold tabular-nums">
        {new Intl.NumberFormat(i18n.language, {
          notation: 'compact',
          maximumFractionDigits: 1,
        }).format(amount)}
      </span>
      <span className="text-sm text-muted-foreground">{t('city.budget')}</span>
    </Tile>
  )
}

function OtherPollsTile({ polls }: Readonly<{ polls: Poll[] }>) {
  const { t, i18n } = useTranslation()
  const formatter = new Intl.DateTimeFormat(i18n.language, { month: 'short', day: 'numeric' })

  return (
    <Tile className="col-span-2 gap-1">
      <h2 className="mb-2 text-base font-bold">{t('city.otherPolls')}</h2>
      <ul className="divide-y">
        {polls.map((poll, index) => (
          <li key={poll.id}>
            <Link
              to={`/polls/${poll.id}`}
              className="-mx-2 grid grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-3 rounded-md px-2 py-2.5 outline-none hover:bg-muted focus-visible:ring-3 focus-visible:ring-ring/50"
            >
              <PollLineBadge pollId={poll.id} number={index + 2} />
              <span className="font-medium [overflow-wrap:anywhere]">{poll.title}</span>
              {poll.endsAt && (
                <span className="text-xs whitespace-nowrap text-muted-foreground">
                  {t('city.until', { date: formatter.format(new Date(poll.endsAt)) })}
                </span>
              )}
            </Link>
          </li>
        ))}
      </ul>
    </Tile>
  )
}

function CitySkeleton() {
  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
      <Skeleton className="col-span-2 h-64 rounded-xl lg:row-span-2 lg:h-80" />
      <Skeleton className="col-span-2 h-48 rounded-xl" />
      <Skeleton className="h-28 rounded-xl" />
      <Skeleton className="h-28 rounded-xl" />
    </div>
  )
}
