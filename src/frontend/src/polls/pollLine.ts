import type { CSSProperties } from 'react'

export const POLL_LINE_COUNT = 8

export function pollLineIndex(pollId: string | undefined) {
  let hash = 0
  for (const char of pollId ?? '') {
    hash = (hash * 31 + (char.codePointAt(0) ?? 0)) >>> 0
  }
  return (hash % POLL_LINE_COUNT) + 1
}

export function pollLineStyle(pollId: string | undefined): CSSProperties {
  return { '--poll-line': `var(--line-${pollLineIndex(pollId)})` } as CSSProperties
}
