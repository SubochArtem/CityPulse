import { ArrowRight, CalendarClock, CircleAlert, Clock, Vote, Wallet } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { components } from '@/api/schema/polls'
import { useCityWithPolls } from './useCityWithPolls'
import { BoxAction } from '@/components/BoxAction'
import { boxCellClass } from '@/components/boxCell'
import { HalftonePoster } from '@/components/HalftonePoster'
import { IconFact } from '@/components/IconFact'
import { TallyMarks } from '@/components/TallyMarks'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { coverImage } from '@/lib/images'
import { cn } from '@/lib/utils'
import { useNow } from '@/lib/useNow'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { pollDaysLeft } from '@/polls/pollTimeline'
import { pollTypeKey } from '@/polls/pollTypes'

type Poll = components['schemas']['PollDto']

const MAX_TALLY = 20

export function CityPolls({ cityId }: Readonly<{ cityId: string }>) {
  const { t } = useTranslation()
  const { data: city, error, isPending } = useCityWithPolls(cityId)
  const [selectedId, setSelectedId] = useState<string>()

  if (isPending) {
    return <Skeleton className="h-96" />
  }
  if (error) {
    return (
      <Alert variant="destructive" className="mx-auto max-w-md">
        <CircleAlert />
        <AlertTitle>{t('home.cityLoadFailed')}</AlertTitle>
      </Alert>
    )
  }

  const polls = [...(city.polls ?? [])].sort((a, b) =>
    (a.endsAt ?? '').localeCompare(b.endsAt ?? ''),
  )
  const selectedIndex = Math.max(
    0,
    polls.findIndex((poll) => poll.id === selectedId),
  )
  const selected = polls[selectedIndex]

  return (
    <section aria-labelledby="city-polls-title" className="paper poster tape px-4 py-12 sm:px-10">
      <h2
        id="city-polls-title"
        className="text-center text-4xl font-medium text-heading [overflow-wrap:anywhere] sm:text-5xl"
      >
        {t('home.decidingNow', { city: city.title ?? '' })}
      </h2>
      {selected ? (
        <>
          {polls.length > 1 && (
            <fieldset className="mx-auto mt-8 grid max-w-3xl min-w-0 border-2 border-heading sm:auto-cols-fr sm:grid-flow-col">
              <legend className="sr-only">
                {t('home.pollsLabel', { city: city.title ?? '' })}
              </legend>
              {polls.map((poll, index) => (
                <button
                  key={poll.id}
                  type="button"
                  aria-pressed={poll.id === selected.id}
                  onClick={() => setSelectedId(poll.id)}
                  className={cn(
                    boxCellClass,
                    'min-w-0 border-b-2 border-heading last:border-b-0 sm:border-b-0 sm:border-l-2 sm:[legend+&]:border-l-0',
                    poll.id === selected.id &&
                      'bg-primary text-primary-foreground hover:bg-primary/85 focus-visible:bg-primary/85',
                  )}
                >
                  <PollLineBadge
                    pollId={poll.id}
                    number={index + 1}
                    className="size-5 text-[0.6rem]"
                  />
                  <span className="truncate">{poll.title}</span>
                </button>
              ))}
            </fieldset>
          )}
          <SelectedPoll poll={selected} number={selectedIndex + 1} />
        </>
      ) : (
        <p className="mt-6 text-center text-muted-foreground">{t('home.noActivePoll')}</p>
      )}
    </section>
  )
}

function SelectedPoll({ poll, number }: Readonly<{ poll: Poll; number: number }>) {
  const { t, i18n } = useTranslation()
  const now = useNow()
  const typeKey = pollTypeKey(poll.type)
  const pollPath = `/polls/${poll.id}`
  const daysLeft = poll.endsAt ? pollDaysLeft(poll.endsAt, now) : undefined

  return (
    <div className="mt-10 grid items-center gap-10 lg:grid-cols-[minmax(0,1fr)_minmax(0,20rem)_minmax(0,1fr)]">
      <div className="flex flex-col items-center gap-3 text-center lg:items-end lg:text-right">
        <span className="bg-coral px-2 py-0.5 font-label text-xs tracking-[0.25em] text-paper uppercase">
          {t('home.openLine', { number })}
        </span>
        <h3 className="text-3xl font-normal text-heading [overflow-wrap:anywhere]">{poll.title}</h3>
        {poll.description && (
          <p className="line-clamp-4 text-sm text-muted-foreground">{poll.description}</p>
        )}
        {daysLeft !== undefined && (
          <span className="flex flex-col items-center gap-1.5 lg:items-end">
            <span className="inline-flex items-center gap-2 font-heading text-lg text-coral">
              <Clock className="size-4" />
              {t('poll.route.daysLeft', { count: daysLeft })}
            </span>
            {daysLeft > 0 && daysLeft <= MAX_TALLY && (
              <TallyMarks count={daysLeft} className="text-heading" />
            )}
          </span>
        )}
      </div>
      <div className="group mx-auto w-full max-w-80">
        <HalftonePoster
          seed={poll.id ?? ''}
          title={poll.title}
          photoUrl={coverImage(poll.images)?.url}
        />
      </div>
      <div className="flex flex-col items-center gap-6 lg:items-start">
        <dl className="flex flex-col gap-5">
          {poll.endsAt && (
            <IconFact
              icon={CalendarClock}
              label={t('home.facts.ends')}
              value={new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
                new Date(poll.endsAt),
              )}
            />
          )}
          {typeKey && <IconFact icon={Vote} label={t('home.facts.type')} value={t(typeKey)} />}
          {poll.budgetAmount ? (
            <IconFact
              icon={Wallet}
              label={t('home.facts.budget')}
              value={new Intl.NumberFormat(i18n.language).format(poll.budgetAmount)}
            />
          ) : null}
        </dl>
        <BoxAction icon={ArrowRight} label={t('poll.open')} to={pollPath} />
      </div>
    </div>
  )
}
