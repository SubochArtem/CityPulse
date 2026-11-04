import { ArrowLeft, CircleAlert, Lightbulb } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { ImageGallery } from '@/components/ImageGallery'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { PollFacts } from '@/polls/PollFacts'
import { PollTypeBadge } from '@/polls/PollTypeBadge'
import { usePollWithIdeas } from '@/polls/usePollWithIdeas'

type Poll = components['schemas']['PollWithIdeasDto']
type Idea = components['schemas']['IdeaDto']

const approvalStatuses = {
  1: { key: 'idea.approval.approved', variant: 'default' },
  2: { key: 'idea.approval.rejected', variant: 'destructive' },
  3: { key: 'idea.approval.deferred', variant: 'outline' },
  4: { key: 'idea.approval.pending', variant: 'outline' },
} as const

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

  return (
    <>
      <div className="space-y-4">
        <ImageGallery key={poll.id} images={poll.images} alt={poll.title ?? ''} />
        <div className="space-y-2">
          <PollTypeBadge type={poll.type} />
          <h1 className="text-3xl font-semibold tracking-tight">{poll.title}</h1>
          <PollFacts endsAt={poll.endsAt} budgetAmount={poll.budgetAmount} />
        </div>
        {poll.description && (
          <p className="max-w-prose whitespace-pre-line text-muted-foreground">
            {poll.description}
          </p>
        )}
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">{t('poll.ideas')}</h2>
        {ideas.length ? (
          <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {ideas.map((idea) => (
              <li key={idea.id}>
                <IdeaCard idea={idea} />
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

function IdeaCard({ idea }: Readonly<{ idea: Idea }>) {
  const { t, i18n } = useTranslation()
  const cover = coverImage(idea.images)
  const approval = approvalStatuses[idea.approvalStatus as keyof typeof approvalStatuses]
  const createdAt = idea.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(idea.createdAt),
      )
    : null

  return (
    <Card className="h-full pt-0">
      {cover?.url ? (
        <img src={cover.url} alt="" loading="lazy" className="aspect-video w-full object-cover" />
      ) : (
        <div className="flex aspect-video items-center justify-center bg-muted text-muted-foreground">
          <Lightbulb className="size-8" />
        </div>
      )}
      <CardHeader>
        {approval && (
          <Badge variant={approval.variant} className="mb-1">
            {t(approval.key)}
          </Badge>
        )}
        <CardTitle className="text-base">{idea.title}</CardTitle>
        {idea.description && (
          <CardDescription className="line-clamp-3">{idea.description}</CardDescription>
        )}
      </CardHeader>
      {createdAt && (
        <CardContent className="mt-auto text-muted-foreground">
          {t('idea.createdAt', { date: createdAt })}
        </CardContent>
      )}
    </Card>
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
