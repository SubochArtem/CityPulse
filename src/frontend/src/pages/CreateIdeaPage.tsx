import { ArrowLeft, CircleAlert, LoaderCircle } from 'lucide-react'
import { type FormEvent, type ReactNode, useId, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { CharacterCount } from '@/components/CharacterCount'
import { ImagePicker } from '@/components/ImagePicker'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { useCreateIdea } from '@/ideas/useCreateIdea'
import { type IdeaCreationAccess, useIdeaCreationAccess } from '@/ideas/useIdeaCreationAccess'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { usePollWithIdeas } from '@/polls/usePollWithIdeas'

type Poll = components['schemas']['PollDto']

const MAX_TITLE_LENGTH = 200
const MAX_DESCRIPTION_LENGTH = 1000

const deniedMessages = {
  closed: 'ideaForm.closed',
  noPermission: 'ideaForm.noPermission',
  otherCity: 'ideaForm.otherCity',
} as const

export function CreateIdeaPage() {
  const { t } = useTranslation()
  const { pollId = '' } = useParams()
  const { data: poll, error } = usePollWithIdeas(pollId)
  const access = useIdeaCreationAccess(poll)

  let content: ReactNode
  if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{notFound ? t('poll.notFound') : t('poll.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else if (access === 'loading' || access === 'guest') {
    content = <FormSkeleton />
  } else if (access === 'allowed') {
    content = poll && <IdeaForm poll={poll} />
  } else {
    content = <AccessDenied access={access} />
  }

  return (
    <section className="max-w-2xl space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to={`/polls/${pollId}`}>
          <ArrowLeft />
          {t('idea.back')}
        </Link>
      </Button>
      <div className="space-y-1">
        <h1 className="text-3xl font-semibold tracking-tight">{t('ideaForm.heading')}</h1>
        {poll?.title && (
          <p className="flex items-center gap-2 text-muted-foreground">
            <PollLineBadge pollId={poll.id} />
            {t('ideaForm.forPoll', { poll: poll.title })}
          </p>
        )}
      </div>
      {content}
    </section>
  )
}

function AccessDenied({
  access,
}: Readonly<{ access: Exclude<IdeaCreationAccess, 'loading' | 'guest' | 'allowed'> }>) {
  const { t } = useTranslation()

  return (
    <Alert className="max-w-md">
      <CircleAlert />
      <AlertTitle>{t(deniedMessages[access])}</AlertTitle>
    </Alert>
  )
}

function submitErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 403) {
    return 'ideaForm.otherCity'
  }
  if (error instanceof ApiError && error.status === 409) {
    return 'ideaForm.conflict'
  }
  return 'ideaForm.failed'
}

function IdeaForm({ poll }: Readonly<{ poll: Poll }>) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const titleId = useId()
  const descriptionId = useId()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [images, setImages] = useState<File[]>([])
  const createIdea = useCreateIdea(poll.id ?? '')

  const isValid = title.trim().length > 0
  const isSubmitting = createIdea.isPending

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!isValid || isSubmitting) {
      return
    }
    createIdea.mutate(
      { title: title.trim(), description: description.trim(), images },
      { onSuccess: (idea) => void navigate(`/ideas/${idea.id}`, { replace: true }) },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6" noValidate>
      <div className="space-y-2">
        <Label htmlFor={titleId}>{t('ideaForm.title')}</Label>
        <Input
          id={titleId}
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          maxLength={MAX_TITLE_LENGTH}
          disabled={isSubmitting}
          required
          autoFocus
        />
        <CharacterCount value={title} max={MAX_TITLE_LENGTH} />
      </div>

      <div className="space-y-2">
        <Label htmlFor={descriptionId}>
          {t('ideaForm.description')}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </Label>
        <Textarea
          id={descriptionId}
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          maxLength={MAX_DESCRIPTION_LENGTH}
          disabled={isSubmitting}
          rows={6}
          className="min-h-32"
        />
        <CharacterCount value={description} max={MAX_DESCRIPTION_LENGTH} />
      </div>

      <div className="space-y-2">
        <p className="flex items-center gap-2 text-sm font-medium">
          {t('ideaForm.images')}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </p>
        <ImagePicker files={images} onChange={setImages} disabled={isSubmitting} />
      </div>

      {createIdea.error && (
        <Alert variant="destructive">
          <CircleAlert />
          <AlertTitle>{t(submitErrorKey(createIdea.error))}</AlertTitle>
        </Alert>
      )}

      <div className="flex flex-wrap gap-2">
        <Button type="submit" disabled={!isValid || isSubmitting}>
          {isSubmitting && <LoaderCircle className="animate-spin" />}
          {isSubmitting ? t('ideaForm.submitting') : t('ideaForm.submit')}
        </Button>
        <Button type="button" variant="ghost" asChild>
          <Link to={`/polls/${poll.id}`}>{t('ideaForm.cancel')}</Link>
        </Button>
      </div>
    </form>
  )
}

function FormSkeleton() {
  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-8 w-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-28" />
        <Skeleton className="h-32 w-full" />
      </div>
      <Skeleton className="aspect-video w-1/3 rounded-lg" />
    </div>
  )
}
