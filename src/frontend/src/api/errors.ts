export interface ProblemDetails {
  type?: string | null
  title?: string | null
  status?: number | null
  detail?: string | null
  instance?: string | null
  errors?: string[] | null
  code?: string | null
}

export class ApiError extends Error {
  readonly status: number
  readonly code: string | null
  readonly errors: string[]

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `Request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.code = problem?.code ?? null
    this.errors = problem?.errors ?? []
  }
}

export async function unwrap<T>(
  request: Promise<{ data?: T; error?: unknown; response: Response }>,
): Promise<T> {
  const { data, error, response } = await request
  if (!response.ok) {
    throw new ApiError(response.status, (error ?? undefined) as ProblemDetails | undefined)
  }
  return data as T
}
