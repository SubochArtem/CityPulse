import { ApiError } from '@/api/errors'

export function pollCreateErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.pollForm.cityNotFound'
  }
  if (error instanceof ApiError && error.status === 409) {
    return 'admin.pollForm.alreadyActive'
  }
  return 'admin.pollForm.saveFailed'
}

export function pollUpdateErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.pollForm.notFound'
  }
  if (error instanceof ApiError && error.status === 409) {
    return 'admin.pollForm.imagesRejected'
  }
  return 'admin.pollForm.saveFailed'
}
