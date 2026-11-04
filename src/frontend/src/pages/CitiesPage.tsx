import { CircleAlert, MapPin, Search } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { CITIES_PAGE_SIZE, useCities } from '@/cities/useCities'
import { Pagination } from '@/components/Pagination'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { useListSearchParams } from '@/lib/useListSearchParams'

type City = components['schemas']['CityDto']

export function CitiesPage() {
  const { t } = useTranslation()
  const { search, page, searchInput, setSearchInput, goToPage } = useListSearchParams()
  const { data, error, isPending, isPlaceholderData } = useCities({ search, page })
  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / CITIES_PAGE_SIZE))

  let content: ReactNode
  if (isPending) {
    content = <CityGridSkeleton />
  } else if (error) {
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{t('cities.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else if (data.totalCount) {
    content = (
      <>
        <ul
          className="grid gap-4 transition-opacity sm:grid-cols-2 lg:grid-cols-3 data-[stale=true]:opacity-60"
          data-stale={isPlaceholderData}
        >
          {data.items?.map((city) => (
            <li key={city.id}>
              <CityCard city={city} />
            </li>
          ))}
        </ul>

        <Pagination
          page={page}
          totalPages={totalPages}
          hasPrevious={Boolean(data.hasPreviousPage)}
          hasNext={Boolean(data.hasNextPage)}
          disabled={isPlaceholderData}
          onPageChange={goToPage}
        />
      </>
    )
  } else {
    content = (
      <p className="text-muted-foreground">
        {search ? t('cities.noMatches', { search }) : t('cities.empty')}
      </p>
    )
  }

  return (
    <section className="space-y-6">
      <div className="space-y-2">
        <h1 className="text-3xl font-semibold tracking-tight">{t('cities.title')}</h1>
        <p className="text-muted-foreground">{t('cities.subtitle')}</p>
      </div>

      <div className="relative max-w-sm">
        <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          type="search"
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          placeholder={t('cities.searchPlaceholder')}
          aria-label={t('cities.searchPlaceholder')}
          className="pl-8"
        />
      </div>

      {content}
    </section>
  )
}

function CityCard({ city }: Readonly<{ city: City }>) {
  const cover = coverImage(city.images)

  return (
    <Link
      to={`/cities/${city.id}`}
      className="group block h-full rounded-xl outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
    >
      <Card className="h-full pt-0 transition-shadow group-hover:ring-foreground/25">
        {cover?.url ? (
          <img src={cover.url} alt="" loading="lazy" className="aspect-video w-full object-cover" />
        ) : (
          <div className="flex aspect-video items-center justify-center bg-muted text-muted-foreground">
            <MapPin className="size-8" />
          </div>
        )}
        <CardHeader>
          <CardTitle className="text-base">{city.title}</CardTitle>
          {city.description && (
            <CardDescription className="line-clamp-2">{city.description}</CardDescription>
          )}
        </CardHeader>
      </Card>
    </Link>
  )
}

function CityGridSkeleton() {
  return (
    <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: 6 }, (_, index) => (
        <li key={index}>
          <Card className="pt-0">
            <Skeleton className="aspect-video w-full rounded-none" />
            <CardHeader>
              <Skeleton className="h-5 w-32" />
              <Skeleton className="h-4 w-full" />
            </CardHeader>
          </Card>
        </li>
      ))}
    </ul>
  )
}
