import { useQueryClient } from '@tanstack/react-query'
import { ArrowUp, CircleAlert } from 'lucide-react'
import { type ReactNode, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useCityNames } from '@/cities/useCityNames'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { useColumnCount } from '@/lib/useColumnCount'
import { useNow } from '@/lib/useNow'
import { useAuth } from '@/auth/useAuth'
import { usePublicProfiles } from '@/users/usePublicProfiles'
import { buildStories, buildWall, distribute, type WallItem } from './activityWall'
import { IdeaPoster } from './IdeaPoster'
import { PollFlyer } from './PollFlyer'
import { PollStories } from './PollStories'
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
  const listRef = useRef<HTMLDivElement>(null)
  const columns = useColumnCount()
  const { isAuthenticated, can } = useAuth()
  const canRead = isAuthenticated && can('feedback.read.any')
  const [pollFilter, setPollFilter] = useState<{ cityId: string | null; pollId: string | null }>({
    cityId,
    pollId: null,
  })
  const selectedPoll = pollFilter.cityId === cityId ? pollFilter.pollId : null
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
  const stories = useMemo(() => buildStories(entries), [entries])
  const wall = useMemo(() => buildWall(entries, selectedPoll), [entries, selectedPoll])

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
      <p className="paper poster tape px-6 py-10 text-center text-muted-foreground">
        {t('activity.empty')}
      </p>
    )
  } else {
    content = (
      <>
        <PollStories
          stories={stories}
          selected={selectedPoll}
          cityNames={showCity ? cityNames.data : undefined}
          onSelect={(pollId) => setPollFilter({ cityId, pollId })}
        />
        <div
          ref={listRef}
          className="grid scroll-mt-24 grid-cols-1 items-start gap-7 sm:grid-cols-2 lg:grid-cols-3"
        >
          {distribute(wall, columns).map((column) => (
            <ul key={column.id} className="flex min-w-0 flex-col gap-8">
              {column.items.map((item) => (
                <li key={item.key}>
                  <WallEntry
                    item={item}
                    nicknames={nicknames.data}
                    cityName={showCity ? cityNameOf(item, cityNames.data) : undefined}
                    now={now}
                    fresh={isFresh(item, freshIds)}
                    canRead={canRead}
                  />
                </li>
              ))}
            </ul>
          ))}
        </div>
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
          <Button size="sm" className="shadow-[3px_3px_0_var(--ink)]" onClick={showPending}>
            <ArrowUp />
            {t('activity.newEvents', { count: pending.length })}
          </Button>
        </div>
      )}

      {content}
    </section>
  )
}

interface WallEntryProps {
  item: WallItem
  nicknames: Map<string, string> | undefined
  cityName: string | undefined
  now: number
  fresh: boolean
  canRead: boolean
}

function WallEntry({ item, nicknames, cityName, now, fresh, canRead }: Readonly<WallEntryProps>) {
  if (item.type === 'poll') {
    return <PollFlyer entry={item.entry} cityName={cityName} now={now} fresh={fresh} />
  }
  return (
    <IdeaPoster
      item={item}
      nicknames={nicknames}
      cityName={cityName}
      now={now}
      fresh={fresh}
      canRead={canRead}
    />
  )
}

function cityNameOf(item: WallItem, names: Map<string, string> | undefined) {
  const entry = item.type === 'poll' ? item.entry : item.events[0]
  return entry.cityId ? names?.get(entry.cityId) : undefined
}

function isFresh(item: WallItem, freshIds: ReadonlySet<string>) {
  const events = item.type === 'poll' ? [item.entry] : item.events
  return events.some((event) => Boolean(event.id && freshIds.has(event.id)))
}

function LiveIndicator({ status }: Readonly<{ status: LiveStatus }>) {
  const { t } = useTranslation()

  let label: string
  let toneClass: string
  let dotClass: string
  if (status === 'live') {
    label = t('activity.live')
    toneClass = 'bg-riot text-paper'
    dotClass = 'bg-paper motion-safe:animate-pulse'
  } else if (status === 'connecting') {
    label = t('activity.connecting')
    toneClass = 'bg-ink text-paper'
    dotClass = 'bg-paper/60'
  } else {
    label = t('activity.offline')
    toneClass = 'border-2 border-dashed border-foreground text-foreground'
    dotClass = 'bg-foreground'
  }

  return (
    <output
      className={cn(
        'inline-flex -rotate-2 items-center gap-1.5 px-2 py-0.5 font-label text-xs tracking-[0.15em] uppercase',
        toneClass,
      )}
    >
      <span aria-hidden="true" className={cn('size-2 rounded-full', dotClass)} />
      {label}
    </output>
  )
}

function FeedSkeleton() {
  return (
    <div className="grid grid-cols-1 items-start gap-7 sm:grid-cols-2 lg:grid-cols-3">
      {[0, 1, 2].map((column) => (
        <div
          key={column}
          className={cn(
            'flex-col gap-8',
            column === 0 ? 'flex' : 'hidden sm:flex',
            column === 2 && 'sm:hidden lg:flex',
          )}
        >
          {[0, 1].map((row) => (
            <div key={row} className="paper poster flex flex-col gap-3 p-3">
              <Skeleton className={cn('w-full', (column + row) % 2 ? 'h-40' : 'h-56')} />
              <Skeleton className="h-6 w-3/4" />
              <Skeleton className="h-3 w-1/2" />
            </div>
          ))}
        </div>
      ))}
    </div>
  )
}
