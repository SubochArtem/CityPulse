import type { components } from '@/api/schema/polls'

type Image = components['schemas']['ImageDto']

export function sortImages(images: Image[] | null | undefined) {
  return [...(images ?? [])].sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
}

export function coverImage(images: Image[] | null | undefined) {
  return sortImages(images)[0]
}
