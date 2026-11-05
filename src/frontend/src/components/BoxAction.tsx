import type { LucideIcon } from 'lucide-react'
import { Link } from 'react-router'
import { cn } from '@/lib/utils'

interface BoxActionProps {
  icon: LucideIcon
  label: string
  to?: string
  className?: string
}

export function BoxAction({ icon: Icon, label, to, className }: Readonly<BoxActionProps>) {
  const content = (
    <>
      <span className="grid w-11 place-items-center border-r border-heading bg-primary text-primary-foreground">
        <Icon className="size-4" />
      </span>
      <span className="inline-flex items-center px-5 font-label text-xs tracking-[0.2em] uppercase">
        {label}
      </span>
    </>
  )
  const boxClass = cn(
    'inline-flex min-h-11 items-stretch border border-heading bg-card text-foreground',
    className,
  )

  if (!to) {
    return <span className={boxClass}>{content}</span>
  }

  return (
    <Link
      to={to}
      className={cn(
        boxClass,
        'outline-none transition-colors hover:bg-secondary focus-visible:ring-3 focus-visible:ring-ring/50',
      )}
    >
      {content}
    </Link>
  )
}
