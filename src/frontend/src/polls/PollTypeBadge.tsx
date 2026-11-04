import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'
import { pollTypeKey } from '@/polls/pollTypes'

interface PollTypeBadgeProps {
  type: number | undefined
  className?: string
}

export function PollTypeBadge({ type, className }: Readonly<PollTypeBadgeProps>) {
  const { t } = useTranslation()
  const key = pollTypeKey(type)

  if (!key) {
    return null
  }

  return (
    <Badge variant="secondary" className={className}>
      {t(key)}
    </Badge>
  )
}
