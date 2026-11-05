import { ArrowLeft, ArrowRight, CircleAlert, Clock } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { IsoCityTile } from '@/cities/IsoCityTile'
import { BoxAction } from '@/components/BoxAction'
import { ImageGallery } from '@/components/ImageGallery'
import { PillHeading } from '@/components/PillHeading'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { IdeaFeedback } from '@/feedback/IdeaFeedback'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useIdeaWithPoll } from '@/ideas/useIdeaWithPoll'
import { coverImage } from '@/lib/images'
import { useNow } from '@/lib/useNow'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { pollDaysLeft } from '@/polls/pollTimeline'
import { pollTypeKey } from '@/polls/pollTypes'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Idea = components['schemas']['IdeaWithPollDto']
type Poll = components['schemas']['PollDto']

export function IdeaPage() {
  const { t } = useTranslation()
  const { ideaId = '' } = useParams()
  const { data: idea, error, isPending } = useIdeaWithPoll(ideaId)

  let content: ReactNode
  if (isPending) {
    content = <IdeaSkeleton />
  } else if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    content = (
      <Alert variant="destructive" className="mx-auto max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('idea.notFound') : t('idea.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else {
    content = <IdeaDetails idea={idea} />
  }

  return (
    <div className="space-y-10">
      <BoxAction
        icon={ArrowLeft}
        label={idea?.pollId ? t('idea.back') : t('city.back')}
        to={idea?.pollId ? `/polls/${idea.pollId}` : '/cities'}
      />
      {content}
    </div>
  )
}

function IdeaDetails({ idea }: Readonly<{ idea: Idea }>) {
  const { t } = useTranslation()
  const hasGallery = (idea.images?.length ?? 0) > 1

  return (
    <div className="space-y-20">
      <section className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
        <div className="flex flex-col items-center gap-5 text-center">
          <span className="inline-flex items-center gap-2.5">
            <PollLineBadge pollId={idea.pollId} />
            <IdeaApprovalBadge status={idea.approvalStatus} />
          </span>
          <h1 className="max-w-lg text-4xl leading-[1.05] font-medium text-heading [overflow-wrap:anywhere] sm:text-6xl">
            {idea.title}
          </h1>
          <span aria-hidden="true" className="size-2.5 rotate-45 border border-heading" />
          {idea.description && (
            <p className="max-w-md whitespace-pre-line text-muted-foreground">{idea.description}</p>
          )}
          <IdeaMeta idea={idea} />
        </div>
        <div className="group mx-auto w-full max-w-xl">
          <IsoCityTile seed={idea.id ?? idea.title ?? ''} photoUrl={coverImage(idea.images)?.url} />
        </div>
      </section>

      <IdeaPoll poll={idea.poll} />

      {hasGallery && (
        <section aria-labelledby="idea-photos-title" className="space-y-6">
          <PillHeading id="idea-photos-title">{t('city.photos')}</PillHeading>
          <ImageGallery key={idea.id} images={idea.images} alt={idea.title ?? ''} />
        </section>
      )}

      {idea.id && <IdeaFeedback ideaId={idea.id} cityId={idea.poll.cityId} />}
    </div>
  )
}

function IdeaMeta({ idea }: Readonly<{ idea: Idea }>) {
  const { t, i18n } = useTranslation()
  const { data: nicknames, isLoading } = usePublicProfiles([idea.userId])
  const dateFormat = new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' })

  if (isLoading) {
    return <Skeleton className="h-4 w-48" />
  }

  const nickname = idea.userId ? nicknames?.get(idea.userId) : undefined
  const parts = [
    nickname ? t('idea.author', { nickname }) : t('idea.unknownAuthor'),
    idea.createdAt && t('idea.createdAt', { date: dateFormat.format(new Date(idea.createdAt)) }),
    idea.updatedAt && t('idea.updatedAt', { date: dateFormat.format(new Date(idea.updatedAt)) }),
  ]

  return (
    <p className="font-heading text-xs tracking-[0.2em] text-muted-foreground uppercase">
      {parts.filter(Boolean).join(' · ')}
    </p>
  )
}

function IdeaPoll({ poll }: Readonly<{ poll: Poll }>) {
  const { t } = useTranslation()
  const now = useNow()
  const typeKey = pollTypeKey(poll.type)
  const daysLeft = pollDaysLeft(poll.endsAt, now)

  return (
    <section
      aria-labelledby="idea-poll-title"
      className="flex flex-col items-center gap-6 border bg-card px-4 py-8 text-center sm:px-10 lg:flex-row lg:justify-between lg:text-left"
    >
      <div className="flex min-w-0 flex-col items-center gap-2 lg:items-start">
        <span className="inline-flex items-center gap-2.5 font-heading text-xs tracking-[0.25em] text-coral uppercase">
          <PollLineBadge pollId={poll.id} />
          {t('idea.poll')}
          {typeKey && <span aria-hidden="true">·</span>}
          {typeKey && t(typeKey)}
        </span>
        <h2
          id="idea-poll-title"
          className="text-3xl font-normal text-heading [overflow-wrap:anywhere]"
        >
          {poll.title}
        </h2>
        {poll.endsAt && (
          <span className="inline-flex items-center gap-2 font-heading text-lg text-coral">
            <Clock className="size-4" />
            {daysLeft > 0 ? t('poll.route.daysLeft', { count: daysLeft }) : t('poll.route.closed')}
          </span>
        )}
      </div>
      <BoxAction
        icon={ArrowRight}
        label={t('poll.open')}
        to={`/polls/${poll.id}`}
        className="shrink-0"
      />
    </section>
  )
}

function IdeaSkeleton() {
  return (
    <div className="grid items-center gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.3fr)]">
      <div className="flex flex-col items-center gap-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-28 w-80" />
        <Skeleton className="h-12 w-80" />
        <Skeleton className="h-4 w-48" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-xl" />
    </div>
  )
}
