import { useLocation } from 'react-router'

export function useCitiesListPath() {
  const { state } = useLocation()
  const listSearch: unknown = state?.listSearch
  return `/admin/cities${typeof listSearch === 'string' ? listSearch : ''}`
}
