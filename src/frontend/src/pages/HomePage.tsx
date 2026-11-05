import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ActivityFeed } from '@/activity/ActivityFeed'
import { useAuth } from '@/auth/useAuth'
import { useCurrentUser } from '@/auth/useCurrentUser'
import { useCityWithPolls } from '@/cities/useCityWithPolls'
import { boxCellClass } from '@/components/boxCell'
import { Skeleton } from '@/components/ui/skeleton'
import { CitiesStrip } from '@/home/CitiesStrip'
import { CityPolls } from '@/cities/CityPolls'
import { HomeHero } from '@/home/HomeHero'
import { cn } from '@/lib/utils'

type Scope = 'city' | 'all'

export function HomePage() {
  const { isAuthenticated, isLoading } = useAuth()
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
    <fieldset className="inline-flex min-w-0 border-2 border-foreground bg-card">
      <legend className="sr-only">{t('activity.scopeLabel')}</legend>
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          aria-pressed={scope === option.value}
          onClick={() => onChange(option.value)}
          className={cn(
            boxCellClass,
            'min-w-0 border-l-2 border-foreground first:border-l-0',
            scope === option.value &&
              'bg-primary text-primary-foreground hover:bg-primary/85 focus-visible:bg-primary/85',
          )}
        >
          <span className="truncate">{option.label}</span>
        </button>
      ))}
    </fieldset>
  )
}

function HomeSkeleton() {
  return (
    <div className="grid items-center gap-10 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)]">
      <Skeleton className="h-80" />
      <Skeleton className="mx-auto aspect-square w-full max-w-sm" />
    </div>
  )
}
