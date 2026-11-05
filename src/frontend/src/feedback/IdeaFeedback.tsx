import { useAuth0 } from '@auth0/auth0-react'
import { Heart, LoaderCircle, LogIn } from 'lucide-react'
import { type FormEvent, type ReactNode, useId, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation } from 'react-router'
import { ActionErrorAlert } from '@/admin/components/ActionErrorAlert'
import { ConfirmDialog } from '@/admin/components/ConfirmDialog'
import type { components } from '@/api/schema/feedback'
import { CharacterCount } from '@/components/CharacterCount'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { cn } from '@/lib/utils'
import { type FeedbackWriteAccess, useFeedbackAccess } from '@/feedback/useFeedbackAccess'
import {
  useAddFeedback,
  useDeleteFeedback,
  useEditComment,
  useIdeaComments,
  useIdeaLikes,
} from '@/feedback/useIdeaFeedback'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Comment = components['schemas']['FeedbackResponse']

const MAX_COMMENT_LENGTH = 1000

const deniedMessages = {
  noPermission: 'feedback.noPermission',
  otherCity: 'feedback.otherCity',
} as const

interface IdeaFeedbackProps {
  ideaId: string
  cityId: string | undefined
}

export function IdeaFeedback({ ideaId, cityId }: Readonly<IdeaFeedbackProps>) {
  const { t } = useTranslation()
  const access = useFeedbackAccess(cityId)
  const likes = useIdeaLikes(ideaId, access.userId, access.canRead)
  const comments = useIdeaComments(ideaId, access.canRead)
  const commentCount = comments.data?.pages[0]?.totalCount ?? 0

  let body: ReactNode
  if (access.write === 'guest') {
    body = <GuestPrompt />
  } else if (access.write === 'loading' || (access.canRead && comments.isPending)) {
    body = <FeedbackSkeleton />
  } else if (!access.canRead) {
    body = <p className="text-muted-foreground">{t('feedback.noPermission')}</p>
  } else if (comments.error || likes.error) {
    body = <ActionErrorAlert message={t('feedback.loadFailed')} />
  } else {
    body = (
      <div className="space-y-6">
        <WriteArea ideaId={ideaId} write={access.write} />
        <CommentList ideaId={ideaId} access={access} comments={comments} />
      </div>
    )
  }

  return (
    <section
      aria-labelledby="idea-feedback-title"
      className="mx-auto max-w-3xl space-y-6 border bg-card px-4 py-10 sm:px-10"
    >
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 id="idea-feedback-title" className="text-3xl font-medium text-heading">
          {t('feedback.title')}
          {access.canRead && commentCount > 0 && (
            <span className="ml-3 font-heading text-xl text-coral tabular-nums">
              {commentCount}
            </span>
          )}
        </h2>
        {access.canRead && likes.data && (
          <LikeButton
            ideaId={ideaId}
            count={likes.data.count}
            myLikeId={likes.data.myLikeId}
            canLike={access.write === 'allowed'}
            canUnlike={access.canDeleteOwn}
          />
        )}
      </div>
      {body}
    </section>
  )
}

interface LikeButtonProps {
  ideaId: string
  count: number
  myLikeId: string | undefined
  canLike: boolean
  canUnlike: boolean
}

function LikeButton({ ideaId, count, myLikeId, canLike, canUnlike }: Readonly<LikeButtonProps>) {
  const { t, i18n } = useTranslation()
  const addFeedback = useAddFeedback(ideaId)
  const deleteFeedback = useDeleteFeedback(ideaId)
  const liked = Boolean(myLikeId)
  const pending = addFeedback.isPending || deleteFeedback.isPending
  const enabled = liked ? canUnlike : canLike
  const formattedCount = new Intl.NumberFormat(i18n.language).format(count)

  const toggle = () => {
    if (myLikeId) {
      deleteFeedback.mutate({ id: myLikeId, asAdmin: false })
    } else {
      addFeedback.mutate(null)
    }
  }

  if (!enabled) {
    return (
      <span className="inline-flex items-center gap-1.5 text-sm text-muted-foreground tabular-nums">
        <Heart className={cn('size-4', liked && 'fill-current')} />
        {t('feedback.likes', { count, formatted: formattedCount })}
      </span>
    )
  }

  return (
    <Button
      variant={liked ? 'default' : 'outline'}
      size="sm"
      aria-pressed={liked}
      disabled={pending}
      onClick={toggle}
    >
      <Heart className={cn(liked && 'fill-current')} />
      <span className="tabular-nums">
        {t('feedback.likes', { count, formatted: formattedCount })}
      </span>
    </Button>
  )
}

