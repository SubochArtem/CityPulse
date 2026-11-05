import { CircleAlert } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { CityCard } from '@/cities/CityCard'
import { useCities } from '@/cities/useCities'
import { boxCellClass } from '@/components/boxCell'
import { PillHeading } from '@/components/PillHeading'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

const STRIP_SIZE = 4

export function CitiesStrip({ cityId }: Readonly<{ cityId: string | null }>) {
  const { t } = useTranslation()
  const { data, error, isPending } = useCities({ search: '', page: 1 })

  let content: ReactNode
  if (isPending) {
    content = <StripSkeleton />
  } else if (error) {
    content = (
      <Alert variant="destructive" className="mx-auto max-w-md">
        <CircleAlert />
        <AlertTitle>{t('home.citiesLoadFailed')}</AlertTitle>
      </Alert>
    )
  } else {
    const cities = data.items ?? []
    const ordered = [
      ...cities.filter((city) => city.id === cityId),
      ...cities.filter((city) => city.id !== cityId),
    ].slice(0, STRIP_SIZE)
    const total = data.totalCount ?? ordered.length

    content = (
      <>
        <div className="flex justify-center">
          <span className="rounded-full border bg-card px-3 py-0.5 font-heading text-xs text-heading">
            {t('home.cityCount', { count: total })}
          </span>
        </div>
        <ul className="grid grid-cols-2 gap-y-10 lg:grid-cols-4">
          {ordered.map((city) => (
            <li key={city.id} className="grid">
              <CityCard city={city} highlighted={city.id === cityId} />
            </li>
          ))}
        </ul>
        {total > ordered.length && (
          <div className="flex justify-center">
            <Link to="/cities" className={cn(boxCellClass, 'border bg-card')}>
              {t('home.allCities')}
            </Link>
          </div>
        )}
      </>
    )
  }

  return (
    <section aria-labelledby="cities-strip-title" className="space-y-6">
      <PillHeading id="cities-strip-title">{t('home.pickCity')}</PillHeading>
      {content}
    </section>
  )
}

function StripSkeleton() {
  return (
    <ul className="grid grid-cols-2 gap-y-10 lg:grid-cols-4">
      {Array.from({ length: STRIP_SIZE }, (_, index) => (
        <li key={index} className="flex flex-col items-center gap-3 px-6">
          <Skeleton className="aspect-square w-full" />
          <Skeleton className="h-7 w-28" />
        </li>
      ))}
    </ul>
  )
}
