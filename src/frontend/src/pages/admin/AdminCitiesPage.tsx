import { CircleAlert, Ellipsis, Power, PowerOff, Search, Trash2 } from 'lucide-react'
import { type ReactNode, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/api/errors'
import type { components } from '@/api/schema/polls'
import { ADMIN_CITIES_PAGE_SIZE, useAdminCities } from '@/admin/cities/useAdminCities'
import { useChangeCityStatus, useDeleteCity } from '@/admin/cities/useCityMutations'
import { usePermissions } from '@/auth/usePermissions'
import { Pagination } from '@/components/Pagination'
import { Alert, AlertTitle } from '@/components/ui/alert'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useListSearchParams } from '@/lib/useListSearchParams'

type City = components['schemas']['CityDto']
type CityStatus = components['schemas']['CityStatus']

const ACTIVE: CityStatus = 1
const INACTIVE: CityStatus = 2
const ALL = 'all'

const statusFilters = [
  { value: ALL, key: 'admin.cities.status.all' },
  { value: String(ACTIVE), key: 'admin.cities.status.active' },
  { value: String(INACTIVE), key: 'admin.cities.status.inactive' },
] as const

type PendingAction = { type: 'activate' | 'deactivate' | 'delete'; city: City }

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
  const { searchParams, search, page, searchInput, setSearchInput, setParam, goToPage } =
    useListSearchParams()
  const status = readStatus(searchParams.get('status'))
  const { data, error, isPending, isPlaceholderData } = useAdminCities({ search, status, page })
  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / ADMIN_CITIES_PAGE_SIZE))
  const changeStatus = useChangeCityStatus()
  const deleteCity = useDeleteCity()
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null)

  const permissions = {
    changeStatus: can('cities.changestatus.any'),
    delete: can('cities.delete.any'),
  }
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

  let content: ReactNode
  if (isPending) {
    content = <TableSkeleton />
  } else if (error) {
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{t('admin.cities.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else if (data.items?.length) {
    content = (
      <>
        <div
          className="rounded-lg border transition-opacity data-[stale=true]:opacity-60"
          data-stale={isPlaceholderData}
        >
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t('admin.cities.columns.title')}</TableHead>
                <TableHead>{t('admin.cities.columns.status')}</TableHead>
                <TableHead className="hidden md:table-cell">
                  {t('admin.cities.columns.coordinates')}
                </TableHead>
                <TableHead className="hidden sm:table-cell">
                  {t('admin.cities.columns.createdAt')}
                </TableHead>
                {(permissions.changeStatus || permissions.delete) && (
                  <TableHead className="w-12">
                    <span className="sr-only">{t('admin.cities.columns.actions')}</span>
                  </TableHead>
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.items.map((city) => (
                <CityRow
                  key={city.id}
                  city={city}
                  canChangeStatus={permissions.changeStatus}
                  canDelete={permissions.delete}
                  disabled={isMutating}
                  onAction={(type) => setPendingAction({ type, city })}
                />
              ))}
            </TableBody>
          </Table>
        </div>
        <Pagination
          page={page}
          totalPages={totalPages}
          hasPrevious={Boolean(data.hasPreviousPage)}
          hasNext={Boolean(data.hasNextPage)}
          disabled={isPlaceholderData}
          onPageChange={goToPage}
        />
      </>
    )
  } else {
    content = (
      <p className="text-muted-foreground">
        {search ? t('admin.cities.noMatches', { search }) : t('admin.cities.empty')}
      </p>
    )
  }

  const dialog = pendingAction ? dialogKeys[pendingAction.type] : null

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-2 sm:flex-row">
        <div className="relative sm:max-w-sm sm:flex-1">
          <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            type="search"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder={t('admin.cities.searchPlaceholder')}
            aria-label={t('admin.cities.searchPlaceholder')}
            className="pl-8"
          />
        </div>
        <Select
          value={status === undefined ? ALL : String(status)}
          onValueChange={(value) => setParam('status', value === ALL ? undefined : value)}
        >
          <SelectTrigger className="sm:w-44" aria-label={t('admin.cities.columns.status')}>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {statusFilters.map((filter) => (
              <SelectItem key={filter.value} value={filter.value}>
                {t(filter.key)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {actionError && (
        <Alert variant="destructive">
          <CircleAlert />
          <AlertTitle>{t(actionErrorKey(actionError))}</AlertTitle>
        </Alert>
      )}

      {content}

      <AlertDialog
        open={pendingAction !== null}
        onOpenChange={(open) => {
          if (!open) {
            setPendingAction(null)
          }
        }}
      >
        {dialog && pendingAction && (
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>
                {t(dialog.title, { title: pendingAction.city.title ?? '' })}
              </AlertDialogTitle>
              <AlertDialogDescription>{t(dialog.description)}</AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>{t('admin.cancel')}</AlertDialogCancel>
              <AlertDialogAction
                variant={pendingAction.type === 'activate' ? 'default' : 'destructive'}
                onClick={confirmAction}
              >
                {t(dialog.confirm)}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        )}
      </AlertDialog>
    </div>
  )
}

interface CityRowProps {
  city: City
  canChangeStatus: boolean
  canDelete: boolean
  disabled: boolean
  onAction: (type: PendingAction['type']) => void
}

function CityRow({ city, canChangeStatus, canDelete, disabled, onAction }: Readonly<CityRowProps>) {
  const { t, i18n } = useTranslation()
  const isActive = city.status === ACTIVE
  const number = new Intl.NumberFormat(i18n.language, { maximumFractionDigits: 4 })
  const createdAt = city.createdAt
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
        new Date(city.createdAt),
      )
    : null

  return (
    <TableRow>
      <TableCell className="font-medium">{city.title}</TableCell>
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
      {(canChangeStatus || canDelete) && (
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
              {canChangeStatus && (
                <DropdownMenuItem onSelect={() => onAction(isActive ? 'deactivate' : 'activate')}>
                  {isActive ? <PowerOff /> : <Power />}
                  {isActive ? t('admin.cities.deactivate') : t('admin.cities.activate')}
                </DropdownMenuItem>
              )}
              {canDelete && (
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

function TableSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: 5 }, (_, index) => (
        <Skeleton key={index} className="h-10 w-full" />
      ))}
    </div>
  )
}
