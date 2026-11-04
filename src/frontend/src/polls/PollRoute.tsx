import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { pollDaysLeft, pollProgress } from '@/polls/pollTimeline'

interface PollRouteProps {
  pollId: string | undefined
  createdAt: string | undefined
  endsAt: string | undefined
  onLine?: boolean
  className?: string
}

export function PollRoute({
  pollId,
  createdAt,
  endsAt,
  onLine = false,
  className,
}: Readonly<PollRouteProps>) {
  const { t, i18n } = useTranslation()
  const [now] = useState(() => Date.now())
  const progress = pollProgress(createdAt, endsAt, now)
  const daysLeft = pollDaysLeft(endsAt, now)
  const formatDate = (value: string | undefined) =>
    value
      ? new Intl.DateTimeFormat(i18n.language, { month: 'short', day: 'numeric' }).format(
          new Date(value),
        )
      : ''

  const todayLabel =
    daysLeft > 0 ? t('poll.route.daysLeft', { count: daysLeft }) : t('poll.route.closed')

  return (
    <div style={pollLineStyle(pollId)} className={cn('space-y-2', className)}>
      <div aria-hidden="true" className="relative h-4.5">
        <div
          className={cn(
            'absolute inset-x-2 top-1.75 h-1 rounded-full',
            onLine ? 'bg-line-foreground/35' : 'bg-muted',
          )}
        />
        <div
          className={cn(
            'absolute top-1.75 left-2 h-1 rounded-full',
            onLine ? 'bg-line-foreground' : 'bg-(--poll-line)',
          )}
          style={{ width: `calc((100% - 1rem) * ${progress})` }}
        />
        <span
          className={cn(
            'absolute top-0 left-0 size-4.5 rounded-full border-4',
            onLine ? 'border-line-foreground bg-(--poll-line)' : 'border-(--poll-line) bg-card',
          )}
        />
        <span
          className={cn(
            'absolute top-0 right-0 size-4.5 rounded-full border-4',
            onLine ? 'border-line-foreground/50 bg-(--poll-line)' : 'border-muted bg-card',
          )}
        />
        {progress > 0 && progress < 1 && (
          <span
            className={cn(
              'absolute top-0 size-4.5 rounded-full border-4 bg-primary',
              onLine ? 'border-line-foreground' : 'border-(--poll-line)',
            )}
            style={{ left: `calc((100% - 1.125rem) * ${progress})` }}
          />
        )}
      </div>
      <div
        className={cn(
          'grid grid-cols-3 gap-2 text-xs',
          onLine ? 'text-line-foreground/85' : 'text-muted-foreground',
        )}
      >
        <RouteStop date={formatDate(createdAt)} label={t('poll.route.opened')} onLine={onLine} />
        <span className="self-end text-center">{todayLabel}</span>
        <RouteStop
          date={formatDate(endsAt)}
          label={t('poll.route.closes')}
          onLine={onLine}
          className="text-right"
        />
      </div>
    </div>
  )
}

interface RouteStopProps {
  date: string
  label: string
  onLine: boolean
  className?: string
}

function RouteStop({ date, label, onLine, className }: Readonly<RouteStopProps>) {
  return (
    <span className={cn('grid', className)}>
      <span className={cn('font-semibold', onLine ? 'text-line-foreground' : 'text-foreground')}>
        {date}
      </span>
      {label}
    </span>
  )
}
