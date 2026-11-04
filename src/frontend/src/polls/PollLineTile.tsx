import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'
import { pollLineStyle } from '@/polls/pollLine'
import { PollLineBadge } from '@/polls/PollLineBadge'
import { PollRoute } from '@/polls/PollRoute'
import { pollTypeKey } from '@/polls/pollTypes'

type Poll = components['schemas']['PollDto']

interface PollLineTileProps {
  poll: Poll
  number?: number
  className?: string
}

export function PollLineTile({ poll, number, className }: Readonly<PollLineTileProps>) {
  const { t, i18n } = useTranslation()
  const typeKey = pollTypeKey(poll.type)

  return (
    <div
      style={pollLineStyle(poll.id)}
      className={cn(
        'flex min-w-0 flex-col gap-4 rounded-xl bg-(--poll-line) p-5 text-line-foreground',
        className,
      )}
    >
      <div className="flex items-center gap-2.5 text-sm">
        <PollLineBadge pollId={poll.id} number={number} inverted />
        {typeKey && <span>{t(typeKey)}</span>}
      </div>
      <h2 className="text-xl font-bold [overflow-wrap:anywhere]">{poll.title}</h2>
      <PollRoute pollId={poll.id} createdAt={poll.createdAt} endsAt={poll.endsAt} onLine />
      <div className="mt-auto flex flex-wrap items-center justify-between gap-3">
        {poll.endsAt && (
          <span className="text-sm text-line-foreground/85">
            {t('poll.endsAt', {
              date: new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
                new Date(poll.endsAt),
              ),
            })}
          </span>
        )}
        <Button asChild>
          <Link to={`/polls/${poll.id}`}>
            {t('poll.open')}
            <ArrowRight />
          </Link>
        </Button>
      </div>
    </div>
  )
}
