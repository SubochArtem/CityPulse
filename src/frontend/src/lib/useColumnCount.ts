import { useSyncExternalStore } from 'react'

const queries = [
  { media: '(min-width: 1024px)', columns: 3 },
  { media: '(min-width: 640px)', columns: 2 },
] as const

function subscribe(onChange: () => void) {
  const lists = queries.map(({ media }) => window.matchMedia(media))
  lists.forEach((list) => list.addEventListener('change', onChange))
  return () => lists.forEach((list) => list.removeEventListener('change', onChange))
}

function snapshot() {
  return queries.find(({ media }) => window.matchMedia(media).matches)?.columns ?? 1
}

export function useColumnCount() {
  return useSyncExternalStore(subscribe, snapshot, () => 1)
}
