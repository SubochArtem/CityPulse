import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

interface PaginationProps {
  page: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
  disabled?: boolean
  onPageChange: (page: number) => void
}

export function Pagination({
  page,
  totalPages,
  hasPrevious,
  hasNext,
  disabled,
  onPageChange,
}: Readonly<PaginationProps>) {
  const { t } = useTranslation()

  if (totalPages <= 1 && page <= 1) {
    return null
  }

  return (
    <nav className="flex items-center justify-between gap-4">
      <Button
        variant="outline"
        size="sm"
        disabled={!hasPrevious || disabled}
        onClick={() => onPageChange(page - 1)}
      >
        <ChevronLeft />
        {t('pagination.previous')}
      </Button>
      <span className="text-sm text-muted-foreground">
        {t('pagination.pageOf', { page, total: totalPages })}
      </span>
      <Button
        variant="outline"
        size="sm"
        disabled={!hasNext || disabled}
        onClick={() => onPageChange(page + 1)}
      >
        {t('pagination.next')}
        <ChevronRight />
      </Button>
    </nav>
  )
}
