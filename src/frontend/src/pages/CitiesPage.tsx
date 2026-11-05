import { CircleAlert, Search } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { CityCard } from '@/cities/CityCard'
import { CITIES_PAGE_SIZE, useCities } from '@/cities/useCities'
import { Pagination } from '@/components/Pagination'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { useListSearchParams } from '@/lib/useListSearchParams'

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
          className="grid gap-y-10 transition-opacity sm:grid-cols-2 lg:grid-cols-4 data-[stale=true]:opacity-60"
          data-stale={isPlaceholderData}
        >
          {data.items?.map((city) => (
            <li key={city.id} className="grid">
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
        <h1 className="text-5xl font-medium text-heading">{t('cities.title')}</h1>
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

function CityGridSkeleton() {
  return (
    <ul className="grid gap-y-10 sm:grid-cols-2 lg:grid-cols-4">
      {Array.from({ length: 4 }, (_, index) => (
        <li key={index} className="flex flex-col items-center gap-3 px-6">
          <Skeleton className="aspect-square w-full" />
          <Skeleton className="h-7 w-28" />
          <Skeleton className="h-4 w-40" />
        </li>
      ))}
    </ul>
  )
}
