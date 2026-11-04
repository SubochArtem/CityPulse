import { useAuth0 } from '@auth0/auth0-react'
import { Ellipsis, Trash2, UserCheck, UserX } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/users'
import { useAdminCityOptions } from '@/admin/cities/useAdminCityOptions'
import { ActionErrorAlert } from '@/admin/components/ActionErrorAlert'
import { AdminListContent } from '@/admin/components/AdminListContent'
import { AdminSearchInput } from '@/admin/components/AdminSearchInput'
import { ConfirmDialog } from '@/admin/components/ConfirmDialog'
import { FilterSelect } from '@/admin/components/FilterSelect'
import { ADMIN_USERS_PAGE_SIZE, useAdminUsers } from '@/admin/users/useAdminUsers'
import { useActivateUser, useDeactivateUser, useDeleteUser } from '@/admin/users/useUserMutations'
import { readInternalUserId } from '@/auth/claims'
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

type User = components['schemas']['GetUserDto']
type UserAction = 'activate' | 'deactivate' | 'delete'

const ACTIVE = 1
const INACTIVE = 2

const statusLabels = {
  1: 'admin.users.status.active',
  2: 'admin.users.status.inactive',
  3: 'admin.users.status.suspended',
} as const

interface UserPermissions {
  activate: boolean
  deactivate: boolean
  delete: boolean
}

function actionErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.users.notFound'
  }
  return 'admin.users.actionFailed'
}

export function AdminUsersPage() {
  const { t } = useTranslation()
  const { user: authUser } = useAuth0()
  const currentUserId = readInternalUserId(authUser)
  const { can } = usePermissions()
  const { searchParams, search, page, searchInput, setSearchInput, setParam, goToPage } =
    useListSearchParams()
  const cityId = searchParams.get('city') ?? undefined
  const { data, error, isPending, isPlaceholderData } = useAdminUsers({ search, cityId, page })
  const { data: cities } = useAdminCityOptions()
  const cityTitles = new Map(cities?.map((city) => [city.id, city.title]))
  const mutations = {
    activate: useActivateUser(),
    deactivate: useDeactivateUser(),
    delete: useDeleteUser(),
  }
  const [pendingAction, setPendingAction] = useState<{ type: UserAction; user: User } | null>(null)

  const permissions: UserPermissions = {
    activate: can('users.activate.any'),
    deactivate: can('users.deactivate.any'),
    delete: can('users.delete.any'),
  }
  const hasActions = Object.values(permissions).some(Boolean)
  const mutationList = Object.values(mutations)
  const actionError = mutationList.find((mutation) => mutation.error)?.error
  const isMutating = mutationList.some((mutation) => mutation.isPending)

  const confirmAction = () => {
    const id = pendingAction?.user.id
    if (!pendingAction || !id) {
      return
    }
    for (const mutation of mutationList) {
      mutation.reset()
    }
    mutations[pendingAction.type].mutate(id)
    setPendingAction(null)
  }

  const pendingType = pendingAction?.type

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
        <AdminSearchInput
          value={searchInput}
          onChange={setSearchInput}
          placeholder={t('admin.users.searchPlaceholder')}
        />
        {cities && (
          <FilterSelect
            label={t('admin.users.columns.city')}
            allLabel={t('admin.users.allCities')}
            value={cityId}
            options={cities.map((city) => ({ value: city.id, label: city.title }))}
            onChange={(value) => setParam('city', value)}
          />
        )}
      </div>

      {actionError && <ActionErrorAlert message={t(actionErrorKey(actionError))} />}

      <AdminListContent
        data={data}
        error={error}
        isPending={isPending}
        isPlaceholderData={isPlaceholderData}
        page={page}
        pageSize={ADMIN_USERS_PAGE_SIZE}
        onPageChange={goToPage}
        loadFailedLabel={t('admin.users.loadFailed')}
        emptyLabel={search ? t('admin.users.noMatches', { search }) : t('admin.users.empty')}
        head={
          <>
            <TableHead>{t('admin.users.columns.nickname')}</TableHead>
            <TableHead className="hidden md:table-cell">{t('admin.users.columns.city')}</TableHead>
            <TableHead>{t('admin.users.columns.status')}</TableHead>
            <TableHead className="hidden sm:table-cell">
              {t('admin.users.columns.createdAt')}
            </TableHead>
            {hasActions && (
              <TableHead className="w-12">
                <span className="sr-only">{t('admin.users.columns.actions')}</span>
              </TableHead>
            )}
          </>
        }
      >
        {data?.items?.map((user) => (
          <UserRow
            key={user.id}
            user={user}
            cityTitle={user.cityId ? cityTitles.get(user.cityId) : undefined}
            isCurrentUser={user.id === currentUserId}
            permissions={permissions}
            disabled={isMutating}
            onAction={(type) => setPendingAction({ type, user })}
          />
        ))}
      </AdminListContent>

      <ConfirmDialog
        content={
          pendingType && pendingAction
            ? {
                title: t(`admin.users.${pendingType}Title`, {
                  nickname: pendingAction.user.nickname ?? '',
                }),
                description: t(`admin.users.${pendingType}Description`),
                confirmLabel: t(`admin.users.${pendingType}`),
                destructive: pendingType !== 'activate',
              }
            : null
        }
        onConfirm={confirmAction}
        onCancel={() => setPendingAction(null)}
      />
    </div>
  )
}

