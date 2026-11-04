import { ArrowLeft, CircleAlert, Lightbulb, Plus, UserRound } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { ImageGallery } from '@/components/ImageGallery'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useIdeaCreationAccess } from '@/ideas/useIdeaCreationAccess'
import { coverImage } from '@/lib/images'
import { PollFacts } from '@/polls/PollFacts'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { PollRoute } from '@/polls/PollRoute'
import { PollTypeBadge } from '@/polls/PollTypeBadge'
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
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('poll.notFound') : t('poll.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else {
    content = <PollDetails poll={poll} />
  }

  return (
    <section className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to={poll?.cityId ? `/cities/${poll.cityId}` : '/cities'}>
          <ArrowLeft />
          {poll?.cityId ? t('poll.back') : t('city.back')}
        </Link>
      </Button>
      {content}
    </section>
  )
}

function PollDetails({ poll }: Readonly<{ poll: Poll }>) {
  const { t } = useTranslation()
  const ideas = [...(poll.ideas ?? [])].sort((a, b) =>
    (b.createdAt ?? '').localeCompare(a.createdAt ?? ''),
  )
  const { data: nicknames } = usePublicProfiles(ideas.map((idea) => idea.userId))

  return (
    <>
      <div className="space-y-4">
        <ImageGallery key={poll.id} images={poll.images} alt={poll.title ?? ''} />
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <PollLineBadge pollId={poll.id} />
            <PollTypeBadge type={poll.type} />
          </div>
          <h1 className="text-3xl font-semibold tracking-tight">{poll.title}</h1>
          <PollFacts endsAt={poll.endsAt} budgetAmount={poll.budgetAmount} />
        </div>
        <PollRoute
          pollId={poll.id}
          createdAt={poll.createdAt}
          endsAt={poll.endsAt}
          className="max-w-xl"
        />
        {poll.description && (
          <p className="max-w-prose whitespace-pre-line text-muted-foreground">
            {poll.description}
          </p>
        )}
      </div>

      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h2 className="text-xl font-semibold tracking-tight">{t('poll.ideas')}</h2>
          <SuggestIdeaButton poll={poll} />
        </div>
        {ideas.length ? (
          <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
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
          <p className="text-muted-foreground">{t('poll.noIdeas')}</p>
        )}
      </div>
    </>
  )
}

function SuggestIdeaButton({ poll }: Readonly<{ poll: Poll }>) {
  const { t } = useTranslation()
  const access = useIdeaCreationAccess(poll)

  if (access !== 'allowed' && access !== 'guest') {
    return null
  }

  return (
    <Button size="sm" asChild>
      <Link to={`/polls/${poll.id}/ideas/new`}>
        <Plus />
        {t('poll.suggestIdea')}
      </Link>
    </Button>
  )
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
      className="group block h-full rounded-xl outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
    >
      <Card className="h-full pt-0 transition-shadow group-hover:ring-foreground/25">
        {cover?.url ? (
          <img src={cover.url} alt="" loading="lazy" className="aspect-video w-full object-cover" />
        ) : (
          <div className="flex aspect-video items-center justify-center bg-muted text-muted-foreground">
            <Lightbulb className="size-8" />
          </div>
        )}
        <CardHeader>
          <div className="mb-1 flex items-center gap-2">
            <PollLineBadge pollId={idea.pollId} />
            <IdeaApprovalBadge status={idea.approvalStatus} />
          </div>
          <CardTitle className="text-base">{idea.title}</CardTitle>
          {idea.description && (
            <CardDescription className="line-clamp-3">{idea.description}</CardDescription>
          )}
        </CardHeader>
        {(author || createdAt) && (
          <CardContent className="mt-auto flex flex-wrap gap-x-3 gap-y-1 text-muted-foreground">
            {author && (
              <span className="inline-flex items-center gap-1.5">
                <UserRound className="size-4" />
                {author}
              </span>
            )}
            {createdAt && <span>{t('idea.createdAt', { date: createdAt })}</span>}
          </CardContent>
        )}
      </Card>
    </Link>
  )
}

function PollSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="aspect-21/9 w-full rounded-xl" />
      <Skeleton className="h-5 w-28" />
      <Skeleton className="h-9 w-72" />
      <Skeleton className="h-4 w-48" />
      <Skeleton className="h-4 w-full max-w-prose" />
    </div>
  )
}
