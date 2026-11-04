import { cn } from '@/lib/utils'

export function Emblem({ className }: Readonly<{ className?: string }>) {
  return (
    <span
      aria-hidden="true"
      className={cn(
        'inline-flex h-16 w-11 shrink-0 flex-col items-center justify-center gap-0.5 rounded-full border-[1.5px] border-heading font-heading leading-none text-heading outline outline-offset-2 outline-heading',
        className,
      )}
    >
      <span className="text-sm font-semibold tracking-wider">CITY</span>
      <svg viewBox="0 0 34 12" className="w-7 text-coral">
        <polyline
          points="0,6 10,6 13,1 17,11 20,6 34,6"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.6"
        />
      </svg>
      <span className="text-[0.5rem] tracking-[0.2em]">PULSE</span>
    </span>
  )
}