interface UserRowProps {
  user: User
  cityTitle: string | undefined
  isCurrentUser: boolean
  permissions: UserPermissions
  disabled: boolean
  onAction: (type: UserAction) => void
}

function UserRow({
  user,
  cityTitle,
  isCurrentUser,
  permissions,
  disabled,
  onAction,
}: Readonly<UserRowProps>) {
  const { t, i18n } = useTranslation()
  const statusKey = statusLabels[user.accessStatus as keyof typeof statusLabels]
  const canActivate = permissions.activate && !isCurrentUser && user.accessStatus !== ACTIVE
  const canDeactivate = permissions.deactivate && !isCurrentUser && user.accessStatus !== INACTIVE
  const canDelete = permissions.delete && !isCurrentUser
  const hasActions = Object.values(permissions).some(Boolean)
  const createdAt = user.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(user.createdAt),
      )
    : null

  return (
    <TableRow>
      <TableCell className="font-medium">
        {user.nickname}
        {isCurrentUser && (
          <span className="ml-2 font-normal text-muted-foreground">{t('admin.users.you')}</span>
        )}
      </TableCell>
      <TableCell className="hidden text-muted-foreground md:table-cell">
        {user.cityId ? cityTitle : t('admin.users.noCity')}
      </TableCell>
      <TableCell>
        {statusKey && (
          <Badge variant={user.accessStatus === ACTIVE ? 'secondary' : 'outline'}>
            {t(statusKey)}
          </Badge>
        )}
      </TableCell>
      <TableCell className="hidden text-muted-foreground sm:table-cell">{createdAt}</TableCell>
      {hasActions && (
        <TableCell>
          {(canActivate || canDeactivate || canDelete) && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  disabled={disabled}
                  aria-label={t('admin.users.actionsFor', { nickname: user.nickname ?? '' })}
                >
                  <Ellipsis />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {canActivate && (
                  <DropdownMenuItem onSelect={() => onAction('activate')}>
                    <UserCheck />
                    {t('admin.users.activate')}
                  </DropdownMenuItem>
                )}
                {canDeactivate && (
                  <DropdownMenuItem onSelect={() => onAction('deactivate')}>
                    <UserX />
                    {t('admin.users.deactivate')}
                  </DropdownMenuItem>
                )}
                {canDelete && (
                  <DropdownMenuItem variant="destructive" onSelect={() => onAction('delete')}>
                    <Trash2 />
                    {t('admin.users.delete')}
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
