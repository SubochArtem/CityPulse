export const MAX_OPTIONS = 100

export interface AdminOption {
  id: string
  title: string
}

export function toOptions(
  items: ReadonlyArray<{ id?: string; title: string | null }> | null | undefined,
): AdminOption[] {
  return (items ?? [])
    .filter((item) => item.id)
    .map((item) => ({ id: item.id ?? '', title: item.title ?? '' }))
    .sort((a, b) => a.title.localeCompare(b.title))
}
