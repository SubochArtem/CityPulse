import { useTranslation } from 'react-i18next'
import { coverImage } from '@/lib/images'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { usePoll } from '@/polls/usePoll'
import type { PollStory } from './activityWall'

const ACTIVE_STATUS = 1

interface PollStoriesProps {
  stories: PollStory[]
  selected: string | null
  cityNames: Map<string, string> | undefined
  onSelect: (pollId: string | null) => void
}

export function PollStories({
  stories,
  selected,
  cityNames,
  onSelect,
}: Readonly<PollStoriesProps>) {
  const { t } = useTranslation()

  if (stories.length < 2) {
    return null
  }

  return (
    <ul
      aria-label={t('activity.stories')}
      className="-mx-1 flex snap-x gap-5 overflow-x-auto px-1 pt-2 pb-4 [scrollbar-width:thin]"
    >
      {stories.map((story, index) => (
        <li key={story.pollId} className="snap-start">
          <Story
            story={story}
            index={index}
            cityName={story.cityId ? cityNames?.get(story.cityId) : undefined}
            selected={selected === story.pollId}
            onToggle={() => onSelect(selected === story.pollId ? null : story.pollId)}
          />
        </li>
      ))}
    </ul>
  )
}

interface StoryProps {
  story: PollStory
  index: number
  cityName: string | undefined
  selected: boolean
  onToggle: () => void
}

function Story({ story, index, cityName, selected, onToggle }: Readonly<StoryProps>) {
  const { t } = useTranslation()
  const { data: poll, isError } = usePoll(story.pollId, !story.closed)
  const photo = coverImage(poll?.images)?.url
  const closed = story.closed || isError || (poll !== undefined && poll.status !== ACTIVE_STATUS)
  const title = poll?.title ?? story.title

  return (
    <button
      type="button"
      aria-pressed={selected}
      aria-label={t('activity.storyLabel', { poll: title })}
      onClick={onToggle}
      style={pollLineStyle(story.pollId)}
      className="group flex w-24 flex-col items-center gap-2 text-center outline-none"
    >
      <span
        className={cn(
          'relative block size-20 bg-(--poll-line) p-1 shadow-[3px_3px_0_var(--ink)] transition-transform group-hover:-translate-y-0.5',
          index % 2 === 0 ? '-rotate-3' : 'rotate-3',
          selected && 'outline-3 outline-offset-3 outline-fluo',
          'group-focus-visible:outline-3 group-focus-visible:outline-offset-3 group-focus-visible:outline-ring',
        )}
      >
        <span
          className={cn(
            'block size-full overflow-hidden bg-paper',
            closed && 'opacity-55 grayscale',
          )}
        >
          {photo ? (
            <img src={photo} alt="" loading="lazy" className="size-full object-cover" />
          ) : (
            <span className="grid size-full place-items-center font-heading text-3xl text-ink">
              {title.slice(0, 1)}
            </span>
          )}
        </span>
        {closed && (
          <span className="absolute -bottom-2 left-1/2 -translate-x-1/2 -rotate-3 bg-ink px-1.5 font-label text-[0.6rem] tracking-[0.15em] text-paper uppercase">
            {t('activity.storyClosed')}
          </span>
        )}
      </span>
      <span className="line-clamp-2 font-label text-[0.65rem] leading-tight tracking-[0.06em] text-foreground uppercase">
        {cityName ? `${cityName} · ${title}` : title}
      </span>
    </button>
  )
}
