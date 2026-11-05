import { ArrowLeft, CalendarClock, CircleAlert, Lightbulb, Plus, Wallet } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { BoxAction } from '@/components/BoxAction'
import { HalftonePoster } from '@/components/HalftonePoster'
import { IconFact } from '@/components/IconFact'
import { ImageGallery } from '@/components/ImageGallery'
import { PillHeading } from '@/components/PillHeading'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useIdeaCreationAccess } from '@/ideas/useIdeaCreationAccess'
import { coverImage } from '@/lib/images'
import { tiltFor } from '@/lib/tilt'
import { PollRoute } from '@/polls/PollRoute'
import { pollLineStyle } from '@/polls/pollLine'
import { pollTypeKey } from '@/polls/pollTypes'
import { usePollWithIdeas } from '@/polls/usePollWithIdeas'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Poll = components['schemas']['PollWithIdeasDto']
type Idea = components['schemas']['IdeaDto']

const heroGrid = 'grid items-center gap-10 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)]'

export function PollPage() {
  const { t } = useTranslation()
  const { pollId = '' } = useParams()
  const { data: poll, error, isPending } = usePollWithIdeas(pollId)

  let content: ReactNode
  if (isPending) {
    content = <PollSkeleton />
  } else if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    content = (
      <div className="paper poster tape mx-auto max-w-md rotate-1 p-4 pt-6">
        <Alert variant="destructive" className="border-none bg-transparent">
          <CircleAlert />
          <AlertTitle>{notFound ? t('poll.notFound') : t('poll.loadFailed')}</AlertTitle>
        </Alert>
      </div>
    )
  } else {
    content = <PollDetails poll={poll} />
  }

  return (
    <div className="space-y-10">
      <BoxAction
        icon={ArrowLeft}
        label={poll?.cityId ? t('poll.back') : t('city.back')}
        to={poll?.cityId ? `/cities/${poll.cityId}` : '/cities'}
      />
      {content}
    </div>
  )
}

