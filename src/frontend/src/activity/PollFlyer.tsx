import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { formatRelativeTime } from '@/lib/relativeTime'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { ActivityKind } from './activityText'
import type { Activity } from './useActivityFeed'

type FlyerLabel =
  | 'activity.flyer.opened'
  | 'activity.flyer.finished'
  | 'activity.flyer.cancelled'
  | 'activity.flyer.closed'

const closedLabels: Record<string, FlyerLabel> = {
  Finished: 'activity.flyer.finished',
  Cancelled: 'activity.flyer.cancelled',
  Inactive: 'activity.flyer.closed',
}

interface PollFlyerProps {
  entry: Activity
  cityName: string | undefined
  now: number
  fresh: boolean
}

export function PollFlyer({ entry, cityName, now, fresh }: Readonly<PollFlyerProps>) {
  const { t, i18n } = useTranslation()
  const opened = entry.kind === ActivityKind.PollOpened
  const labelKey: FlyerLabel | undefined = opened
    ? 'activity.flyer.opened'
    : closedLabels[entry.status ?? '']
  if (!labelKey) {
    return null
  }

  const meta = [t(labelKey), cityName].filter(Boolean).join(' · ')

  return (
    <article
      style={pollLineStyle(entry.pollId ?? undefined)}
      className={cn(
        'paper poster flex rotate-[1.2deg] flex-col gap-2 border-l-8 border-(--poll-line) p-4',
        opened
          ? 'bg-[color-mix(in_oklch,var(--fluo)_60%,var(--paper))]'
          : 'bg-[color-mix(in_oklch,var(--fluo)_22%,var(--paper))]',
        fresh && 'ring-4 ring-fluo',
      )}
    >
      <p className="flex flex-wrap items-baseline justify-between gap-x-3 font-label text-[0.7rem] tracking-[0.14em] uppercase">
        <span>{meta}</span>
        {entry.occurredAt && (
          <time dateTime={entry.occurredAt} className="text-muted-foreground tabular-nums">
            {formatRelativeTime(entry.occurredAt, now, i18n.language)}
          </time>
        )}
      </p>
      <h3 className="font-heading text-2xl leading-tight text-balance [overflow-wrap:anywhere]">
        {entry.pollTitle}
      </h3>
      {entry.pollId && (
        <Link
          to={`/polls/${entry.pollId}`}
          className="self-start font-label text-xs tracking-[0.14em] uppercase underline decoration-riot decoration-2 underline-offset-4 outline-none hover:decoration-4 focus-visible:decoration-4"
        >
          {t('activity.openPoll')}
        </Link>
      )}
    </article>
  )
}
