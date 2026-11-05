import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { BoxAction } from '@/components/BoxAction'
import { HalftonePoster } from '@/components/HalftonePoster'
import { coverImage } from '@/lib/images'
import { tiltFor } from '@/lib/tilt'
import { cn } from '@/lib/utils'

type City = components['schemas']['CityDto']

interface CityCardProps {
  city: City
  highlighted?: boolean
}

export function CityCard({ city, highlighted = false }: Readonly<CityCardProps>) {
  const { t } = useTranslation()
  const seed = city.id ?? city.title ?? ''

  return (
    <Link
      to={`/cities/${city.id}`}
      style={{ rotate: `${tiltFor(seed)}deg` }}
      className="group paper poster tape flex h-full flex-col p-3 pb-4 outline-none transition-[rotate,translate] duration-300 hover:-translate-y-1 hover:rotate-0! focus-visible:rotate-0! focus-visible:ring-3 focus-visible:ring-ring/50 motion-reduce:transition-none"
    >
      <HalftonePoster
        seed={seed}
        title={city.title}
        photoUrl={coverImage(city.images)?.url}
        framed={false}
      />
      {highlighted && (
        <span className="mt-3 self-start bg-fluo px-2 py-0.5 font-label text-xs tracking-[0.2em] text-ink uppercase">
          {t('home.yourCity')}
        </span>
      )}
      <h2
        className={cn(
          'text-3xl leading-none text-foreground [overflow-wrap:anywhere] group-hover:text-coral',
          highlighted ? 'mt-2' : 'mt-3',
        )}
      >
        {city.title}
      </h2>
      {city.description && (
        <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">{city.description}</p>
      )}
      {highlighted && <BoxAction icon={ArrowRight} label={t('home.openCity')} className="mt-4" />}
    </Link>
  )
}
