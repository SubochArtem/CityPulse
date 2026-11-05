import type { LucideIcon } from 'lucide-react'

interface IconFactProps {
  icon: LucideIcon
  label: string
  value: string
}

export function IconFact({ icon: Icon, label, value }: Readonly<IconFactProps>) {
  return (
    <div className="flex items-center gap-4">
      <span className="grid size-11 shrink-0 place-items-center rounded-full border outline outline-offset-3 outline-border/60">
        <Icon className="size-5 text-heading" />
      </span>
      <div className="flex flex-col-reverse">
        <dt className="font-heading text-[0.65rem] tracking-[0.2em] text-muted-foreground uppercase">
          {label}
        </dt>
        <dd className="font-heading text-xl font-medium">{value}</dd>
      </div>
    </div>
  )
}