function GuestPrompt() {
  const { t, i18n } = useTranslation()
  const { loginWithRedirect } = useAuth0()
  const location = useLocation()

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 border bg-background p-4">
      <p className="text-muted-foreground">{t('feedback.loginPrompt')}</p>
      <Button
        size="sm"
        onClick={() =>
          void loginWithRedirect({
            appState: { returnTo: location.pathname + location.search },
            authorizationParams: { ui_locales: i18n.resolvedLanguage },
          })
        }
      >
        <LogIn />
        {t('auth.logIn')}
      </Button>
    </div>
  )
}

function WriteArea({ ideaId, write }: Readonly<{ ideaId: string; write: FeedbackWriteAccess }>) {
  const { t } = useTranslation()

  if (write === 'allowed') {
    return <CommentForm ideaId={ideaId} />
  }
  if (write === 'noPermission' || write === 'otherCity') {
    return <p className="text-sm text-muted-foreground">{t(deniedMessages[write])}</p>
  }
  return null
}

function CommentForm({ ideaId }: Readonly<{ ideaId: string }>) {
  const { t } = useTranslation()
  const id = useId()
  const [text, setText] = useState('')
  const addFeedback = useAddFeedback(ideaId)
  const trimmed = text.trim()

  const submit = (event: FormEvent) => {
    event.preventDefault()
    if (!trimmed) {
      return
    }
    addFeedback.mutate(trimmed, { onSuccess: () => setText('') })
  }

  return (
    <form onSubmit={submit} className="space-y-2">
      <label htmlFor={id} className="sr-only">
        {t('feedback.placeholder')}
      </label>
      <Textarea
        id={id}
        value={text}
        maxLength={MAX_COMMENT_LENGTH}
        rows={3}
        placeholder={t('feedback.placeholder')}
        onChange={(event) => setText(event.target.value)}
      />
      <div className="flex items-start justify-between gap-3">
        <Button type="submit" size="sm" disabled={!trimmed || addFeedback.isPending}>
          {addFeedback.isPending && <LoaderCircle className="animate-spin" />}
          {t('feedback.submit')}
        </Button>
        <CharacterCount value={text} max={MAX_COMMENT_LENGTH} />
      </div>
      {addFeedback.error && <ActionErrorAlert message={t('feedback.actionFailed')} />}
    </form>
  )
}

interface CommentListProps {
  ideaId: string
  access: ReturnType<typeof useFeedbackAccess>
  comments: ReturnType<typeof useIdeaComments>
}

function CommentList({ ideaId, access, comments }: Readonly<CommentListProps>) {
  const { t } = useTranslation()
  const items = comments.data?.pages.flatMap((page) => page.items ?? []) ?? []
  const { data: nicknames } = usePublicProfiles(items.map((comment) => comment.userId))
  const deleteFeedback = useDeleteFeedback(ideaId)
  const [pendingDelete, setPendingDelete] = useState<Comment | null>(null)

  if (!items.length) {
    return <p className="text-muted-foreground">{t('feedback.noComments')}</p>
  }

  const confirmDelete = () => {
    if (pendingDelete?.id) {
      deleteFeedback.mutate({
        id: pendingDelete.id,
        asAdmin: pendingDelete.userId !== access.userId,
      })
    }
    setPendingDelete(null)
  }

  return (
    <div className="space-y-4">
      {deleteFeedback.error && <ActionErrorAlert message={t('feedback.actionFailed')} />}
      <ul className="divide-y">
        {items.map((comment) => {
          const own = Boolean(access.userId) && comment.userId === access.userId
          return (
            <li key={comment.id} className="py-4 first:pt-0">
              <CommentItem
                ideaId={ideaId}
                comment={comment}
                author={comment.userId ? nicknames?.get(comment.userId) : undefined}
                canEdit={own && access.canUpdateOwn}
                canDelete={(own && access.canDeleteOwn) || access.canDeleteAny}
                onDelete={() => setPendingDelete(comment)}
              />
            </li>
          )
        })}
      </ul>
      {comments.hasNextPage && (
        <Button
          variant="outline"
          size="sm"
          disabled={comments.isFetchingNextPage}
          onClick={() => void comments.fetchNextPage()}
        >
          {comments.isFetchingNextPage && <LoaderCircle className="animate-spin" />}
          {t('feedback.showMore')}
        </Button>
      )}
      <ConfirmDialog
        content={
          pendingDelete && {
            title: t('feedback.deleteTitle'),
            description: t('feedback.deleteDescription'),
            confirmLabel: t('feedback.delete'),
            destructive: true,
          }
        }
        onConfirm={confirmDelete}
        onCancel={() => setPendingDelete(null)}
      />
    </div>
  )
}

