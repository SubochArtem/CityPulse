export interface AdminSection {
  path: string
  labelKey: 'admin.sections.cities'
  permission: string
}

export const adminSections: readonly AdminSection[] = [
  { path: 'cities', labelKey: 'admin.sections.cities', permission: 'cities.read.any' },
]
