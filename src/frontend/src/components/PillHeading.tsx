import type { ReactNode } from 'react'

export function PillHeading({ id, children }: Readonly<{ id: string; children: ReactNode }>) {
  return (
    <div className="flex justify-center">
      <h2
        id={id}
        className="-rotate-1 bg-foreground px-6 py-2 text-center text-3xl text-background shadow-[3px_3px_0_oklch(0_0_0/0.25)] [overflow-wrap:anywhere]"
      >
        {children}
      </h2>
    </div>
  )
}
