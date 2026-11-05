import { cn } from '@/lib/utils'

const GROUP = 5
const GROUP_WIDTH = 34
const STROKE_GAP = 6

interface Stroke {
  id: string
  x1: number
  y1: number
  x2: number
  y2: number
}

function toStrokes(count: number) {
  const strokes: Stroke[] = []
  for (let mark = 0; mark < count; mark++) {
    const group = Math.floor(mark / GROUP)
    const position = mark % GROUP
    const left = group * GROUP_WIDTH
    if (position === GROUP - 1) {
      strokes.push({ id: `${group}-cross`, x1: left, y1: 20, x2: left + 26, y2: 5 })
    } else {
      const x = left + position * STROKE_GAP + 3
      strokes.push({ id: `${group}-${position}`, x1: x, y1: 2, x2: x, y2: 24 })
    }
  }
  return strokes
}

export function TallyMarks({ count, className }: Readonly<{ count: number; className?: string }>) {
  const groups = Math.max(1, Math.ceil(count / GROUP))

  return (
    <svg
      viewBox={`-2 0 ${groups * GROUP_WIDTH} 26`}
      aria-hidden="true"
      className={cn('h-6 w-auto', className)}
    >
      <g stroke="currentColor" strokeWidth="2.4" strokeLinecap="round">
        {toStrokes(count).map(({ id, ...line }) => (
          <line key={id} {...line} />
        ))}
      </g>
    </svg>
  )
}
