import { box, flat, ISO_UNIT, type IsoPolygon, type IsoRect, project, windows } from './projection'
import { type Random, seededRandom } from './random'

export type IsoTree = Readonly<{ x: number; y: number; size: number }>

export type SceneItem =
  | Readonly<{ kind: 'polygon'; key: string; polygon: IsoPolygon }>
  | Readonly<{ kind: 'tree'; key: string; tree: IsoTree }>

export type CityScene = Readonly<{ viewBox: string; items: readonly SceneItem[] }>

type RoadLayout = 'none' | 'alongX' | 'alongY' | 'cross'

type LotKind = 'building' | 'tower' | 'park' | 'pond' | 'plaza'

type DepthObject = Readonly<{ depth: number; polygons: IsoPolygon[]; tree?: IsoTree }>

const HALF = 7
const ROAD = 0.65
const GROUND = 0.36
const SLAB_DEPTH = 2.2
const PALETTES = ['coral', 'blue', 'green', 'sand', 'teal', 'plum'] as const
const LOT_KINDS: readonly LotKind[] = [
  'building',
  'building',
  'building',
  'tower',
  'park',
  'pond',
  'plaza',
]

const iso = (name: string) => `var(--iso-${name})`

const LOTS: readonly IsoRect[] = [
  { x0: -6.2, x1: -1.2, y0: -6.2, y1: -1.2 },
  { x0: 1.2, x1: 6.2, y0: -6.2, y1: -1.2 },
  { x0: -6.2, x1: -1.2, y0: 1.2, y1: 6.2 },
  { x0: 1.2, x1: 6.2, y0: 1.2, y1: 6.2 },
]

function inset(rect: IsoRect, amount: number): IsoRect {
  return { x0: rect.x0 + amount, x1: rect.x1 - amount, y0: rect.y0 + amount, y1: rect.y1 - amount }
}

function treeAt(x: number, y: number, size: number): DepthObject {
  const [screenX, screenY] = project(x, y, GROUND)
  return { depth: x + y, polygons: [], tree: { x: screenX, y: screenY, size } }
}

function building(rect: IsoRect, height: number, random: Random): DepthObject {
  const palette = random.pick(PALETTES)
  const fills = {
    top: iso(`${palette}-roof`),
    left: iso(`${palette}-left`),
    right: iso(`${palette}-right`),
  }
  return {
    depth: rect.x1 + rect.y1,
    polygons: [
      ...box(rect, GROUND, height, fills),
      ...windows(rect, GROUND, height, iso(`${palette}-window`)),
    ],
  }
}

function buildingLot(lot: IsoRect, random: Random): DepthObject[] {
  const area = inset(lot, random.between(0.2, 0.6))
  if (!random.chance(0.4)) {
    return [building(area, random.between(3, 7), random)]
  }
  if (random.chance(0.5)) {
    const middle = (area.x0 + area.x1) / 2
    return [
      building({ ...area, x1: middle - 0.3 }, random.between(3, 8), random),
      building({ ...area, x0: middle + 0.3 }, random.between(2.5, 6), random),
    ]
  }
  const middle = (area.y0 + area.y1) / 2
  return [
    building({ ...area, y1: middle - 0.3 }, random.between(3, 8), random),
    building({ ...area, y0: middle + 0.3 }, random.between(2.5, 6), random),
  ]
}

function towerLot(lot: IsoRect, random: Random): DepthObject[] {
  const size = random.between(1.6, 2.4)
  const centerX = (lot.x0 + lot.x1) / 2
  const centerY = (lot.y0 + lot.y1) / 2
  const tower = {
    x0: centerX - size / 2,
    x1: centerX + size / 2,
    y0: centerY - size / 2,
    y1: centerY + size / 2,
  }
  return [
    building(tower, random.between(9, 12), random),
    treeAt(lot.x1 - 0.5, lot.y0 + 0.6, 0.8),
    treeAt(lot.x0 + 0.6, lot.y1 - 0.5, 0.9),
  ]
}

function parkLot(lot: IsoRect, random: Random): DepthObject[] {
  const trees: DepthObject[] = []
  const count = 4 + Math.floor(random.next() * 4)
  for (let index = 0; index < count; index++) {
    trees.push(
      treeAt(
        random.between(lot.x0 + 0.5, lot.x1 - 0.5),
        random.between(lot.y0 + 0.5, lot.y1 - 0.5),
        random.between(0.75, 1.1),
      ),
    )
  }
  return trees
}

function pondGround(lot: IsoRect): IsoPolygon[] {
  return [
    flat(inset(lot, 0.3), GROUND, iso('water-edge')),
    flat(inset(lot, 0.9), GROUND, iso('water')),
  ]
}

function plazaGround(lot: IsoRect): IsoPolygon[] {
  return [flat(inset(lot, 0.2), GROUND, iso('plaza'))]
}

