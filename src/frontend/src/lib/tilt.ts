const tilts = [-2, -1.2, -0.6, 0.8, 1.4, 2] as const

function hash(seed: string) {
  let value = 2166136261
  for (let index = 0; index < seed.length; index++) {
    value ^= seed.codePointAt(index) ?? 0
    value = Math.imul(value, 16777619)
  }
  return value >>> 0
}

export function tiltFor(seed: string) {
  return tilts[hash(seed) % tilts.length]
}
