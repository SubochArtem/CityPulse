import { useAuth0 } from '@auth0/auth0-react'
import { ArrowRight, CircleAlert } from 'lucide-react'
import { type CSSProperties, type ReactNode, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { ActivityFeed } from '@/activity/ActivityFeed'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { cn } from '@/lib/utils'
import { PollLineTile } from '@/polls/PollLineTile'

type Scope = 'city' | 'all'

const decorativeLines = [1, 3, 5]

export function HomePage() {
  const { isAuthenticated, isLoading } = useAuth0()
  const { data: user, isPending: userPending } = useCurrentUser()
  const [scope, setScope] = useState<Scope>('city')
  const resolving = isLoading || (isAuthenticated && userPending)
  const cityId = user?.cityId ?? null

  if (resolving) {
    return <HomeSkeleton />
  }

  const feedCityId = scope === 'city' ? cityId : null

  return (
    <div className="space-y-10">
      {cityId ? <CityHero cityId={cityId} /> : <GuestHero />}
      <ActivityFeed
        cityId={feedCityId}
        actions={cityId && <ScopeToggle cityId={cityId} scope={scope} onChange={setScope} />}
      />
    </div>
  )
}

function HeroTile({ className, children }: Readonly<{ className?: string; children: ReactNode }>) {
  return (
    <div
      className={cn(
        'relative flex min-h-56 flex-col justify-end gap-4 overflow-hidden rounded-xl bg-navy p-6 text-navy-foreground',
        className,
      )}
    >
      {children}
    </div>
  )
}

function GuestHero() {
  const { t } = useTranslation()

  return (
    <HeroTile className="sm:min-h-72">
      <MetroLines />
      <div className="relative max-w-xl space-y-3">
        <h1 className="text-3xl font-extrabold tracking-tight sm:text-5xl">
          {t('home.guestTitle')}
        </h1>
        <p className="text-navy-foreground/80">{t('home.guestText')}</p>
      </div>
      <div className="relative">
        <Button asChild>
          <Link to="/cities">
            {t('home.browseCities')}
            <ArrowRight />
          </Link>
        </Button>
      </div>
    </HeroTile>
  )
}

function MetroLines() {
  return (
    <div
      aria-hidden="true"
      className="pointer-events-none absolute top-6 -right-12 hidden w-72 rotate-[-18deg] flex-col gap-5 opacity-80 sm:flex"
    >
      {decorativeLines.map((line) => (
        <div
          key={line}
          style={{ '--poll-line': `var(--line-${line})` } as CSSProperties}
          className="relative h-2 rounded-full bg-(--poll-line)"
        >
          <span className="absolute top-1/2 left-1/3 size-4 -translate-y-1/2 rounded-full border-4 border-(--poll-line) bg-navy" />
        </div>
      ))}
    </div>
  )
}

function CityHero({ cityId }: Readonly<{ cityId: string }>) {
  const { t } = useTranslation()
  const { data: city, error, isPending } = useCityWithPolls(cityId)

  if (isPending) {
    return <HeroSkeleton />
  }
  if (error) {
    return (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{t('home.cityLoadFailed')}</AlertTitle>
      </Alert>
    )
  }

  const cover = coverImage(city.images)
  const [poll] = [...(city.polls ?? [])].sort((a, b) =>
    (a.endsAt ?? '').localeCompare(b.endsAt ?? ''),
  )

  return (
    <div className="grid gap-3 lg:grid-cols-2">
      <HeroTile>
        {cover?.url && (
          <>
            <img src={cover.url} alt="" className="absolute inset-0 size-full object-cover" />
            <div className="absolute inset-0 bg-linear-to-t from-navy via-navy/70 to-navy/20" />
          </>
        )}
        <div className="relative space-y-2">
          <p className="text-sm font-medium text-navy-foreground/80">{t('home.yourCity')}</p>
          <h1 className="text-4xl font-extrabold tracking-tight [overflow-wrap:anywhere] sm:text-5xl">
            {city.title}
          </h1>
          {city.description && (
            <p className="line-clamp-2 text-sm text-navy-foreground/80">{city.description}</p>
          )}
        </div>
        <div className="relative">
          <Button asChild>
            <Link to={`/cities/${cityId}`}>
              {t('home.openCity')}
              <ArrowRight />
            </Link>
          </Button>
        </div>
      </HeroTile>
      {poll ? (
        <PollLineTile poll={poll} number={1} />
      ) : (
        <div className="flex items-center rounded-xl bg-card p-6 ring-1 ring-foreground/10">
          <p className="text-muted-foreground">{t('home.noActivePoll')}</p>
        </div>
      )}
    </div>
  )
}

interface ScopeToggleProps {
  cityId: string
  scope: Scope
  onChange: (scope: Scope) => void
}

function ScopeToggle({ cityId, scope, onChange }: Readonly<ScopeToggleProps>) {
  const { t } = useTranslation()
  const { data: city } = useCityWithPolls(cityId)
  const options: { value: Scope; label: string }[] = [
    { value: 'city', label: city?.title ?? t('home.yourCity') },
    { value: 'all', label: t('activity.allCities') },
  ]

  return (
    <fieldset className="inline-flex min-w-0 rounded-lg bg-muted p-1">
      <legend className="sr-only">{t('activity.scopeLabel')}</legend>
      {options.map((option) => (
        <Button
          key={option.value}
          size="sm"
          variant={scope === option.value ? 'default' : 'ghost'}
          aria-pressed={scope === option.value}
          onClick={() => onChange(option.value)}
        >
          {option.label}
        </Button>
      ))}
    </fieldset>
  )
}

function HeroSkeleton() {
  return (
    <div className="grid gap-3 lg:grid-cols-2">
      <Skeleton className="min-h-56 rounded-xl" />
      <Skeleton className="min-h-56 rounded-xl" />
    </div>
  )
}

function HomeSkeleton() {
  return (
    <div className="space-y-10">
      <HeroSkeleton />
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-4 w-2/3" />
        <Skeleton className="h-4 w-1/2" />
      </div>
    </div>
  )
}
