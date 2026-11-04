import { CalendarClock, Wallet } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'

interface PollFactsProps {
  endsAt: string | undefined
  budgetAmount: number | undefined
  className?: string
}

export function PollFacts({ endsAt, budgetAmount, className }: Readonly<PollFactsProps>) {
  const { t, i18n } = useTranslation()

  if (!endsAt && !budgetAmount) {
    return null
  }

  return (
    <div className={cn('flex flex-wrap gap-x-4 gap-y-1 text-muted-foreground', className)}>
      {endsAt && (
        <span className="inline-flex items-center gap-1.5">
          <CalendarClock className="size-4" />
          {t('poll.endsAt', {
            date: new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(
              new Date(endsAt),
            ),
          })}
        </span>
      )}
      {budgetAmount ? (
        <span className="inline-flex items-center gap-1.5">
          <Wallet className="size-4" />
          {t('poll.budget', { amount: new Intl.NumberFormat(i18n.language).format(budgetAmount) })}
        </span>
      ) : null}
    </div>
  )
}
