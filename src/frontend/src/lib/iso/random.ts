export type Random = Readonly<{
  next: () => number
  between: (min: number, max: number) => number
  pick: <T>(items: readonly T[]) => T
  chance: (probability: number) => boolean
}>

function hash(seed: string) {
  let value = 2166136261
  for (let index = 0; index < seed.length; index++) {
    value ^= seed.charCodeAt(index)
    value = Math.imul(value, 16777619)
  }
  return value >>> 0
}

export function seededRandom(seed: string): Random {
  let state = hash(seed)

  const next = () => {
    state = (state + 0x6d2b79f5) >>> 0
    let value = state
    value = Math.imul(value ^ (value >>> 15), value | 1)
    value ^= value + Math.imul(value ^ (value >>> 7), value | 61)
    return ((value ^ (value >>> 14)) >>> 0) / 4294967296
  }

  return {
    next,
    between: (min, max) => min + next() * (max - min),
    pick: (items) => items[Math.floor(next() * items.length)],
    chance: (probability) => next() < probability,
  }
}
