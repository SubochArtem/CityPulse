import { ApiError } from '@/api/errors'

export function citySaveErrorKey(error: Error) {
  if (error instanceof ApiError && error.status === 404) {
    return 'admin.cityForm.notFound'
  }
  if (error instanceof ApiError && error.status === 409) {
    return 'admin.cityForm.conflict'
  }
  return 'admin.cityForm.saveFailed'
}
