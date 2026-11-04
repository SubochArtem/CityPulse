import { useQuery } from '@tanstack/react-query'
import { usersApi } from '@/api/client'
import { unwrap } from '@/api/errors'

const MAX_IDS_PER_REQUEST = 100

function chunk<T>(items: T[], size: number) {
  const chunks: T[][] = []
  for (let index = 0; index < items.length; index += size) {
    chunks.push(items.slice(index, index + size))
  }
  return chunks
}

export function usePublicProfiles(userIds: ReadonlyArray<string | undefined>) {
  const ids = [...new Set(userIds.filter((id): id is string => Boolean(id)))].sort((a, b) =>
    a.localeCompare(b),
  )

  return useQuery({
    queryKey: ['users', 'profiles', ids],
    enabled: ids.length > 0,
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const responses = await Promise.all(
        chunk(ids, MAX_IDS_PER_REQUEST).map((batch) =>
          unwrap(usersApi.GET('/api/v1/Users/profiles', { params: { query: { Ids: batch } } })),
        ),
      )
      const nicknames = new Map<string, string>()
      for (const profile of responses.flat()) {
        if (profile.id && profile.nickname) {
          nicknames.set(profile.id, profile.nickname)
        }
      }
      return nicknames
    },
  })
}
