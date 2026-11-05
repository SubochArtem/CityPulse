import { ArrowLeft, CalendarClock, CircleAlert, Lightbulb, Plus, Wallet } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { IsoCityTile } from '@/cities/IsoCityTile'
import { BoxAction } from '@/components/BoxAction'
import { IconFact } from '@/components/IconFact'
import { ImageGallery } from '@/components/ImageGallery'
import { PillHeading } from '@/components/PillHeading'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useIdeaCreationAccess } from '@/ideas/useIdeaCreationAccess'
import { coverImage } from '@/lib/images'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { PollRoute } from '@/polls/PollRoute'
import { pollTypeKey } from '@/polls/pollTypes'
import { usePollWithIdeas } from '@/polls/usePollWithIdeas'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Poll = components['schemas']['PollWithIdeasDto']
type Idea = components['schemas']['IdeaDto']

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
      <Alert variant="destructive" className="mx-auto max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('poll.notFound') : t('poll.loadFailed')}</AlertTitle>
      </Alert>
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
      <section className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
        <div className="flex flex-col items-center gap-5 text-center">
          <span className="inline-flex items-center gap-2.5 font-heading text-xs tracking-[0.25em] text-coral uppercase">
            <PollLineBadge pollId={poll.id} />
            {typeKey && t(typeKey)}
          </span>
          <h1 className="max-w-lg text-4xl leading-[1.05] font-medium text-heading [overflow-wrap:anywhere] sm:text-6xl">
            {poll.title}
          </h1>
          <span aria-hidden="true" className="size-2.5 rotate-45 border border-heading" />
          {poll.description && (
            <p className="max-w-md whitespace-pre-line text-muted-foreground">{poll.description}</p>
          )}
        </div>
        <div className="group mx-auto w-full max-w-xl">
          <IsoCityTile seed={poll.id ?? poll.title ?? ''} photoUrl={coverImage(poll.images)?.url} />
        </div>
      </section>

      <section className="space-y-10 border bg-card px-4 py-10 sm:px-10">
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
          <ul className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {ideas.map((idea) => (
              <li key={idea.id} className="grid">
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
  const cover = coverImage(idea.images)
  const createdAt = idea.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(idea.createdAt),
      )
    : null

  return (
    <Link
      to={`/ideas/${idea.id}`}
      className="group flex h-full flex-col border bg-card p-1.5 transition-colors outline-none hover:border-heading focus-visible:ring-3 focus-visible:ring-ring/50"
    >
      {cover?.url ? (
        <img src={cover.url} alt="" loading="lazy" className="aspect-video w-full object-cover" />
      ) : (
        <div className="grid aspect-video place-items-center bg-secondary text-heading">
          <Lightbulb className="size-8" />
        </div>
      )}
      <div className="flex flex-1 flex-col items-start gap-2 p-4">
        <IdeaApprovalBadge status={idea.approvalStatus} />
        <h3 className="text-2xl font-normal text-coral [overflow-wrap:anywhere] group-hover:underline group-hover:underline-offset-4">
          {idea.title}
        </h3>
        {idea.description && (
          <p className="line-clamp-3 text-sm text-muted-foreground">{idea.description}</p>
        )}
        {(author || createdAt) && (
          <p className="mt-auto pt-2 font-heading text-[0.65rem] tracking-[0.2em] text-muted-foreground uppercase">
            {[author, createdAt && t('idea.createdAt', { date: createdAt })]
              .filter(Boolean)
              .join(' · ')}
          </p>
        )}
      </div>
    </Link>
  )
}

function PollSkeleton() {
  return (
    <div className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
      <div className="flex flex-col items-center gap-4">
        <Skeleton className="h-4 w-32" />
        <Skeleton className="h-28 w-80" />
        <Skeleton className="h-12 w-80" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-xl" />
    </div>
  )
}
