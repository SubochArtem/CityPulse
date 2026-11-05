import { Skeleton } from '@/components/ui/skeleton'
import { adminSheetClass } from './adminSheet'
import { cn } from '@/lib/utils'

export function FormSkeleton() {
  return (
    <div className={cn(adminSheetClass, 'max-w-2xl space-y-6 px-4 py-8 sm:px-8')}>
      <Skeleton className="h-8 w-full" />
      <Skeleton className="h-24 w-full" />
      <div className="grid gap-4 sm:grid-cols-2">
        <Skeleton className="h-8 w-full" />
        <Skeleton className="h-8 w-full" />
      </div>
      <Skeleton className="aspect-video w-1/3" />
    </div>
  )
}
