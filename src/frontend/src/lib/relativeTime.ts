const MINUTE = 60_000
const HOUR = 60 * MINUTE
const DAY = 24 * HOUR
const WEEK = 7 * DAY

export function formatRelativeTime(value: string, now: number, locale: string) {
  const date = new Date(value)
  const elapsed = now - date.getTime()
  const relative = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' })

  if (elapsed < MINUTE) {
    return relative.format(0, 'second')
  }
  if (elapsed < HOUR) {
    return relative.format(-Math.floor(elapsed / MINUTE), 'minute')
  }
  if (elapsed < DAY) {
    return relative.format(-Math.floor(elapsed / HOUR), 'hour')
  }
  if (elapsed < WEEK) {
    return relative.format(-Math.floor(elapsed / DAY), 'day')
  }

  const sameYear = date.getFullYear() === new Date(now).getFullYear()
  return new Intl.DateTimeFormat(
    locale,
    sameYear ? { month: 'short', day: 'numeric' } : { dateStyle: 'medium' },
  ).format(date)
}
