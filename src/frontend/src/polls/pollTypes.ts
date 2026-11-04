export const pollTypeKeys = {
  1: 'poll.type.userVote',
  2: 'poll.type.managerVote',
} as const

export function pollTypeKey(type: number | undefined) {
  return pollTypeKeys[type as keyof typeof pollTypeKeys]
}
