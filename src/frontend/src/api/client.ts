import createClient, { type Middleware } from 'openapi-fetch'
import { apiBaseUrl } from './config'
import type { paths as FeedbackPaths } from './schema/feedback'
import type { paths as PollsPaths } from './schema/polls'
import type { paths as UsersPaths } from './schema/users'

type TokenGetter = () => Promise<string | undefined>

let getToken: TokenGetter = async () => undefined

export function setTokenGetter(getter: TokenGetter) {
  getToken = getter
}

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = await getToken()
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`)
    }
    return request
  },
}

function createApiClient<TPaths extends object>() {
  const client = createClient<TPaths>({ baseUrl: apiBaseUrl })
  client.use(authMiddleware)
  return client
}

export const usersApi = createApiClient<UsersPaths>()
export const pollsApi = createApiClient<PollsPaths>()
export const feedbackApi = createApiClient<FeedbackPaths>()
