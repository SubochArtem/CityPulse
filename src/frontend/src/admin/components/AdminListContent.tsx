import { CircleAlert } from 'lucide-react'
import type { ReactNode } from 'react'
import { Pagination } from '@/components/Pagination'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Table, TableBody, TableHeader, TableRow } from '@/components/ui/table'
import { TableSkeleton } from './TableSkeleton'

interface PagedList {
  items?: readonly unknown[] | null
  totalCount?: number
  hasNextPage?: boolean
  hasPreviousPage?: boolean
}

interface AdminListContentProps {
  data: PagedList | undefined
  error: Error | null
  isPending: boolean
  isPlaceholderData: boolean
  page: number
  pageSize: number
  onPageChange: (page: number) => void
  loadFailedLabel: string
  emptyLabel: string
  head: ReactNode
  children: ReactNode
}

export function AdminListContent({
  data,
  error,
  isPending,
  isPlaceholderData,
  page,
  pageSize,
  onPageChange,
  loadFailedLabel,
  emptyLabel,
  head,
  children,
}: Readonly<AdminListContentProps>) {
  if (isPending) {
    return <TableSkeleton />
  }

  if (error || !data) {
    return (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{loadFailedLabel}</AlertTitle>
      </Alert>
    )
  }

  if (!data.items?.length) {
    return <p className="text-muted-foreground">{emptyLabel}</p>
  }

  const totalPages = Math.max(1, Math.ceil((data.totalCount ?? 0) / pageSize))

  return (
    <>
      <div
        className="rounded-lg border transition-opacity data-[stale=true]:opacity-60"
        data-stale={isPlaceholderData}
      >
        <Table>
          <TableHeader>
            <TableRow>{head}</TableRow>
          </TableHeader>
          <TableBody>{children}</TableBody>
        </Table>
      </div>
      <Pagination
        page={page}
        totalPages={totalPages}
        hasPrevious={Boolean(data.hasPreviousPage)}
        hasNext={Boolean(data.hasNextPage)}
        disabled={isPlaceholderData}
        onPageChange={onPageChange}
      />
    </>
  )
}
