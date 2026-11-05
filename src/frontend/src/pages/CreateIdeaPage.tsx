import { ArrowLeft, CircleAlert, LoaderCircle } from 'lucide-react'
import { type FormEvent, type ReactNode, useId, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { BoxAction } from '@/components/BoxAction'
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
import { pollLineStyle } from '@/polls/pollLine'
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
      <div className="paper poster tape mx-auto max-w-md rotate-1 p-4 pt-6">
        <Alert variant="destructive" className="border-none bg-transparent">
          <CircleAlert />
          <AlertTitle>{notFound ? t('poll.notFound') : t('poll.loadFailed')}</AlertTitle>
        </Alert>
      </div>
    )
  } else if (access === 'loading' || access === 'guest') {
    content = <FormSkeleton />
  } else if (access === 'allowed') {
    content = poll && <IdeaForm poll={poll} />
  } else {
    content = <AccessDenied access={access} />
  }

  return (
    <div className="space-y-10">
      <BoxAction icon={ArrowLeft} label={t('idea.back')} to={`/polls/${pollId}`} />
      <div className="mx-auto max-w-2xl space-y-10">
        <div className="flex flex-col items-center gap-5 text-center">
          <h1 className="-rotate-1 bg-foreground px-6 py-2 text-4xl text-background shadow-[3px_3px_0_oklch(0_0_0/0.25)] [overflow-wrap:anywhere] sm:text-5xl">
            {t('ideaForm.heading')}
          </h1>
          {poll?.title && (
            <p className="flex flex-col items-center gap-2 font-label text-xs tracking-[0.2em] text-muted-foreground uppercase [overflow-wrap:anywhere]">
              <span
                aria-hidden="true"
                style={pollLineStyle(poll.id)}
                className="h-4 w-8 shrink-0 -rotate-2 bg-(--poll-line)"
              />
              {t('ideaForm.forPoll', { poll: poll.title })}
            </p>
          )}
        </div>
        {content}
      </div>
    </div>
  )
}

function AccessDenied({
  access,
}: Readonly<{ access: Exclude<IdeaCreationAccess, 'loading' | 'guest' | 'allowed'> }>) {
  const { t } = useTranslation()

  return (
    <div className="paper poster tape mx-auto max-w-md -rotate-1 p-4 pt-6">
      <Alert className="border-none bg-transparent">
        <CircleAlert />
        <AlertTitle>{t(deniedMessages[access])}</AlertTitle>
      </Alert>
    </div>
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
    <form
      onSubmit={handleSubmit}
      className="paper poster tape space-y-6 px-4 pt-10 pb-8 sm:px-10"
      noValidate
    >
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
    <div className="paper poster space-y-6 px-4 pt-10 pb-8 sm:px-10">
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-8 w-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-28" />
        <Skeleton className="h-32 w-full" />
      </div>
      <Skeleton className="aspect-video w-1/3" />
    </div>
  )
}
