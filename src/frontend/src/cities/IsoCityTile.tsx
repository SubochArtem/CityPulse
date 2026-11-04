import { useMemo } from 'react'
import { buildCityScene, type IsoTree } from '@/lib/iso/cityScene'
import { cn } from '@/lib/utils'

export function IsoCityTile({
  seed,
  photoUrl,
  className,
}: Readonly<{ seed: string; photoUrl?: string | null; className?: string }>) {
  const scene = useMemo(() => buildCityScene(seed), [seed])

  return (
    <div className={className}>
      <div className="relative">
        <svg
          viewBox={scene.viewBox}
          aria-hidden="true"
          className={cn(
            'block h-auto w-full dark:brightness-90',
            photoUrl &&
              'opacity-0 transition-opacity duration-300 group-hover:opacity-100 group-focus-visible:opacity-100',
          )}
        >
          {scene.items.map((item) =>
            item.kind === 'polygon' ? (
              <polygon key={item.key} points={item.polygon.points} fill={item.polygon.fill} />
            ) : (
              <Tree key={item.key} tree={item.tree} />
            ),
          )}
        </svg>
        {photoUrl && (
          <img
            src={photoUrl}
            alt=""
            loading="lazy"
            className="pointer-events-none absolute inset-0 size-full border bg-card object-cover p-1.5 shadow-lg transition duration-300 group-hover:scale-95 group-hover:opacity-0 group-focus-visible:scale-95 group-focus-visible:opacity-0 motion-reduce:transition-opacity"
          />
        )}
      </div>
    </div>
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
