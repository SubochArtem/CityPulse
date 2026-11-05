import type { ReactNode } from 'react'

export function PillHeading({ id, children }: Readonly<{ id: string; children: ReactNode }>) {
  return (
    <div className="flex justify-center">
      <h2
        id={id}
        className="rounded-full border px-8 py-2 text-center text-2xl font-normal text-heading outline outline-offset-4 outline-border"
      >
        {children}
      </h2>
    </div>
  )
}
