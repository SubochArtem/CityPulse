import { Ellipsis, ListChecks, Pencil, Plus, Power, PowerOff, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation } from 'react-router'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { ADMIN_CITIES_PAGE_SIZE, useAdminCities } from '@/admin/cities/useAdminCities'
import { useChangeCityStatus, useDeleteCity } from '@/admin/cities/useCityMutations'
import { AdminListContent } from '@/admin/components/AdminListContent'
import { ActionErrorAlert } from '@/admin/components/ActionErrorAlert'
import { AdminSearchInput } from '@/admin/components/AdminSearchInput'
import { ConfirmDialog } from '@/admin/components/ConfirmDialog'
import { FilterSelect } from '@/admin/components/FilterSelect'
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

type City = components['schemas']['CityDto']
type CityStatus = components['schemas']['CityStatus']

const ACTIVE: CityStatus = 1
const INACTIVE: CityStatus = 2

const statusFilters = [
  { value: String(ACTIVE), key: 'admin.cities.status.active' },
  { value: String(INACTIVE), key: 'admin.cities.status.inactive' },
] as const

type PendingAction = { type: 'activate' | 'deactivate' | 'delete'; city: City }

interface CityPermissions {
  update: boolean
  polls: boolean
  changeStatus: boolean
  delete: boolean
}

const dialogKeys = {
  activate: {
    title: 'admin.cities.activateTitle',
    description: 'admin.cities.activateDescription',
    confirm: 'admin.cities.activate',
  },
  deactivate: {
    title: 'admin.cities.deactivateTitle',
    description: 'admin.cities.deactivateDescription',
    confirm: 'admin.cities.deactivate',
  },
  delete: {
    title: 'admin.cities.deleteTitle',
    description: 'admin.cities.deleteDescription',
    confirm: 'admin.cities.delete',
  },
} as const

function readStatus(value: string | null): CityStatus | undefined {
  if (value === String(ACTIVE)) {
    return ACTIVE
  }
  if (value === String(INACTIVE)) {
    return INACTIVE
  }
  return undefined
}

function actionErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.cities.notFound'
  }
  return 'admin.cities.actionFailed'
}

export function AdminCitiesPage() {
  const { t } = useTranslation()
  const { can } = usePermissions()
  const { search: listSearch } = useLocation()
  const { searchParams, search, page, searchInput, setSearchInput, setParam, goToPage } =
    useListSearchParams()
  const status = readStatus(searchParams.get('status'))
  const { data, error, isPending, isPlaceholderData } = useAdminCities({ search, status, page })
  const changeStatus = useChangeCityStatus()
  const deleteCity = useDeleteCity()
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null)

  const permissions: CityPermissions = {
    update: can('cities.update.any'),
    polls: can('polls.read.any'),
    changeStatus: can('cities.changestatus.any'),
    delete: can('cities.delete.any'),
  }
  const hasActions = Object.values(permissions).some(Boolean)
  const actionError = changeStatus.error ?? deleteCity.error
  const isMutating = changeStatus.isPending || deleteCity.isPending

  const confirmAction = () => {
    const id = pendingAction?.city.id
    if (!pendingAction || !id) {
      return
    }
    changeStatus.reset()
    deleteCity.reset()
    if (pendingAction.type === 'delete') {
      deleteCity.mutate(id)
    } else {
      changeStatus.mutate({ id, status: pendingAction.type === 'activate' ? ACTIVE : INACTIVE })
    }
    setPendingAction(null)
  }

  const dialog = pendingAction ? dialogKeys[pendingAction.type] : null

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
        <AdminSearchInput
          value={searchInput}
          onChange={setSearchInput}
          placeholder={t('admin.cities.searchPlaceholder')}
        />
        <FilterSelect
          label={t('admin.cities.columns.status')}
          allLabel={t('admin.cities.status.all')}
          value={status === undefined ? undefined : String(status)}
          options={statusFilters.map((filter) => ({ value: filter.value, label: t(filter.key) }))}
          onChange={(value) => setParam('status', value)}
        />
        {can('cities.create.any') && (
          <Button asChild className="sm:ml-auto">
            <Link to="/admin/cities/new" state={{ listSearch }}>
              <Plus />
              {t('admin.cities.create')}
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
        pageSize={ADMIN_CITIES_PAGE_SIZE}
        onPageChange={goToPage}
        loadFailedLabel={t('admin.cities.loadFailed')}
        emptyLabel={search ? t('admin.cities.noMatches', { search }) : t('admin.cities.empty')}
        head={
          <>
            <TableHead>{t('admin.cities.columns.title')}</TableHead>
            <TableHead>{t('admin.cities.columns.status')}</TableHead>
            <TableHead className="hidden md:table-cell">
              {t('admin.cities.columns.coordinates')}
            </TableHead>
            <TableHead className="hidden sm:table-cell">
              {t('admin.cities.columns.createdAt')}
            </TableHead>
            {hasActions && (
              <TableHead className="w-12">
                <span className="sr-only">{t('admin.cities.columns.actions')}</span>
              </TableHead>
            )}
          </>
        }
      >
        {data?.items?.map((city) => (
          <CityRow
            key={city.id}
            city={city}
            permissions={permissions}
            listSearch={listSearch}
            disabled={isMutating}
            onAction={(type) => setPendingAction({ type, city })}
          />
        ))}
      </AdminListContent>

      <ConfirmDialog
        content={
          dialog && pendingAction
            ? {
                title: t(dialog.title, { title: pendingAction.city.title ?? '' }),
                description: t(dialog.description),
                confirmLabel: t(dialog.confirm),
                destructive: pendingAction.type !== 'activate',
              }
            : null
        }
        onConfirm={confirmAction}
        onCancel={() => setPendingAction(null)}
      />
    </div>
  )
}

