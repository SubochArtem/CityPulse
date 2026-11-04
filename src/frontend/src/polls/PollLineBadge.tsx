import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'

interface PollLineBadgeProps {
  pollId: string | undefined
  number?: number
  className?: string
}

export function PollLineBadge({ pollId, number, className }: Readonly<PollLineBadgeProps>) {
  if (number === undefined) {
    return (
      <span
        aria-hidden="true"
        style={pollLineStyle(pollId)}
        className={cn(
          'inline-block size-4 shrink-0 rounded-full border-4 border-(--poll-line) bg-card',
          className,
        )}
      />
    )
  }

  return (
    <span
      aria-hidden="true"
      style={pollLineStyle(pollId)}
      className={cn(
        'inline-grid size-7 shrink-0 place-items-center rounded-full bg-(--poll-line) font-heading text-xs font-bold text-line-foreground tabular-nums',
        className,
      )}
    >
      {number}
    </span>
  )
}