function PollDetails({ poll }: Readonly<{ poll: Poll }>) {
  const { t, i18n } = useTranslation()
  const typeKey = pollTypeKey(poll.type)
  const ideas = [...(poll.ideas ?? [])].sort((a, b) =>
    (b.createdAt ?? '').localeCompare(a.createdAt ?? ''),
  )
  const { data: nicknames } = usePublicProfiles(ideas.map((idea) => idea.userId))
  const hasGallery = (poll.images?.length ?? 0) > 1

  return (
    <div className="space-y-20">
      <section className={heroGrid}>
        <div className="paper poster tape relative rotate-[0.5deg] px-6 pt-10 pb-8 sm:px-10">
          <span
            aria-hidden="true"
            className="halftone pointer-events-none absolute right-0 bottom-0 h-2/3 w-1/2 text-riot/45 [mask-image:linear-gradient(315deg,black,transparent_75%)]"
          />
          <div className="relative flex flex-col items-start gap-5">
            <p className="flex flex-wrap items-center gap-2 font-label text-xs tracking-[0.25em] uppercase">
              <span
                aria-hidden="true"
                style={pollLineStyle(poll.id)}
                className="h-5 w-10 -rotate-2 bg-(--poll-line)"
              />
              {typeKey && <span className="bg-ink px-2 py-0.5 text-paper">{t(typeKey)}</span>}
            </p>
            <h1 className="text-4xl leading-[1.05] text-foreground [overflow-wrap:anywhere] sm:text-6xl">
              {poll.title}
            </h1>
            {poll.description && (
              <p className="max-w-lg whitespace-pre-line text-muted-foreground">
                {poll.description}
              </p>
            )}
          </div>
        </div>
        <div className="group mx-auto w-full max-w-sm">
          <HalftonePoster
            seed={poll.id ?? poll.title ?? ''}
            title={poll.title}
            photoUrl={coverImage(poll.images)?.url}
          />
        </div>
      </section>

      <section style={pollLineStyle(poll.id)} className="paper poster -rotate-[0.3deg] pb-10">
        <div
          aria-hidden="true"
          className="h-4 bg-(--poll-line) [mask:radial-gradient(circle_at_10px_100%,transparent_6px,black_6.5px)_0_0/20px_100%]"
        />
        <div className="space-y-10 px-4 pt-8 sm:px-10">
          <dl className="flex flex-wrap justify-center gap-x-14 gap-y-6">
            {poll.endsAt && (
              <IconFact
                icon={CalendarClock}
                label={t('home.facts.ends')}
                value={new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
                  new Date(poll.endsAt),
                )}
              />
            )}
            {poll.budgetAmount ? (
              <IconFact
                icon={Wallet}
                label={t('home.facts.budget')}
                value={new Intl.NumberFormat(i18n.language).format(poll.budgetAmount)}
              />
            ) : null}
            <IconFact
              icon={Lightbulb}
              label={t('poll.ideas')}
              value={new Intl.NumberFormat(i18n.language).format(ideas.length)}
            />
          </dl>
          <PollRoute
            pollId={poll.id}
            createdAt={poll.createdAt}
            endsAt={poll.endsAt}
            className="mx-auto max-w-3xl"
          />
        </div>
      </section>

      {hasGallery && (
        <section aria-labelledby="poll-photos-title" className="space-y-6">
          <PillHeading id="poll-photos-title">{t('city.photos')}</PillHeading>
          <ImageGallery key={poll.id} images={poll.images} alt={poll.title ?? ''} />
        </section>
      )}

      <section aria-labelledby="poll-ideas-title" className="space-y-8">
        <div className="flex flex-col items-center gap-6">
          <PillHeading id="poll-ideas-title">{t('poll.ideas')}</PillHeading>
          <SuggestIdeaAction poll={poll} />
        </div>
        {ideas.length ? (
          <ul className="grid items-start gap-8 sm:grid-cols-2 lg:grid-cols-3">
            {ideas.map((idea) => (
              <li key={idea.id}>
                <IdeaCard
                  idea={idea}
                  author={idea.userId ? nicknames?.get(idea.userId) : undefined}
                />
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-center text-muted-foreground">{t('poll.noIdeas')}</p>
        )}
      </section>
    </div>
  )
}

function SuggestIdeaAction({ poll }: Readonly<{ poll: Poll }>) {
  const { t } = useTranslation()
  const access = useIdeaCreationAccess(poll)

  if (access !== 'allowed' && access !== 'guest') {
    return null
  }

  return <BoxAction icon={Plus} label={t('poll.suggestIdea')} to={`/polls/${poll.id}/ideas/new`} />
}

function IdeaCard({ idea, author }: Readonly<{ idea: Idea; author: string | undefined }>) {
  const { t, i18n } = useTranslation()
  const seed = idea.id ?? idea.title ?? ''
  const cover = coverImage(idea.images)
  const createdAt = idea.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(idea.createdAt),
      )
    : null

  return (
    <Link
      to={`/ideas/${idea.id}`}
      style={{ rotate: `${tiltFor(seed)}deg` }}
      className="group paper poster flex h-full flex-col p-3 pb-0 outline-none transition-[rotate,translate] duration-300 hover:-translate-y-1 hover:rotate-0! focus-visible:rotate-0! focus-visible:ring-3 focus-visible:ring-ring/50 motion-reduce:transition-none"
    >
      {cover?.url ? (
        <HalftonePoster
          seed={seed}
          photoUrl={cover.url}
          framed={false}
          className="[&>div]:aspect-video"
        />
      ) : (
        <span
          aria-hidden="true"
          className="halftone h-14 text-riot/30 [mask-image:linear-gradient(100deg,black,transparent_85%)]"
        />
      )}
      <div className="flex flex-1 flex-col items-start gap-2 pt-4 pb-4">
        <IdeaApprovalBadge
          status={idea.approvalStatus}
          className="h-auto -rotate-2 rounded-none border-2 border-current px-1.5 font-label text-[0.65rem] tracking-[0.2em] uppercase"
        />
        <h3 className="text-2xl leading-tight text-foreground [overflow-wrap:anywhere] group-hover:text-coral">
          {idea.title}
        </h3>
        {idea.description && (
          <p className="line-clamp-3 text-sm text-muted-foreground">{idea.description}</p>
        )}
      </div>
      {(author || createdAt) && (
        <p className="-mx-3 mt-auto border-t-2 border-dashed border-foreground/60 px-3 py-2.5 font-label text-[0.65rem] tracking-[0.2em] text-muted-foreground uppercase">
          {[author, createdAt && t('idea.createdAt', { date: createdAt })]
            .filter(Boolean)
            .join(' · ')}
        </p>
      )}
    </Link>
  )
}

function PollSkeleton() {
  return (
    <div className={heroGrid}>
      <div className="paper poster flex flex-col items-start gap-5 px-6 pt-10 pb-8 sm:px-10">
        <Skeleton className="h-5 w-40" />
        <Skeleton className="h-28 w-full max-w-lg" />
        <Skeleton className="h-12 w-full max-w-md" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-sm" />
    </div>
  )
}
