import { useLocation } from 'react-router'

export function useListPath(basePath: string) {
  const { state } = useLocation()
  const listSearch: unknown = state?.listSearch
  return `${basePath}${typeof listSearch === 'string' ? listSearch : ''}`
}