interface CommentItemProps {
  ideaId: string
  comment: Comment
  author: string | undefined
  canEdit: boolean
  canDelete: boolean
  onDelete: () => void
}

function CommentItem({
  ideaId,
  comment,
  author,
  canEdit,
  canDelete,
  onDelete,
}: Readonly<CommentItemProps>) {
  const { t, i18n } = useTranslation()
  const [editing, setEditing] = useState(false)
  const createdAt = comment.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium', timeStyle: 'short' }).format(
        new Date(comment.createdAt),
      )
    : null

  return (
    <article className="space-y-1.5">
      <div className="flex flex-wrap items-baseline gap-x-2 gap-y-1 text-sm">
        <span className="font-heading text-base text-heading">
          {author ?? t('idea.unknownAuthor')}
        </span>
        {createdAt && <span className="text-muted-foreground">{createdAt}</span>}
        {comment.updatedAt && (
          <span className="text-muted-foreground">· {t('feedback.edited')}</span>
        )}
      </div>
      {editing ? (
        <CommentEditor ideaId={ideaId} comment={comment} onDone={() => setEditing(false)} />
      ) : (
        <p className="whitespace-pre-line [overflow-wrap:anywhere]">{comment.text}</p>
      )}
      {!editing && (canEdit || canDelete) && (
        <div className="-ml-2 flex gap-1">
          {canEdit && (
            <Button variant="ghost" size="xs" onClick={() => setEditing(true)}>
              {t('feedback.edit')}
            </Button>
          )}
          {canDelete && (
            <Button
              variant="ghost"
              size="xs"
              className="text-destructive hover:text-destructive"
              onClick={onDelete}
            >
              {t('feedback.delete')}
            </Button>
          )}
        </div>
      )}
    </article>
  )
}

interface CommentEditorProps {
  ideaId: string
  comment: Comment
  onDone: () => void
}

function CommentEditor({ ideaId, comment, onDone }: Readonly<CommentEditorProps>) {
  const { t } = useTranslation()
  const [text, setText] = useState(comment.text ?? '')
  const editComment = useEditComment(ideaId)
  const trimmed = text.trim()

  const submit = (event: FormEvent) => {
    event.preventDefault()
    if (!comment.id || !trimmed) {
      return
    }
    editComment.mutate({ id: comment.id, text: trimmed }, { onSuccess: onDone })
  }

  return (
    <form onSubmit={submit} className="space-y-2">
      <Textarea
        value={text}
        maxLength={MAX_COMMENT_LENGTH}
        rows={3}
        aria-label={t('feedback.edit')}
        autoFocus
        onChange={(event) => setText(event.target.value)}
      />
      <div className="flex items-start justify-between gap-3">
        <div className="flex gap-2">
          <Button type="submit" size="sm" disabled={!trimmed || editComment.isPending}>
            {editComment.isPending && <LoaderCircle className="animate-spin" />}
            {t('feedback.save')}
          </Button>
          <Button type="button" variant="ghost" size="sm" onClick={onDone}>
            {t('feedback.cancel')}
          </Button>
        </div>
        <CharacterCount value={text} max={MAX_COMMENT_LENGTH} />
      </div>
      {editComment.error && <ActionErrorAlert message={t('feedback.actionFailed')} />}
    </form>
  )
}

function FeedbackSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-20 w-full" />
      <Skeleton className="h-4 w-40" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-2/3" />
    </div>
  )
}
