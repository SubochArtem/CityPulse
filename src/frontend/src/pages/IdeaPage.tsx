import { ArrowLeft, CalendarDays, CircleAlert, UserRound } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { ImageGallery } from '@/components/ImageGallery'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useIdeaWithPoll } from '@/ideas/useIdeaWithPoll'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { PollLineTile } from '@/polls/PollLineTile'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Idea = components['schemas']['IdeaWithPollDto']

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
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('idea.notFound') : t('idea.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else {
    content = <IdeaDetails idea={idea} />
  }

  return (
    <section className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to={idea?.pollId ? `/polls/${idea.pollId}` : '/cities'}>
          <ArrowLeft />
          {idea?.pollId ? t('idea.back') : t('city.back')}
        </Link>
      </Button>
      {content}
    </section>
  )
}

function IdeaDetails({ idea }: Readonly<{ idea: Idea }>) {
  const { t, i18n } = useTranslation()
  const dateFormat = new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' })

  return (
    <>
      <div className="space-y-4">
        <ImageGallery key={idea.id} images={idea.images} alt={idea.title ?? ''} />
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <PollLineBadge pollId={idea.pollId} />
            <IdeaApprovalBadge status={idea.approvalStatus} />
          </div>
          <h1 className="text-3xl font-semibold tracking-tight">{idea.title}</h1>
          <div className="flex flex-wrap gap-x-4 gap-y-1 text-muted-foreground">
            <IdeaAuthor userId={idea.userId} />
            {idea.createdAt && (
              <span className="inline-flex items-center gap-1.5">
                <CalendarDays className="size-4" />
                {t('idea.createdAt', { date: dateFormat.format(new Date(idea.createdAt)) })}
              </span>
            )}
            {idea.updatedAt && (
              <span>
                {t('idea.updatedAt', { date: dateFormat.format(new Date(idea.updatedAt)) })}
              </span>
            )}
          </div>
        </div>
        {idea.description && (
          <p className="max-w-prose whitespace-pre-line text-muted-foreground">
            {idea.description}
          </p>
        )}
      </div>

      <div className="max-w-xl space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">{t('idea.poll')}</h2>
        <PollLineTile poll={idea.poll} />
      </div>
    </>
  )
}

function IdeaAuthor({ userId }: Readonly<{ userId: string | undefined }>) {
  const { t } = useTranslation()
  const { data: nicknames, isLoading } = usePublicProfiles([userId])

  if (isLoading) {
    return <Skeleton className="h-5 w-32" />
  }

  const nickname = userId ? nicknames?.get(userId) : undefined

  return (
    <span className="inline-flex items-center gap-1.5">
      <UserRound className="size-4" />
      {nickname ? t('idea.author', { nickname }) : t('idea.unknownAuthor')}
    </span>
  )
}

function IdeaSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="aspect-21/9 w-full rounded-xl" />
      <Skeleton className="h-5 w-28" />
      <Skeleton className="h-9 w-72" />
      <Skeleton className="h-4 w-56" />
      <Skeleton className="h-4 w-full max-w-prose" />
    </div>
  )
}
