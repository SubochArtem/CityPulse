import { Ellipsis, Pause, Pencil, Play, Plus, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { useAdminCityOptions } from '@/admin/cities/useAdminCityOptions'
import { AdminListContent } from '@/admin/components/AdminListContent'
import { ActionErrorAlert } from '@/admin/components/ActionErrorAlert'
import { AdminSearchInput } from '@/admin/components/AdminSearchInput'
import { ConfirmDialog } from '@/admin/components/ConfirmDialog'
import { FilterSelect } from '@/admin/components/FilterSelect'
import { ADMIN_POLLS_PAGE_SIZE, useAdminPolls } from '@/admin/polls/useAdminPolls'
import { useChangePollStatus, useDeletePoll } from '@/admin/polls/usePollMutations'
import { readEnum } from '@/admin/readEnum'
import { usePermissions } from '@/auth/usePermissions'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { TableCell, TableHead, TableRow } from '@/components/ui/table'
import { useListSearchParams } from '@/lib/useListSearchParams'
import { PollTypeBadge } from '@/polls/PollTypeBadge'

type Poll = components['schemas']['PollDto']
type PollStatus = components['schemas']['PollStatus']
type PollType = components['schemas']['PollType']

const ACTIVE: PollStatus = 1
const SUSPENDED: PollStatus = 4

const statusLabels = {
  1: 'admin.polls.status.active',
  2: 'admin.polls.status.finished',
  3: 'admin.polls.status.cancelled',
  4: 'admin.polls.status.suspended',
  5: 'admin.polls.status.inactive',
} as const

const typeLabels = {
  1: 'poll.type.userVote',
  2: 'poll.type.managerVote',
} as const

type PendingAction = { type: 'suspend' | 'resume' | 'delete'; poll: Poll }

interface PollPermissions {
  update: boolean
  changeStatus: boolean
  delete: boolean
}

const dialogKeys = {
  suspend: {
    title: 'admin.polls.suspendTitle',
    description: 'admin.polls.suspendDescription',
    confirm: 'admin.polls.suspend',
  },
  resume: {
    title: 'admin.polls.resumeTitle',
    description: 'admin.polls.resumeDescription',
    confirm: 'admin.polls.resume',
  },
  delete: {
    title: 'admin.polls.deleteTitle',
    description: 'admin.polls.deleteDescription',
    confirm: 'admin.polls.delete',
  },
} as const

function actionErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.polls.notFound'
  }
  return 'admin.polls.actionFailed'
}

export function AdminPollsPage() {
  const { t } = useTranslation()
  const { can } = usePermissions()
  const { search: listSearch } = useLocation()
  const { searchParams, search, page, searchInput, setSearchInput, setParam, goToPage } =
    useListSearchParams()
  const cityId = searchParams.get('city') ?? undefined
  const status = readEnum<PollStatus>(searchParams.get('status'), statusLabels)
  const type = readEnum<PollType>(searchParams.get('type'), typeLabels)
  const { data, error, isPending, isPlaceholderData } = useAdminPolls({
    search,
    cityId,
    status,
    type,
    page,
  })
  const { data: cities } = useAdminCityOptions()
  const cityTitles = new Map(cities?.map((city) => [city.id, city.title]))
  const changeStatus = useChangePollStatus()
  const deletePoll = useDeletePoll()
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null)

  const permissions: PollPermissions = {
    update: can('polls.update.any'),
    changeStatus: can('polls.changestatus.any'),
    delete: can('polls.delete.any'),
  }
  const hasActions = Object.values(permissions).some(Boolean)
  const actionError = changeStatus.error ?? deletePoll.error
  const isMutating = changeStatus.isPending || deletePoll.isPending

  const confirmAction = () => {
    const id = pendingAction?.poll.id
    if (!pendingAction || !id) {
      return
    }
    changeStatus.reset()
    deletePoll.reset()
    if (pendingAction.type === 'delete') {
      deletePoll.mutate(id)
    } else {
      changeStatus.mutate({ id, status: pendingAction.type === 'resume' ? ACTIVE : SUSPENDED })
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
          placeholder={t('admin.polls.searchPlaceholder')}
        />
        <div className="flex flex-wrap gap-2">
          <FilterSelect
            label={t('admin.polls.columns.city')}
            allLabel={t('admin.polls.allCities')}
            value={cityId}
            options={(cities ?? []).map((city) => ({ value: city.id, label: city.title }))}
            onChange={(value) => setParam('city', value)}
          />
          <FilterSelect
            label={t('admin.polls.columns.status')}
            allLabel={t('admin.polls.status.all')}
            value={status === undefined ? undefined : String(status)}
            options={Object.entries(statusLabels).map(([value, key]) => ({
              value,
              label: t(key),
            }))}
            onChange={(value) => setParam('status', value)}
          />
          <FilterSelect
            label={t('admin.polls.columns.type')}
            allLabel={t('admin.polls.allTypes')}
            value={type === undefined ? undefined : String(type)}
            options={Object.entries(typeLabels).map(([value, key]) => ({ value, label: t(key) }))}
            onChange={(value) => setParam('type', value)}
          />
        </div>
        {can('polls.create.any') && (
          <Button asChild className="lg:ml-auto">
            <Link
              to={cityId ? `/admin/polls/new?city=${cityId}` : '/admin/polls/new'}
              state={{ listSearch }}
            >
              <Plus />
              {t('admin.polls.create')}
            </Link>
          </Button>
        )}
      </div>

      {actionError && <ActionErrorAlert message={t(actionErrorKey(actionError))} />}

      <AdminListContent
        data={data}
        error={error}
        isPending={isPending}
        isPlaceholderData={isPlaceholderData}
        page={page}
        pageSize={ADMIN_POLLS_PAGE_SIZE}
        onPageChange={goToPage}
        loadFailedLabel={t('admin.polls.loadFailed')}
        emptyLabel={t('admin.polls.empty')}
        head={
          <>
            <TableHead>{t('admin.polls.columns.title')}</TableHead>
            <TableHead className="hidden md:table-cell">{t('admin.polls.columns.city')}</TableHead>
            <TableHead className="hidden lg:table-cell">{t('admin.polls.columns.type')}</TableHead>
            <TableHead>{t('admin.polls.columns.status')}</TableHead>
            <TableHead className="hidden sm:table-cell">
              {t('admin.polls.columns.endsAt')}
            </TableHead>
            <TableHead className="hidden text-right lg:table-cell">
              {t('admin.polls.columns.budget')}
            </TableHead>
            {hasActions && (
              <TableHead className="w-12">
                <span className="sr-only">{t('admin.polls.columns.actions')}</span>
              </TableHead>
            )}
          </>
        }
      >
        {data?.items?.map((poll) => (
          <PollRow
            key={poll.id}
            poll={poll}
            cityTitle={cityTitles.get(poll.cityId ?? '')}
            permissions={permissions}
            listSearch={listSearch}
            disabled={isMutating}
            onAction={(actionType) => setPendingAction({ type: actionType, poll })}
          />
        ))}
      </AdminListContent>

      <ConfirmDialog
        content={
          dialog && pendingAction
            ? {
                title: t(dialog.title, { title: pendingAction.poll.title ?? '' }),
                description: t(dialog.description),
                confirmLabel: t(dialog.confirm),
                destructive: pendingAction.type !== 'resume',
              }
            : null
        }
        onConfirm={confirmAction}
        onCancel={() => setPendingAction(null)}
      />
    </div>
  )
}

