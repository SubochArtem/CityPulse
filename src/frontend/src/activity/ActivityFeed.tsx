import { useQueryClient } from '@tanstack/react-query'
import { ArrowUp, CircleAlert } from 'lucide-react'
import { type ReactNode, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useCityNames } from '@/cities/useCityNames'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { useNow } from '@/lib/useNow'
import { usePublicProfiles } from '@/users/usePublicProfiles'
import { ActivityItem } from './ActivityItem'
import { type Activity, activityKey, prependActivity, useActivityFeed } from './useActivityFeed'
import { type LiveStatus, useActivityLive } from './useActivityLive'

interface ActivityFeedProps {
  cityId: string | null
  actions?: ReactNode
}

interface Incoming {
  cityId: string | null
  items: Activity[]
}

export function ActivityFeed({ cityId, actions }: Readonly<ActivityFeedProps>) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const now = useNow()
  const listRef = useRef<HTMLOListElement>(null)
  const feed = useActivityFeed(cityId)
  const entries = useMemo(
    () => feed.data?.pages.flatMap((page) => page.items ?? []) ?? [],
    [feed.data],
  )
  const [incoming, setIncoming] = useState<Incoming>({ cityId, items: [] })
  const [freshIds, setFreshIds] = useState<ReadonlySet<string>>(new Set())

  const status = useActivityLive(cityId, {
    onAdded: (entry) => {
      if (!feed.data) {
        void queryClient.invalidateQueries({ queryKey: activityKey(cityId) })
        return
      }
      if (entries.length === 0) {
        prependActivity(queryClient, cityId, [entry])
        return
      }
      setIncoming((current) => {
        const items = current.cityId === cityId ? current.items : []
        return items.some((item) => item.id === entry.id)
          ? current
          : { cityId, items: [entry, ...items] }
      })
    },
    onInvalidated: () => {
      setIncoming({ cityId, items: [] })
      void queryClient.invalidateQueries({ queryKey: activityKey(cityId) })
    },
  })

  const pending = (incoming.cityId === cityId ? incoming.items : []).filter(
    (item) => !entries.some((entry) => entry.id === item.id),
  )
  const showCity = cityId === null
  const nicknames = usePublicProfiles(entries.map((entry) => entry.userId ?? undefined))
  const cityNames = useCityNames(showCity)

  const showPending = () => {
    prependActivity(queryClient, cityId, pending)
    setFreshIds(new Set(pending.flatMap((item) => (item.id ? [item.id] : []))))
    setIncoming({ cityId, items: [] })
    listRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }

  let content: ReactNode
  if (feed.isPending) {
    content = <FeedSkeleton />
  } else if (feed.error) {
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{t('activity.loadFailed')}</AlertTitle>
      </Alert>
    )
  } else if (entries.length === 0) {
    content = (
      <p className="border bg-card px-6 py-10 text-center text-muted-foreground">
        {t('activity.empty')}
      </p>
    )
  } else {
    content = (
      <>
        <ol ref={listRef} className="scroll-mt-24 border bg-card">
          {entries.map((entry) => (
            <ActivityItem
              key={entry.id}
              entry={entry}
              nickname={entry.userId ? nicknames.data?.get(entry.userId) : undefined}
              cityName={showCity && entry.cityId ? cityNames.data?.get(entry.cityId) : undefined}
              now={now}
              fresh={Boolean(entry.id && freshIds.has(entry.id))}
            />
          ))}
        </ol>
        {feed.hasNextPage && (
          <div className="flex justify-center">
            <Button
              variant="outline"
              disabled={feed.isFetchingNextPage}
              onClick={() => void feed.fetchNextPage()}
            >
              {feed.isFetchingNextPage ? t('common.loading') : t('activity.loadMore')}
            </Button>
          </div>
        )}
      </>
    )
  }

  return (
    <section aria-labelledby="activity-title" className="space-y-5">
      <div className="flex flex-col items-center gap-4 text-center">
        <h2 id="activity-title" className="text-4xl font-medium text-heading sm:text-5xl">
          {t('activity.title')}
        </h2>
        <LiveIndicator status={status} />
        {actions}
      </div>

      {pending.length > 0 && (
        <div className="sticky top-3 z-10 flex justify-center">
          <Button size="sm" className="rounded-full shadow-md" onClick={showPending}>
            <ArrowUp />
            {t('activity.newEvents', { count: pending.length })}
          </Button>
        </div>
      )}

      {content}
    </section>
  )
}

function LiveIndicator({ status }: Readonly<{ status: LiveStatus }>) {
  const { t } = useTranslation()

  let label: string
  let dotClass: string
  if (status === 'live') {
    label = t('activity.live')
    dotClass = 'bg-(--line-2) motion-safe:animate-pulse'
  } else if (status === 'connecting') {
    label = t('activity.connecting')
    dotClass = 'bg-muted-foreground'
  } else {
    label = t('activity.offline')
    dotClass = 'bg-destructive'
  }

  return (
    <output className="inline-flex items-center gap-1.5 rounded-full border bg-card px-3 py-0.5 font-label text-xs tracking-[0.15em] text-muted-foreground uppercase">
      <span aria-hidden="true" className={cn('size-2 rounded-full', dotClass)} />
      {label}
    </output>
  )
}

function FeedSkeleton() {
  return (
    <ul className="border bg-card">
      {Array.from({ length: 5 }, (_, index) => (
        <li
          key={index}
          className="grid grid-cols-[1.75rem_1fr] gap-x-3 border-b px-4 py-4 last:border-b-0 sm:px-6"
        >
          <Skeleton className="mx-auto size-3 rounded-full" />
          <div className="space-y-2">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-1/3" />
          </div>
        </li>
      ))}
    </ul>
  )
}
