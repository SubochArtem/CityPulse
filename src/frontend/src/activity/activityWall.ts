import { ActivityKind } from './activityText'
import type { Activity } from './useActivityFeed'

export type WallItem =
  | { type: 'idea'; key: string; ideaId: string; pollId?: string; events: Activity[] }
  | { type: 'poll'; key: string; entry: Activity }

export interface PollStory {
  pollId: string
  cityId?: string
  title: string
  closed: boolean
}

export function buildWall(entries: Activity[], pollId: string | null): WallItem[] {
  const items: WallItem[] = []
  const ideas = new Map<string, Extract<WallItem, { type: 'idea' }>>()

  for (const entry of entries) {
    if (pollId && entry.pollId !== pollId) {
      continue
    }
    if (entry.kind === ActivityKind.PollOpened || entry.kind === ActivityKind.PollClosed) {
      items.push({ type: 'poll', key: entry.id ?? `${entry.pollId}-${entry.kind}`, entry })
      continue
    }
    if (!entry.ideaId) {
      continue
    }
    const existing = ideas.get(entry.ideaId)
    if (existing) {
      existing.events.push(entry)
      continue
    }
    const item = {
      type: 'idea' as const,
      key: entry.ideaId,
      ideaId: entry.ideaId,
      pollId: entry.pollId ?? undefined,
      events: [entry],
    }
    ideas.set(entry.ideaId, item)
    items.push(item)
  }

  return items
}

export function buildStories(entries: Activity[]): PollStory[] {
  const stories = new Map<string, PollStory>()
  const settled = new Set<string>()
  for (const entry of entries) {
    if (!entry.pollId) {
      continue
    }
    const story = stories.get(entry.pollId) ?? {
      pollId: entry.pollId,
      cityId: entry.cityId ?? undefined,
      title: entry.pollTitle ?? '',
      closed: false,
    }
    const statusEvent =
      entry.kind === ActivityKind.PollClosed || entry.kind === ActivityKind.PollOpened
    if (statusEvent && !settled.has(entry.pollId)) {
      story.closed = entry.kind === ActivityKind.PollClosed
      settled.add(entry.pollId)
    }
    stories.set(entry.pollId, story)
  }
  return [...stories.values()]
}

export interface WallColumn<T> {
  id: string
  items: T[]
}

export function distribute<T>(items: T[], columns: number): WallColumn<T>[] {
  const result = Array.from({ length: columns }, (_, column) => ({
    id: `column-${column + 1}`,
    items: [] as T[],
  }))
  items.forEach((item, index) => result[index % columns].items.push(item))
  return result
}
