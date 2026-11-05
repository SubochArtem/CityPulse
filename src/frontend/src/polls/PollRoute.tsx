import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { TallyMarks } from '@/components/TallyMarks'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { pollDaysLeft, pollProgress } from '@/polls/pollTimeline'

const MAX_TALLY = 20

interface PollRouteProps {
  pollId: string | undefined
  createdAt: string | undefined
  endsAt: string | undefined
  className?: string
}

export function PollRoute({ pollId, createdAt, endsAt, className }: Readonly<PollRouteProps>) {
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
    <div style={pollLineStyle(pollId)} className={cn('space-y-3', className)}>
      <div aria-hidden="true" className="relative h-5">
        <div className="absolute inset-x-2 top-2 border-t-4 border-dashed border-foreground/25" />
        <div
          className="absolute top-2 left-2 h-1 bg-(--poll-line)"
          style={{ width: `calc((100% - 1rem) * ${progress})` }}
        />
        <span className="absolute top-0 left-0 size-5 border-4 border-(--poll-line) bg-card" />
        <span className="absolute top-0 right-0 size-5 border-4 border-foreground/30 bg-card" />
        {progress > 0 && progress < 1 && (
          <span
            className="absolute top-0 size-5 rotate-45 border-2 border-ink bg-fluo"
            style={{ left: `calc((100% - 1.25rem) * ${progress})` }}
          />
        )}
      </div>
      <div className="grid grid-cols-3 gap-2 font-label text-[0.65rem] tracking-[0.2em] text-muted-foreground uppercase">
        <RouteStop date={formatDate(createdAt)} label={t('poll.route.opened')} />
        <span className="flex flex-col items-center gap-1.5 self-end text-center">
          {daysLeft > 0 && daysLeft <= MAX_TALLY && (
            <TallyMarks count={daysLeft} className="text-foreground" />
          )}
          {todayLabel}
        </span>
        <RouteStop
          date={formatDate(endsAt)}
          label={t('poll.route.closes')}
          className="text-right"
        />
      </div>
    </div>
  )
}

interface RouteStopProps {
  date: string
  label: string
  className?: string
}

function RouteStop({ date, label, className }: Readonly<RouteStopProps>) {
  return (
    <span className={cn('grid gap-0.5', className)}>
      <span className="font-heading text-lg tracking-normal text-foreground normal-case">
        {date}
      </span>
      {label}
    </span>
  )
}
