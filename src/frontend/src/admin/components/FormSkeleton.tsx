import { Skeleton } from '@/components/ui/skeleton'

export function FormSkeleton() {
  return (
    <div className="max-w-2xl space-y-6 border bg-card px-4 py-8 sm:px-8">
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
