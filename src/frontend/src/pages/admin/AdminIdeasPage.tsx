import { Ellipsis, Eye, EyeOff, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { ActionErrorAlert } from '@/admin/components/ActionErrorAlert'
import { AdminListContent } from '@/admin/components/AdminListContent'
import { AdminSearchInput } from '@/admin/components/AdminSearchInput'
import { ConfirmDialog } from '@/admin/components/ConfirmDialog'
import { FilterSelect } from '@/admin/components/FilterSelect'
import { ADMIN_IDEAS_PAGE_SIZE, useAdminIdeas } from '@/admin/ideas/useAdminIdeas'
import {
  useChangeIdeaAccess,
  useChangeIdeaApproval,
  useDeleteIdea,
} from '@/admin/ideas/useIdeaMutations'
import { useAdminPollOptions } from '@/admin/polls/useAdminPollOptions'
import { readEnum } from '@/admin/readEnum'
import { usePermissions } from '@/auth/usePermissions'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { TableCell, TableHead, TableRow } from '@/components/ui/table'
import { IdeaApprovalBadge } from '@/ideas/IdeaApprovalBadge'
import { useListSearchParams } from '@/lib/useListSearchParams'
import { usePublicProfiles } from '@/users/usePublicProfiles'

type Idea = components['schemas']['IdeaDto']
type IdeaAccessStatus = components['schemas']['IdeaAccessStatus']
type IdeaApprovalStatus = components['schemas']['IdeaApprovalStatus']

const ACTIVE: IdeaAccessStatus = 1
const INACTIVE: IdeaAccessStatus = 2

const accessLabels = {
  1: 'admin.ideas.access.active',
  2: 'admin.ideas.access.inactive',
  3: 'admin.ideas.access.restrictedByContext',
  4: 'admin.ideas.access.restrictedByAuthor',
} as const

const approvalLabels = {
  4: 'idea.approval.pending',
  1: 'idea.approval.approved',
  3: 'idea.approval.deferred',
  2: 'idea.approval.rejected',
} as const

const approvalOrder = [4, 1, 3, 2] as const

type PendingAction = { type: 'hide' | 'show' | 'delete'; idea: Idea }

interface IdeaPermissions {
  approval: boolean
  access: boolean
  delete: boolean
}

const dialogKeys = {
  hide: {
    title: 'admin.ideas.hideTitle',
    description: 'admin.ideas.hideDescription',
    confirm: 'admin.ideas.hide',
  },
  show: {
    title: 'admin.ideas.showTitle',
    description: 'admin.ideas.showDescription',
    confirm: 'admin.ideas.show',
  },
  delete: {
    title: 'admin.ideas.deleteTitle',
    description: 'admin.ideas.deleteDescription',
    confirm: 'admin.ideas.delete',
  },
} as const

function actionErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.ideas.notFound'
  }
  return 'admin.ideas.actionFailed'
}

