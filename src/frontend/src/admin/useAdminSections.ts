import { usePermissions } from '@/auth/usePermissions'
import { adminSections } from './sections'

export function useAdminSections() {
  const { can, isLoading } = usePermissions()

  return {
    sections: adminSections.filter((section) => can(section.permission)),
    isLoading,
  }
}