interface CityRowProps {
  city: City
  permissions: CityPermissions
  listSearch: string
  disabled: boolean
  onAction: (type: PendingAction['type']) => void
}

function CityRow({ city, permissions, listSearch, disabled, onAction }: Readonly<CityRowProps>) {
  const { t, i18n } = useTranslation()
  const isActive = city.status === ACTIVE
  const number = new Intl.NumberFormat(i18n.language, { maximumFractionDigits: 4 })
  const editPath = `/admin/cities/${city.id}/edit`
  const hasActions = Object.values(permissions).some(Boolean)
  const createdAt = city.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(city.createdAt),
      )
    : null

  return (
    <TableRow>
      <TableCell className="font-medium">
        {permissions.update ? (
          <Link
            to={editPath}
            state={{ listSearch }}
            className="underline-offset-4 outline-none hover:underline focus-visible:underline"
          >
            {city.title}
          </Link>
        ) : (
          city.title
        )}
      </TableCell>
      <TableCell>
        <Badge variant={isActive ? 'secondary' : 'outline'}>
          {isActive ? t('admin.cities.status.active') : t('admin.cities.status.inactive')}
        </Badge>
      </TableCell>
      <TableCell className="hidden text-muted-foreground tabular-nums md:table-cell">
        {number.format(city.coordinates.latitude ?? 0)},{' '}
        {number.format(city.coordinates.longitude ?? 0)}
      </TableCell>
      <TableCell className="hidden text-muted-foreground sm:table-cell">{createdAt}</TableCell>
      {hasActions && (
        <TableCell>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                size="icon-sm"
                disabled={disabled}
                aria-label={t('admin.cities.actionsFor', { title: city.title ?? '' })}
              >
                <Ellipsis />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {permissions.update && (
                <DropdownMenuItem asChild>
                  <Link to={editPath} state={{ listSearch }}>
                    <Pencil />
                    {t('admin.cities.edit')}
                  </Link>
                </DropdownMenuItem>
              )}
              {permissions.polls && (
                <DropdownMenuItem asChild>
                  <Link to={`/admin/polls?city=${city.id}`}>
                    <ListChecks />
                    {t('admin.cities.polls')}
                  </Link>
                </DropdownMenuItem>
              )}
              {permissions.changeStatus && (
                <DropdownMenuItem onSelect={() => onAction(isActive ? 'deactivate' : 'activate')}>
                  {isActive ? <PowerOff /> : <Power />}
                  {isActive ? t('admin.cities.deactivate') : t('admin.cities.activate')}
                </DropdownMenuItem>
              )}
              {permissions.delete && (
                <DropdownMenuItem variant="destructive" onSelect={() => onAction('delete')}>
                  <Trash2 />
                  {t('admin.cities.delete')}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </TableCell>
      )}
    </TableRow>
  )
}