function plazaObjects(lot: IsoRect, random: Random): DepthObject[] {
  const centerX = (lot.x0 + lot.x1) / 2
  const centerY = (lot.y0 + lot.y1) / 2
  const base = { x0: centerX - 0.8, x1: centerX + 0.8, y0: centerY - 0.8, y1: centerY + 0.8 }
  const column = inset(base, 0.5)
  const gold = { top: iso('gold'), left: iso('gold-left'), right: iso('gold-right') }
  const slab = { top: iso('slab-top'), left: iso('slab-left'), right: iso('slab-right') }
  return [
    {
      depth: centerX + centerY,
      polygons: [...box(base, GROUND, 1, slab), ...box(column, 1, random.between(4, 6), gold)],
    },
    treeAt(lot.x0 + 0.4, lot.y1 - 0.4, 0.8),
    treeAt(lot.x1 - 0.4, lot.y0 + 0.4, 0.8),
  ]
}

function roadGround(layout: RoadLayout): IsoPolygon[] {
  const edge = HALF - 0.4
  const alongX = { x0: -edge, x1: edge, y0: -ROAD, y1: ROAD }
  const alongY = { x0: -ROAD, x1: ROAD, y0: -edge, y1: edge }
  const result: IsoPolygon[] = []
  if (layout === 'alongX' || layout === 'cross') {
    result.push(flat(alongX, GROUND, iso('road')))
    for (let x = -edge + 0.3; x < edge - 0.6; x += 1.2) {
      result.push(flat({ x0: x, x1: x + 0.6, y0: -0.07, y1: 0.07 }, GROUND, iso('road-mark')))
    }
  }
  if (layout === 'alongY' || layout === 'cross') {
    result.push(flat(alongY, GROUND, iso('road')))
    for (let y = -edge + 0.3; y < edge - 0.6; y += 1.2) {
      result.push(flat({ x0: -0.07, x1: 0.07, y0: y, y1: y + 0.6 }, GROUND, iso('road-mark')))
    }
  }
  if (layout === 'none') {
    result.push(flat({ x0: -edge, x1: edge, y0: -0.3, y1: 0.3 }, GROUND, iso('paving')))
    result.push(flat({ x0: -0.3, x1: 0.3, y0: -edge, y1: edge }, GROUND, iso('paving')))
  }
  return result
}

function lotKinds(random: Random): LotKind[] {
  const kinds = LOTS.map(() => random.pick(LOT_KINDS))
  const isBuilt = (kind: LotKind) => kind === 'building' || kind === 'tower'
  let built = kinds.filter(isBuilt).length
  for (let index = 0; index < kinds.length && built < 2; index++) {
    if (!isBuilt(kinds[index])) {
      kinds[index] = 'building'
      built++
    }
  }
  return kinds
}

export function buildCityScene(seed: string): CityScene {
  const random = seededRandom(seed)
  const layout = random.pick<RoadLayout>(['none', 'alongX', 'alongY', 'cross', 'cross'])
  const slab = { top: iso('slab-top'), left: iso('slab-left'), right: iso('slab-right') }
  const grass = { top: iso('grass'), left: iso('grass-left'), right: iso('grass-right') }
  const ground: IsoPolygon[] = [
    ...box({ x0: -HALF, x1: HALF, y0: -HALF, y1: HALF }, -SLAB_DEPTH, 0, slab),
    ...box(inset({ x0: -HALF, x1: HALF, y0: -HALF, y1: HALF }, 0.4), 0, GROUND, grass),
    ...roadGround(layout),
  ]
  const objects: DepthObject[] = []

  lotKinds(random).forEach((kind, index) => {
    const lot = LOTS[index]
    if (kind === 'building') {
      objects.push(...buildingLot(lot, random))
    } else if (kind === 'tower') {
      objects.push(...towerLot(lot, random))
    } else if (kind === 'park') {
      objects.push(...parkLot(lot, random))
    } else if (kind === 'pond') {
      ground.push(...pondGround(lot))
      objects.push(treeAt(lot.x1 - 0.3, lot.y0 + 0.3, 0.8))
    } else {
      ground.push(...plazaGround(lot))
      objects.push(...plazaObjects(lot, random))
    }
  })

  objects.sort((a, b) => a.depth - b.depth)

  const items: SceneItem[] = ground.map((polygon, index) => ({
    kind: 'polygon',
    key: `g${index}`,
    polygon,
  }))
  objects.forEach((object, objectIndex) => {
    object.polygons.forEach((polygon, index) => {
      items.push({ kind: 'polygon', key: `o${objectIndex}-${index}`, polygon })
    })
    if (object.tree) {
      items.push({ kind: 'tree', key: `t${objectIndex}`, tree: object.tree })
    }
  })

  const width = HALF * 4 * ISO_UNIT + 8
  return { viewBox: `${-width / 2} -175 ${width} 262`, items }
}
