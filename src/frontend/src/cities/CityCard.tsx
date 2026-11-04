import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { coverImage } from '@/lib/images'
import { cn } from '@/lib/utils'
import { IsoCityTile } from './IsoCityTile'

type City = components['schemas']['CityDto']

interface CityCardProps {
  city: City
  highlighted?: boolean
}

export function CityCard({ city, highlighted = false }: Readonly<CityCardProps>) {
  const { t } = useTranslation()

  return (
    <Link
      to={`/cities/${city.id}`}
      className={cn(
        'group flex h-full flex-col items-center pb-2 text-center outline-none focus-visible:ring-3 focus-visible:ring-ring/50',
        highlighted && 'border bg-card pb-5',
      )}
    >
      <IsoCityTile
        seed={city.id ?? city.title ?? ''}
        photoUrl={coverImage(city.images)?.url}
        className="w-full max-w-64 px-6 transition-transform duration-300 group-hover:-translate-y-1.5 motion-reduce:transition-none"
      />
      <TileMarker />
      {highlighted && (
        <span className="mt-3 font-heading text-xs tracking-[0.2em] text-muted-foreground uppercase">
          {t('home.yourCity')}
        </span>
      )}
      <h2
        className={cn(
          'px-3 text-2xl font-medium text-coral [overflow-wrap:anywhere] group-hover:underline group-hover:underline-offset-4',
          highlighted ? 'mt-1' : 'mt-3',
        )}
      >
        {city.title}
      </h2>
      {city.description && (
        <p className="mt-1 line-clamp-2 px-4 text-sm text-muted-foreground">{city.description}</p>
      )}
      {highlighted && (
        <span className="mt-4 inline-flex min-h-10 items-stretch border border-heading">
          <span className="grid w-10 place-items-center border-r border-heading bg-primary text-primary-foreground">
            <ArrowRight className="size-4" />
          </span>
          <span className="inline-flex items-center px-4 font-heading text-xs tracking-[0.2em] uppercase">
            {t('home.openCity')}
          </span>
        </span>
      )}
    </Link>
  )
}

function TileMarker() {
  return (
    <div aria-hidden="true" className="relative flex w-full justify-center">
      <span className="absolute inset-x-0 top-1/2 h-px bg-border" />
      <span className="relative size-3 rounded-full border border-muted-foreground/60 bg-background" />
    </div>
  )
}