interface PollRowProps {
  poll: Poll
  cityTitle: string | undefined
  permissions: PollPermissions
  listSearch: string
  disabled: boolean
  onAction: (type: PendingAction['type']) => void
}

function PollRow({
  poll,
  cityTitle,
  permissions,
  listSearch,
  disabled,
  onAction,
}: Readonly<PollRowProps>) {
  const { t, i18n } = useTranslation()
  const statusKey = statusLabels[poll.status as keyof typeof statusLabels]
  const canSuspend = permissions.changeStatus && poll.status === ACTIVE
  const canResume = permissions.changeStatus && poll.status === SUSPENDED
  const hasActions = Object.values(permissions).some(Boolean)
  const editPath = `/admin/polls/${poll.id}/edit`
  const endsAt = poll.endsAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(new Date(poll.endsAt))
    : null

  return (
    <TableRow>
      <TableCell className="font-medium whitespace-normal">
        {permissions.update ? (
          <Link
            to={editPath}
            state={{ listSearch }}
            className="underline-offset-4 outline-none hover:underline focus-visible:underline"
          >
            {poll.title}
          </Link>
        ) : (
          poll.title
        )}
      </TableCell>
      <TableCell className="hidden text-muted-foreground md:table-cell">{cityTitle}</TableCell>
      <TableCell className="hidden lg:table-cell">
        <PollTypeBadge type={poll.type} />
      </TableCell>
      <TableCell>
        {statusKey && (
          <Badge variant={poll.status === ACTIVE ? 'secondary' : 'outline'}>{t(statusKey)}</Badge>
        )}
      </TableCell>
      <TableCell className="hidden text-muted-foreground sm:table-cell">{endsAt}</TableCell>
      <TableCell className="hidden text-right text-muted-foreground tabular-nums lg:table-cell">
        {new Intl.NumberFormat(i18n.language).format(poll.budgetAmount ?? 0)}
      </TableCell>
      {hasActions && (
        <TableCell>
          {(permissions.update || canSuspend || canResume || permissions.delete) && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  disabled={disabled}
                  aria-label={t('admin.polls.actionsFor', { title: poll.title ?? '' })}
                >
                  <Ellipsis />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {permissions.update && (
                  <DropdownMenuItem asChild>
                    <Link to={editPath} state={{ listSearch }}>
                      <Pencil />
                      {t('admin.polls.edit')}
                    </Link>
                  </DropdownMenuItem>
                )}
                {canSuspend && (
                  <DropdownMenuItem onSelect={() => onAction('suspend')}>
                    <Pause />
                    {t('admin.polls.suspend')}
                  </DropdownMenuItem>
                )}
                {canResume && (
                  <DropdownMenuItem onSelect={() => onAction('resume')}>
                    <Play />
                    {t('admin.polls.resume')}
                  </DropdownMenuItem>
                )}
                {permissions.delete && (
                  <DropdownMenuItem variant="destructive" onSelect={() => onAction('delete')}>
                    <Trash2 />
                    {t('admin.polls.delete')}
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