export function AdminIdeasPage() {
  const { t } = useTranslation()
  const { can } = usePermissions()
  const { searchParams, search, page, searchInput, setSearchInput, setParam, goToPage } =
    useListSearchParams()
  const pollId = searchParams.get('poll') ?? undefined
  const accessStatus = readEnum<IdeaAccessStatus>(searchParams.get('access'), accessLabels)
  const approvalStatus = readEnum<IdeaApprovalStatus>(searchParams.get('approval'), approvalLabels)
  const { data, error, isPending, isPlaceholderData } = useAdminIdeas({
    search,
    pollId,
    accessStatus,
    approvalStatus,
    page,
  })
  const { data: polls } = useAdminPollOptions()
  const { data: nicknames } = usePublicProfiles(data?.items?.map((idea) => idea.userId) ?? [])
  const pollTitles = new Map(polls?.map((poll) => [poll.id, poll.title]))
  const changeApproval = useChangeIdeaApproval()
  const changeAccess = useChangeIdeaAccess()
  const deleteIdea = useDeleteIdea()
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null)

  const permissions: IdeaPermissions = {
    approval: can('ideas.change-approval-status.any'),
    access: can('ideas.change-access-status.any'),
    delete: can('ideas.delete.any'),
  }
  const hasActions = Object.values(permissions).some(Boolean)
  const actionError = changeApproval.error ?? changeAccess.error ?? deleteIdea.error
  const isMutating = changeApproval.isPending || changeAccess.isPending || deleteIdea.isPending

  const resetMutations = () => {
    changeApproval.reset()
    changeAccess.reset()
    deleteIdea.reset()
  }

  const changeIdeaApproval = (id: string, status: IdeaApprovalStatus) => {
    resetMutations()
    changeApproval.mutate({ id, status })
  }

  const confirmAction = () => {
    const id = pendingAction?.idea.id
    if (!pendingAction || !id) {
      return
    }
    resetMutations()
    if (pendingAction.type === 'delete') {
      deleteIdea.mutate(id)
    } else {
      changeAccess.mutate({ id, status: pendingAction.type === 'show' ? ACTIVE : INACTIVE })
    }
    setPendingAction(null)
  }

  const dialog = pendingAction ? dialogKeys[pendingAction.type] : null

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-2 lg:flex-row lg:items-center">
        <AdminSearchInput
          value={searchInput}
          onChange={setSearchInput}
          placeholder={t('admin.ideas.searchPlaceholder')}
        />
        <div className="flex flex-wrap gap-2">
          <FilterSelect
            label={t('admin.ideas.columns.poll')}
            allLabel={t('admin.ideas.allPolls')}
            value={pollId}
            options={(polls ?? []).map((poll) => ({ value: poll.id, label: poll.title }))}
            onChange={(value) => setParam('poll', value)}
          />
          <FilterSelect
            label={t('admin.ideas.columns.approval')}
            allLabel={t('admin.ideas.allApprovals')}
            value={approvalStatus === undefined ? undefined : String(approvalStatus)}
            options={approvalOrder.map((value) => ({
              value: String(value),
              label: t(approvalLabels[value]),
            }))}
            onChange={(value) => setParam('approval', value)}
          />
          <FilterSelect
            label={t('admin.ideas.columns.access')}
            allLabel={t('admin.ideas.allAccess')}
            value={accessStatus === undefined ? undefined : String(accessStatus)}
            options={Object.entries(accessLabels).map(([value, key]) => ({
              value,
              label: t(key),
            }))}
            onChange={(value) => setParam('access', value)}
          />
        </div>
      </div>

      {actionError && <ActionErrorAlert message={t(actionErrorKey(actionError))} />}

      <AdminListContent
        data={data}
        error={error}
        isPending={isPending}
        isPlaceholderData={isPlaceholderData}
        page={page}
        pageSize={ADMIN_IDEAS_PAGE_SIZE}
        onPageChange={goToPage}
        loadFailedLabel={t('admin.ideas.loadFailed')}
        emptyLabel={t('admin.ideas.empty')}
        head={
          <>
            <TableHead>{t('admin.ideas.columns.title')}</TableHead>
            <TableHead className="hidden md:table-cell">{t('admin.ideas.columns.poll')}</TableHead>
            <TableHead className="hidden lg:table-cell">
              {t('admin.ideas.columns.author')}
            </TableHead>
            <TableHead>{t('admin.ideas.columns.approval')}</TableHead>
            <TableHead className="hidden sm:table-cell">
              {t('admin.ideas.columns.access')}
            </TableHead>
            <TableHead className="hidden xl:table-cell">
              {t('admin.ideas.columns.createdAt')}
            </TableHead>
            {hasActions && (
              <TableHead className="w-12">
                <span className="sr-only">{t('admin.ideas.columns.actions')}</span>
              </TableHead>
            )}
          </>
        }
      >
        {data?.items?.map((idea) => (
          <IdeaRow
            key={idea.id}
            idea={idea}
            pollTitle={pollTitles.get(idea.pollId ?? '')}
            author={nicknames?.get(idea.userId ?? '')}
            permissions={permissions}
            disabled={isMutating}
            onApprovalChange={(status) => idea.id && changeIdeaApproval(idea.id, status)}
            onAction={(type) => setPendingAction({ type, idea })}
          />
        ))}
      </AdminListContent>

      <ConfirmDialog
        content={
          dialog && pendingAction
            ? {
                title: t(dialog.title, { title: pendingAction.idea.title ?? '' }),
                description: t(dialog.description),
                confirmLabel: t(dialog.confirm),
                destructive: pendingAction.type !== 'show',
              }
            : null
        }
        onConfirm={confirmAction}
        onCancel={() => setPendingAction(null)}
      />
    </div>
  )
}

