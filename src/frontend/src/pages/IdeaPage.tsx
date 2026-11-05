import { ArrowLeft, ArrowRight, CircleAlert, Clock } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { BoxAction } from '@/components/BoxAction'
import { HalftonePoster } from '@/components/HalftonePoster'
import { ImageGallery } from '@/components/ImageGallery'
import { PillHeading } from '@/components/PillHeading'
import { TallyMarks } from '@/components/TallyMarks'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { IdeaFeedback } from '@/feedback/IdeaFeedback'
import { approvalStampClass } from '@/ideas/approvalStamp'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useIdeaWithPoll } from '@/ideas/useIdeaWithPoll'
import { coverImage } from '@/lib/images'
import { useNow } from '@/lib/useNow'
import { pollLineStyle } from '@/polls/pollLine'
import { pollDaysLeft, showsTally } from '@/polls/pollTimeline'
import { PollTicketBand } from '@/polls/PollTicketBand'
import { pollTypeKey } from '@/polls/pollTypes'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Idea = components['schemas']['IdeaWithPollDto']
type Poll = components['schemas']['PollDto']

const heroGrid = 'grid items-center gap-10 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)]'

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
      <div className="paper poster tape mx-auto max-w-md rotate-1 p-4 pt-6">
        <Alert variant="destructive" className="border-none bg-transparent">
          <CircleAlert />
          <AlertTitle>{notFound ? t('idea.notFound') : t('idea.loadFailed')}</AlertTitle>
        </Alert>
      </div>
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
      <section className={heroGrid}>
        <div className="paper poster tape relative -rotate-[0.4deg] px-6 pt-10 pb-8 sm:px-10">
          <span
            aria-hidden="true"
            className="halftone pointer-events-none absolute right-0 bottom-0 h-2/3 w-1/2 text-riot/45 [mask-image:linear-gradient(315deg,black,transparent_75%)]"
          />
          <div className="relative flex flex-col items-start gap-5">
            <span className="flex flex-wrap items-center gap-2">
              <span
                aria-hidden="true"
                style={pollLineStyle(idea.pollId)}
                className="h-5 w-10 -rotate-2 bg-(--poll-line)"
              />
              <IdeaApprovalBadge status={idea.approvalStatus} className={approvalStampClass} />
            </span>
            <h1 className="text-4xl leading-[1.05] text-foreground [overflow-wrap:anywhere] sm:text-6xl">
              {idea.title}
            </h1>
            {idea.description && (
              <p className="max-w-lg whitespace-pre-line text-muted-foreground">
                {idea.description}
              </p>
            )}
            <IdeaMeta idea={idea} />
          </div>
        </div>
        <div className="group mx-auto w-full max-w-sm">
          <HalftonePoster
            seed={idea.id ?? idea.title ?? ''}
            title={idea.title}
            photoUrl={coverImage(idea.images)?.url}
          />
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
    <p className="border-t-2 border-dashed border-foreground/60 pt-3 font-label text-xs tracking-[0.2em] text-muted-foreground uppercase">
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
      className="paper poster mx-auto max-w-4xl rotate-[0.4deg]"
    >
      <PollTicketBand pollId={poll.id} />
      <div className="flex flex-col items-center gap-6 px-4 py-8 text-center sm:px-10 lg:flex-row lg:justify-between lg:text-left">
        <div className="flex min-w-0 flex-col items-center gap-3 lg:items-start">
          <span className="flex flex-wrap items-center gap-2 font-label text-xs tracking-[0.25em] uppercase">
            <span className="bg-ink px-2 py-0.5 text-paper">{t('idea.poll')}</span>
            {typeKey && <span className="text-muted-foreground">{t(typeKey)}</span>}
          </span>
          <h2 id="idea-poll-title" className="text-3xl text-foreground [overflow-wrap:anywhere]">
            {poll.title}
          </h2>
          {poll.endsAt && (
            <span className="flex flex-wrap items-center justify-center gap-3">
              <span className="inline-flex items-center gap-2 font-heading text-lg text-coral">
                <Clock className="size-4" />
                {daysLeft > 0
                  ? t('poll.route.daysLeft', { count: daysLeft })
                  : t('poll.route.closed')}
              </span>
              {showsTally(daysLeft) && <TallyMarks count={daysLeft} className="text-foreground" />}
            </span>
          )}
        </div>
        <BoxAction
          icon={ArrowRight}
          label={t('poll.open')}
          to={`/polls/${poll.id}`}
          className="shrink-0"
        />
      </div>
    </section>
  )
}

function IdeaSkeleton() {
  return (
    <div className={heroGrid}>
      <div className="paper poster flex flex-col items-start gap-5 px-6 pt-10 pb-8 sm:px-10">
        <Skeleton className="h-5 w-40" />
        <Skeleton className="h-28 w-full max-w-lg" />
        <Skeleton className="h-12 w-full max-w-md" />
        <Skeleton className="h-4 w-48" />
      </div>
      <Skeleton className="mx-auto aspect-square w-full max-w-sm" />
    </div>
  )
}
