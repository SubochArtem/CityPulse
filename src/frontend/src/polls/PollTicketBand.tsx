import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'

interface PollTicketBandProps {
  pollId: string | undefined
  className?: string
}

export function PollTicketBand({ pollId, className }: Readonly<PollTicketBandProps>) {
  return (
    <div
      aria-hidden="true"
      style={pollLineStyle(pollId)}
      className={cn(
        'h-4 bg-(--poll-line) [mask:radial-gradient(circle_at_10px_100%,transparent_6px,black_6.5px)_0_0/20px_100%]',
        className,
      )}
    />
  )
}
