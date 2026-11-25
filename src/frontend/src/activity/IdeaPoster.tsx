import type { ReactNode } from 'react'
import { Heart, MessageSquare } from 'lucide-react'
import { Trans, useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { useLatestComment, useLikeCount } from '@/feedback/useIdeaFeedback'
import { useIdeaWithPoll } from '@/ideas/useIdeaWithPoll'
import { coverImage } from '@/lib/images'
import { formatRelativeTime } from '@/lib/relativeTime'
import { tiltFor } from '@/lib/tilt'
import { useInView } from '@/lib/useInView'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { usePublicProfiles } from '@/users/usePublicProfiles'
import { ActivityKind } from './activityText'
import type { WallItem } from './activityWall'
import type { Activity } from './useActivityFeed'

type IdeaItem = Extract<WallItem, { type: 'idea' }>
type Stamp = 'new' | 'approved' | 'rejected'

interface IdeaPosterProps {
  item: IdeaItem
  nicknames: Map<string, string> | undefined
  cityName: string | undefined
  now: number
  fresh: boolean
  canRead: boolean
}

const stampClass: Record<Stamp, string> = {
  new: 'bg-fluo text-ink',
  approved: 'bg-paper/85 text-ink',
  rejected: 'bg-paper/85 text-riot',
}

export function IdeaPoster({
  item,
  nicknames,
  cityName,
  now,
  fresh,
  canRead,
}: Readonly<IdeaPosterProps>) {
  const { t } = useTranslation()
  const [ref, inView] = useInView<HTMLElement>()
  const { data: idea } = useIdeaWithPoll(item.ideaId, inView)
  const likes = useLikeCount(item.ideaId, canRead && inView)
  const comments = useLatestComment(item.ideaId, canRead && inView)
  const latest = comments.data?.latest
  const authorId = authorOf(item.events) ?? idea?.userId ?? undefined
  const commenterId = latest?.userId ?? undefined
  const missing = [authorId, commenterId].filter((id) => id && !nicknames?.has(id))
  const extra = usePublicProfiles(inView ? missing : [])
  const nameOf = (id: string | undefined) =>
    id ? (nicknames?.get(id) ?? extra.data?.get(id)) : undefined

  const first = item.events[0]
  const title = idea?.title ?? first.ideaTitle ?? ''
  const photo = coverImage(idea?.images)?.url
  const author = nameOf(authorId)
  const stamp = stampFor(item.events)
  const tilt = tiltFor(item.ideaId) / 2
  const ideaPath = `/ideas/${item.ideaId}`

  return (
    <article
      ref={ref}
      style={{ ...pollLineStyle(item.pollId), rotate: `${tilt}deg` }}
      className="flex flex-col"
    >
      <div
        className={cn(
          'paper poster flex flex-col gap-3 border-t-[6px] border-(--poll-line) p-3 pb-4',
          tilt < 0 && 'tape',
          fresh && 'ring-4 ring-fluo',
        )}
      >
        <Link
          to={ideaPath}
          className="relative block outline-none focus-visible:ring-4 focus-visible:ring-fluo"
          tabIndex={-1}
          aria-hidden="true"
        >
          {photo ? (
            <span className="relative block overflow-hidden bg-ink/5">
              <img src={photo} alt="" loading="lazy" className="max-h-96 w-full object-cover" />
              <span className="halftone absolute inset-0 text-riot/50 [mask-image:linear-gradient(135deg,transparent_55%,#000)]" />
            </span>
          ) : (
            <span className="halftone grid aspect-[4/3] place-items-center bg-fluo p-6 text-center text-(--poll-line)/60">
              <span className="font-heading text-3xl leading-tight text-balance text-ink [overflow-wrap:anywhere]">
                {title}
              </span>
            </span>
          )}
          {stamp && (
            <span
              className={cn(
                'absolute top-2 right-2 -rotate-6 border-[3px] border-current px-2 py-0.5 font-heading text-base uppercase',
                stampClass[stamp],
              )}
            >
              {t(`activity.stamp.${stamp}`)}
            </span>
          )}
        </Link>

        <p className="flex min-w-0 items-center gap-2">
          <span
            aria-hidden="true"
            className="grid size-7 shrink-0 -rotate-6 place-items-center bg-(--poll-line) font-heading text-base text-paper shadow-[2px_2px_0_var(--ink)]"
          >
            {(author ?? '?').slice(0, 1).toUpperCase()}
          </span>
          <span className="truncate font-label text-[0.7rem] tracking-[0.12em] text-muted-foreground uppercase">
            {[cityName, author && t('activity.byAuthor', { author })].filter(Boolean).join(' · ')}
          </span>
        </p>

        <h3 className="font-heading text-2xl leading-tight text-balance [overflow-wrap:anywhere]">
          <Link to={ideaPath} className="outline-none hover:underline focus-visible:underline">
            {title}
          </Link>
        </h3>
        {idea?.description && (
          <p className="line-clamp-2 text-sm text-muted-foreground [overflow-wrap:anywhere]">
            {idea.description}
          </p>
        )}

        <Trail events={item.events} nicknames={nicknames} now={now} />

        <div className="flex flex-wrap items-center gap-3">
          {canRead && (
            <>
              <Counter
                icon={<Heart aria-hidden="true" />}
                label={t('activity.likes', { count: likes.data ?? 0 })}
                value={likes.data}
                highlighted={(likes.data ?? 0) > 0}
              />
              <Counter
                icon={<MessageSquare aria-hidden="true" />}
                label={t('activity.comments', { count: comments.data?.count ?? 0 })}
                value={comments.data?.count}
              />
            </>
          )}
          <Link
            to={ideaPath}
            className="ml-auto font-label text-xs tracking-[0.14em] uppercase underline decoration-riot decoration-2 underline-offset-4 outline-none hover:decoration-4 focus-visible:decoration-4"
          >
            {t('activity.openIdea')}
          </Link>
        </div>
      </div>

      {canRead && latest?.text && (
        <figure className="mx-2.5 -mt-0.5 flex -rotate-1 flex-col gap-1.5 bg-ink px-3.5 pt-3 pb-5 text-paper shadow-[0_8px_16px_-10px_oklch(0_0_0/0.7)] [clip-path:polygon(0_0,100%_0,100%_88%,94%_100%,86%_90%,77%_100%,66%_91%,55%_100%,44%_90%,33%_100%,22%_91%,11%_100%,0_90%)]">
          <figcaption className="font-label text-[0.65rem] tracking-[0.14em] text-paper/70 uppercase">
            {t('activity.says', {
              author: nameOf(commenterId) ?? t('activity.someone'),
            })}
          </figcaption>
          <blockquote className="line-clamp-4 font-heading text-lg leading-snug [overflow-wrap:anywhere]">
            {t('activity.quoted', { text: latest.text })}
          </blockquote>
          {(comments.data?.count ?? 0) > 1 && (
            <Link
              to={ideaPath}
              className="font-label text-[0.65rem] tracking-[0.14em] text-fluo uppercase outline-none hover:underline focus-visible:underline"
            >
              {t('activity.moreComments', { count: (comments.data?.count ?? 1) - 1 })}
            </Link>
          )}
        </figure>
      )}
    </article>
  )
}

function authorOf(events: Activity[]) {
  const authored = events.find(
    (event) => event.kind === ActivityKind.IdeaProposed || event.kind === ActivityKind.IdeaReviewed,
  )
  return authored?.userId ?? undefined
}

function stampFor(events: Activity[]): Stamp | undefined {
  const review = events.find((event) => event.kind === ActivityKind.IdeaReviewed)
  if (review) {
    return review.status === 'Rejected' ? 'rejected' : 'approved'
  }
  if (events.some((event) => event.kind === ActivityKind.IdeaProposed)) {
    return 'new'
  }
  return undefined
}

interface TrailProps {
  events: Activity[]
  nicknames: Map<string, string> | undefined
  now: number
}

function Trail({ events, nicknames, now }: Readonly<TrailProps>) {
  const { t, i18n } = useTranslation()
  const name = (userId: string | null | undefined) =>
    (userId && nicknames?.get(userId)) || t('activity.someone')

  const comments = events.filter((event) => event.kind === ActivityKind.IdeaCommented)
  const review = events.find((event) => event.kind === ActivityKind.IdeaReviewed)
  const proposal = events.find((event) => event.kind === ActivityKind.IdeaProposed)
  const newest = events[0]
  const rows: { key: string; event: Activity; text: ReactNode }[] = []

  if (comments.length > 0) {
    const actors = [...new Set(comments.map((event) => name(event.userId)))].slice(0, 2).join(', ')
    rows.push({
      key: 'comments',
      event: comments[0],
      text: (
        <Trans
          i18nKey="activity.trail.commented"
          count={comments.length}
          values={{ actors }}
          components={{ actor: <strong className="font-semibold text-foreground" /> }}
        />
      ),
    })
  }
  if (review) {
    rows.push({
      key: 'review',
      event: review,
      text:
        review.status === 'Rejected' ? t('activity.trail.rejected') : t('activity.trail.approved'),
    })
  }
  if (proposal) {
    rows.push({
      key: 'proposal',
      event: proposal,
      text: (
        <Trans
          i18nKey="activity.trail.proposed"
          values={{ actor: name(proposal.userId) }}
          components={{ actor: <strong className="font-semibold text-foreground" /> }}
        />
      ),
    })
  }

  return (
    <ul className="flex flex-col gap-1.5">
      {rows.map(({ key, event, text }) => (
        <li
          key={key}
          className="flex min-w-0 items-baseline gap-2 text-[0.8rem] text-muted-foreground"
        >
          <span
            aria-hidden="true"
            className={cn(
              'size-2.5 shrink-0 translate-y-px border-2 border-ink',
              event === newest || (key === 'comments' && newest.kind === ActivityKind.IdeaCommented)
                ? 'bg-fluo'
                : 'bg-paper',
            )}
          />
          <span className="min-w-0 [overflow-wrap:anywhere]">{text}</span>
          {event.occurredAt && (
            <time
              dateTime={event.occurredAt}
              className="ml-auto shrink-0 font-label text-[0.65rem] tabular-nums"
            >
              {formatRelativeTime(event.occurredAt, now, i18n.language)}
            </time>
          )}
        </li>
      ))}
    </ul>
  )
}

interface CounterProps {
  icon: ReactNode
  label: string
  value: number | undefined
  highlighted?: boolean
}

function Counter({ icon, label, value, highlighted = false }: Readonly<CounterProps>) {
  return (
    <span
      title={label}
      className={cn(
        'inline-flex items-center gap-1.5 border-2 border-ink px-2 py-0.5 font-label text-xs tabular-nums shadow-[2px_2px_0_var(--ink)] [&_svg]:size-3.5',
        highlighted ? 'bg-fluo' : 'bg-paper',
      )}
    >
      {icon}
      <span aria-hidden="true">{value ?? '·'}</span>
      <span className="sr-only">{label}</span>
    </span>
  )
}
