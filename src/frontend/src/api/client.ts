import createClient, { type Middleware } from 'openapi-fetch'
import { authQueryKey } from '../auth/useAuth'
import { queryClient } from '../queryClient'
import { csrfHeaders } from './csrf'
import type { paths as FeedbackPaths } from './schema/feedback'
import type { paths as PollsPaths } from './schema/polls'
import type { paths as UsersPaths } from './schema/users'

const UNAUTHORIZED = 401

const sessionMiddleware: Middleware = {
  onRequest({ request }) {
    for (const [name, value] of Object.entries(csrfHeaders)) {
      request.headers.set(name, value)
    }
    return request
  },
  onResponse({ response }) {
    if (response.status === UNAUTHORIZED) {
      void queryClient.invalidateQueries({ queryKey: authQueryKey })
    }
    return response
  },
}

function createApiClient<TPaths extends object>() {
  const client = createClient<TPaths>({ credentials: 'same-origin' })
  client.use(sessionMiddleware)
  return client
}

export const usersApi = createApiClient<UsersPaths>()
export const pollsApi = createApiClient<PollsPaths>()
export const feedbackApi = createApiClient<FeedbackPaths>()
