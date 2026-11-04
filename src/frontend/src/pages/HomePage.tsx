import { useAuth0 } from '@auth0/auth0-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ActivityFeed } from '@/activity/ActivityFeed'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { CitiesStrip } from '@/home/CitiesStrip'
import { CityPolls } from '@/home/CityPolls'
import { HomeHero } from '@/home/HomeHero'

type Scope = 'city' | 'all'

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
    <div className="space-y-20">
      <HomeHero cityId={cityId} />
      <CitiesStrip cityId={cityId} />
      {cityId && <CityPolls cityId={cityId} />}
      <ActivityFeed
        cityId={feedCityId}
        actions={cityId && <ScopeToggle cityId={cityId} scope={scope} onChange={setScope} />}
      />
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

function HomeSkeleton() {
  return (
    <div className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
      <div className="flex flex-col items-center gap-4">
        <Skeleton className="h-28 w-80" />
        <Skeleton className="h-4 w-48" />
        <Skeleton className="h-11 w-56" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-xl" />
    </div>
  )
}