interface IdeaRowProps {
  idea: Idea
  pollTitle: string | undefined
  author: string | undefined
  permissions: IdeaPermissions
  disabled: boolean
  onApprovalChange: (status: IdeaApprovalStatus) => void
  onAction: (type: PendingAction['type']) => void
}

function IdeaRow({
  idea,
  pollTitle,
  author,
  permissions,
  disabled,
  onApprovalChange,
  onAction,
}: Readonly<IdeaRowProps>) {
  const { t, i18n } = useTranslation()
  const isActive = idea.accessStatus === ACTIVE
  const accessKey = accessLabels[idea.accessStatus as keyof typeof accessLabels]
  const canHide = permissions.access && isActive
  const canShow = permissions.access && idea.accessStatus === INACTIVE
  const hasActions = Object.values(permissions).some(Boolean)
  const hasMenu = permissions.approval || canHide || canShow || permissions.delete
  const createdAt = idea.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(idea.createdAt),
      )
    : null

  return (
    <TableRow>
      <TableCell className="font-medium whitespace-normal">
        {isActive ? (
          <Link
            to={`/ideas/${idea.id}`}
            className="underline-offset-4 outline-none hover:underline focus-visible:underline"
          >
            {idea.title}
          </Link>
        ) : (
          idea.title
        )}
      </TableCell>
      <TableCell className="hidden whitespace-normal text-muted-foreground md:table-cell">
        {pollTitle}
      </TableCell>
      <TableCell className="hidden text-muted-foreground lg:table-cell">
        {author ?? t('idea.unknownAuthor')}
      </TableCell>
      <TableCell>
        <IdeaApprovalBadge status={idea.approvalStatus} />
      </TableCell>
      <TableCell className="hidden sm:table-cell">
        {accessKey && <Badge variant={isActive ? 'secondary' : 'outline'}>{t(accessKey)}</Badge>}
      </TableCell>
      <TableCell className="hidden text-muted-foreground xl:table-cell">{createdAt}</TableCell>
      {hasActions && (
        <TableCell>
          {hasMenu && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  disabled={disabled}
                  aria-label={t('admin.ideas.actionsFor', { title: idea.title ?? '' })}
                >
                  <Ellipsis />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="min-w-44">
                {permissions.approval && (
                  <>
                    <DropdownMenuLabel>{t('admin.ideas.columns.approval')}</DropdownMenuLabel>
                    <DropdownMenuRadioGroup
                      value={String(idea.approvalStatus)}
                      onValueChange={(value) =>
                        onApprovalChange(Number(value) as IdeaApprovalStatus)
                      }
                    >
                      {approvalOrder.map((value) => (
                        <DropdownMenuRadioItem key={value} value={String(value)}>
                          {t(approvalLabels[value])}
                        </DropdownMenuRadioItem>
                      ))}
                    </DropdownMenuRadioGroup>
                  </>
                )}
                {permissions.approval && (canHide || canShow || permissions.delete) && (
                  <DropdownMenuSeparator />
                )}
                {canHide && (
                  <DropdownMenuItem onSelect={() => onAction('hide')}>
                    <EyeOff />
                    {t('admin.ideas.hide')}
                  </DropdownMenuItem>
                )}
                {canShow && (
                  <DropdownMenuItem onSelect={() => onAction('show')}>
                    <Eye />
                    {t('admin.ideas.show')}
                  </DropdownMenuItem>
                )}
                {permissions.delete && (
                  <DropdownMenuItem variant="destructive" onSelect={() => onAction('delete')}>
                    <Trash2 />
                    {t('admin.ideas.delete')}
                  </DropdownMenuItem>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </TableCell>
      )}
    </TableRow>
  )
}
