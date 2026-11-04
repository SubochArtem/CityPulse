import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'

const pollTypeKeys = {
  1: 'poll.type.userVote',
  2: 'poll.type.managerVote',
} as const

interface PollTypeBadgeProps {
  type: number | undefined
  className?: string
}

export function PollTypeBadge({ type, className }: Readonly<PollTypeBadgeProps>) {
  const { t } = useTranslation()
  const key = pollTypeKeys[type as keyof typeof pollTypeKeys]

  if (!key) {
    return null
  }

  return (
    <Badge variant="secondary" className={className}>
      {t(key)}
    </Badge>
  )
}
