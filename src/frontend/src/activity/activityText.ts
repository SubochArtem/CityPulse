import type { Activity } from './useActivityFeed'

export const ActivityKind = {
  PollOpened: 1,
  PollClosed: 2,
  IdeaProposed: 3,
  IdeaReviewed: 4,
  IdeaCommented: 5,
} as const

export type ActivityEventKey =
  | 'activity.event.pollOpened'
  | 'activity.event.pollFinished'
  | 'activity.event.pollCancelled'
  | 'activity.event.pollInactive'
  | 'activity.event.ideaProposed'
  | 'activity.event.ideaApproved'
  | 'activity.event.ideaRejected'
  | 'activity.event.ideaCommented'

const pollClosedKeys: Record<string, ActivityEventKey> = {
  Finished: 'activity.event.pollFinished',
  Cancelled: 'activity.event.pollCancelled',
  Inactive: 'activity.event.pollInactive',
}

const ideaReviewedKeys: Record<string, ActivityEventKey> = {
  Approved: 'activity.event.ideaApproved',
  Rejected: 'activity.event.ideaRejected',
}

export function activityEventKey(entry: Activity): ActivityEventKey | undefined {
  switch (entry.kind) {
    case ActivityKind.PollOpened:
      return 'activity.event.pollOpened'
    case ActivityKind.PollClosed:
      return pollClosedKeys[entry.status ?? '']
    case ActivityKind.IdeaProposed:
      return 'activity.event.ideaProposed'
    case ActivityKind.IdeaReviewed:
      return ideaReviewedKeys[entry.status ?? '']
    case ActivityKind.IdeaCommented:
      return 'activity.event.ideaCommented'
    default:
      return undefined
  }
}

export function isPollEvent(entry: Activity) {
  return entry.kind === ActivityKind.PollOpened || entry.kind === ActivityKind.PollClosed
}
