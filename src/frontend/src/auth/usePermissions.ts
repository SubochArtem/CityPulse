import { useAuth } from './useAuth'

export function usePermissions() {
  const { permissions, isLoading, can } = useAuth()

  return { permissions, isLoading, can }
}
