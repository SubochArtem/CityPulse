import { useTranslation, Trans } from 'react-i18next'
import { Link } from 'react-router'
import { formatRelativeTime } from '@/lib/relativeTime'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { activityEventKey, isPollEvent } from './activityText'
import type { Activity } from './useActivityFeed'

interface ActivityItemProps {
  entry: Activity
  nickname: string | undefined
  cityName: string | undefined
  now: number
  fresh?: boolean
}

const linkClass =
  'font-semibold text-foreground underline-offset-4 outline-none hover:underline focus-visible:underline [overflow-wrap:anywhere]'

export function ActivityItem({
  entry,
  nickname,
  cityName,
  now,
  fresh = false,
}: Readonly<ActivityItemProps>) {
  const { t, i18n } = useTranslation()
  const eventKey = activityEventKey(entry)
  if (!eventKey) {
    return null
  }

  const pollId = entry.pollId ?? undefined
  const pollEvent = isPollEvent(entry)
  const showPoll = !pollEvent && Boolean(entry.pollTitle)
  const quote = (text: string) => t('activity.quoted', { text })
  const poll = entry.pollTitle ? quote(entry.pollTitle) : ''
  const idea = entry.ideaTitle ? quote(entry.ideaTitle) : t('activity.anIdea')
  const pollLink = entry.pollId ? (
    <Link to={`/polls/${entry.pollId}`} className={linkClass} />
  ) : (
    <span />
  )
  const ideaLink = entry.ideaId ? (
    <Link to={`/ideas/${entry.ideaId}`} className={linkClass} />
  ) : (
    <span />
  )

  return (
    <li
      style={pollLineStyle(pollId)}
      className="relative grid grid-cols-[1.75rem_1fr] gap-x-3 pb-6 last:pb-0"
    >
      <span className="flex justify-center pt-1">
        {pollEvent ? (
          <PollLineBadge pollId={pollId} className="size-5" />
        ) : (
          <span
            aria-hidden="true"
            className={cn(
              'mt-1 size-3 rounded-full bg-(--poll-line) ring-4 ring-background',
              fresh && 'motion-safe:animate-pulse',
            )}
          />
        )}
      </span>
      <div className="min-w-0 space-y-1">
        <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-0.5">
          <p className="min-w-0 text-sm text-muted-foreground sm:text-base">
            <Trans
              i18nKey={eventKey}
              values={{ poll, idea, actor: nickname ?? t('activity.someone') }}
              components={{
                poll: pollLink,
                idea: ideaLink,
                actor: <strong className="font-semibold text-foreground" />,
              }}
            />
          </p>
          {entry.occurredAt && (
            <time
              dateTime={entry.occurredAt}
              title={new Date(entry.occurredAt).toLocaleString(i18n.language)}
              className="shrink-0 text-xs text-muted-foreground tabular-nums"
            >
              {formatRelativeTime(entry.occurredAt, now, i18n.language)}
            </time>
          )}
        </div>
        {(showPoll || cityName) && (
          <p className="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-muted-foreground">
            {showPoll && (
              <span className="inline-flex min-w-0 items-center gap-1.5">
                <span
                  aria-hidden="true"
                  className="size-2 shrink-0 rounded-full bg-(--poll-line)"
                />
                <span className="[overflow-wrap:anywhere]">{t('activity.inPoll', { poll })}</span>
              </span>
            )}
            {cityName && (
              <span className="rounded-full bg-muted px-2 py-0.5 font-medium text-foreground">
                {cityName}
              </span>
            )}
          </p>
        )}
      </div>
    </li>
  )
}
