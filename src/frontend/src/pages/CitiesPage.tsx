import { CircleAlert, Search } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { CITIES_PAGE_SIZE, useCities } from '@/cities/useCities'
import { Pagination } from '@/components/Pagination'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { useListSearchParams } from '@/lib/useListSearchParams'
import { cn } from '@/lib/utils'

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
          className="grid gap-3 transition-opacity sm:grid-cols-2 lg:grid-cols-3 data-[stale=true]:opacity-60"
          data-stale={isPlaceholderData}
        >
          {data.items?.map((city, index) => (
            <li key={city.id} className={cn('grid', index === 0 && 'sm:col-span-2 lg:row-span-2')}>
              <CityTile city={city} featured={index === 0} />
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
        <h1 className="text-4xl font-extrabold tracking-tight">{t('cities.title')}</h1>
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

function CityTile({ city, featured }: Readonly<{ city: City; featured: boolean }>) {
  const cover = coverImage(city.images)

  return (
    <Link
      to={`/cities/${city.id}`}
      className={cn(
        'group relative flex min-h-48 flex-col justify-end overflow-hidden rounded-xl bg-navy p-5 text-navy-foreground outline-none focus-visible:ring-3 focus-visible:ring-ring/50',
        featured && 'sm:col-span-2 lg:row-span-2 lg:min-h-96',
      )}
    >
      {cover?.url && (
        <>
          <img
            src={cover.url}
            alt=""
            loading="lazy"
            className="absolute inset-0 size-full object-cover transition-transform duration-300 group-hover:scale-105 motion-reduce:transition-none"
          />
          <div className="absolute inset-0 bg-linear-to-t from-navy via-navy/60 to-navy/10" />
        </>
      )}
      <div className="relative space-y-2">
        <h2
          className={cn(
            'font-extrabold tracking-tight [overflow-wrap:anywhere]',
            featured ? 'text-3xl lg:text-5xl' : 'text-2xl',
          )}
        >
          {city.title}
        </h2>
        {city.description && (
          <p className="line-clamp-2 text-sm text-navy-foreground/80">{city.description}</p>
        )}
      </div>
    </Link>
  )
}

function CityGridSkeleton() {
  return (
    <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: 6 }, (_, index) => (
        <li key={index} className={cn(index === 0 && 'sm:col-span-2 lg:row-span-2')}>
          <Skeleton className="size-full min-h-48 rounded-xl" />
        </li>
      ))}
    </ul>
  )
}
