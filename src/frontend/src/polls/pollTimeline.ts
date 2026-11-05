const DAY_MS = 24 * 60 * 60 * 1000

export function pollProgress(
  createdAt: string | undefined,
  endsAt: string | undefined,
  now: number,
) {
  if (!createdAt || !endsAt) {
    return 0
  }
  const start = new Date(createdAt).getTime()
  const end = new Date(endsAt).getTime()
  if (end <= start) {
    return 1
  }
  return Math.min(1, Math.max(0, (now - start) / (end - start)))
}

export function pollDaysLeft(endsAt: string | undefined, now: number) {
  if (!endsAt) {
    return 0
  }
  return Math.max(0, Math.ceil((new Date(endsAt).getTime() - now) / DAY_MS))
}

const MAX_TALLY_DAYS = 20

export function showsTally(daysLeft: number) {
  return daysLeft > 0 && daysLeft <= MAX_TALLY_DAYS
}
