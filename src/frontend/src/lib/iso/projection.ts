export const ISO_UNIT = 9

export type IsoRect = Readonly<{ x0: number; x1: number; y0: number; y1: number }>

export type IsoPolygon = Readonly<{ points: string; fill: string }>

export type BoxFills = Readonly<{ top: string; left: string; right: string }>

export function project(x: number, y: number, z: number): readonly [number, number] {
  return [(x - y) * ISO_UNIT, ((x + y) * ISO_UNIT) / 2 - z * ISO_UNIT]
}

function polygon(
  corners: readonly (readonly [number, number, number])[],
  fill: string,
): IsoPolygon {
  const points = corners
    .map(([x, y, z]) =>
      project(x, y, z)
        .map((value) => value.toFixed(1))
        .join(','),
    )
    .join(' ')
  return { points, fill }
}

export function flat({ x0, x1, y0, y1 }: IsoRect, z: number, fill: string): IsoPolygon {
  return polygon(
    [
      [x0, y0, z],
      [x1, y0, z],
      [x1, y1, z],
      [x0, y1, z],
    ],
    fill,
  )
}

export function box(rect: IsoRect, z0: number, z1: number, fills: BoxFills): IsoPolygon[] {
  const { x0, x1, y0, y1 } = rect
  return [
    polygon(
      [
        [x0, y1, z1],
        [x1, y1, z1],
        [x1, y1, z0],
        [x0, y1, z0],
      ],
      fills.left,
    ),
    polygon(
      [
        [x1, y0, z1],
        [x1, y1, z1],
        [x1, y1, z0],
        [x1, y0, z0],
      ],
      fills.right,
    ),
    flat(rect, z1, fills.top),
  ]
}

export function windows(rect: IsoRect, z0: number, z1: number, fill: string): IsoPolygon[] {
  const { x0, x1, y0, y1 } = rect
  const result: IsoPolygon[] = []
  for (let bottom = z0 + 0.6; bottom + 0.55 <= z1 - 0.4; bottom += 1.1) {
    const top = bottom + 0.55
    for (let x = x0 + 0.35; x + 0.45 <= x1 - 0.2; x += 0.9) {
      result.push(
        polygon(
          [
            [x, y1, top],
            [x + 0.45, y1, top],
            [x + 0.45, y1, bottom],
            [x, y1, bottom],
          ],
          fill,
        ),
      )
    }
    for (let y = y0 + 0.35; y + 0.45 <= y1 - 0.2; y += 0.9) {
      result.push(
        polygon(
          [
            [x1, y, top],
            [x1, y + 0.45, top],
            [x1, y + 0.45, bottom],
            [x1, y, bottom],
          ],
          fill,
        ),
      )
    }
  }
  return result
}
