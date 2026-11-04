import { useMemo } from 'react'
import { buildCityScene, type IsoTree } from '@/lib/iso/cityScene'
import { cn } from '@/lib/utils'

export function IsoCityTile({ seed, className }: Readonly<{ seed: string; className?: string }>) {
  const scene = useMemo(() => buildCityScene(seed), [seed])

  return (
    <svg
      viewBox={scene.viewBox}
      aria-hidden="true"
      className={cn('block h-auto w-full dark:brightness-90', className)}
    >
      {scene.items.map((item) =>
        item.kind === 'polygon' ? (
          <polygon key={item.key} points={item.polygon.points} fill={item.polygon.fill} />
        ) : (
          <Tree key={item.key} tree={item.tree} />
        ),
      )}
    </svg>
  )
}

function Tree({ tree: { x, y, size } }: Readonly<{ tree: IsoTree }>) {
  return (
    <g>
      <rect x={x - 1.5} y={y - 10 * size} width={3} height={10 * size} fill="var(--iso-trunk)" />
      <ellipse cx={x} cy={y - 14 * size} rx={7 * size} ry={9 * size} fill="var(--iso-leaves)" />
      <ellipse
        cx={x - 2 * size}
        cy={y - 17 * size}
        rx={3.5 * size}
        ry={4.5 * size}
        fill="var(--iso-leaves-light)"
      />
    </g>
  )
}
