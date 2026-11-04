import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'

const approvalStatuses = {
  1: { key: 'idea.approval.approved', variant: 'default' },
  2: { key: 'idea.approval.rejected', variant: 'destructive' },
  3: { key: 'idea.approval.deferred', variant: 'outline' },
  4: { key: 'idea.approval.pending', variant: 'outline' },
} as const

interface IdeaApprovalBadgeProps {
  status: number | undefined
  className?: string
}

export function IdeaApprovalBadge({ status, className }: Readonly<IdeaApprovalBadgeProps>) {
  const { t } = useTranslation()
  const approval = approvalStatuses[status as keyof typeof approvalStatuses]

  if (!approval) {
    return null
  }

  return (
    <Badge variant={approval.variant} className={className}>
      {t(approval.key)}
    </Badge>
  )
}
