export interface AdminSection {
  path: string
  labelKey: 'admin.sections.cities' | 'admin.sections.polls'
  permission: string
}

export const adminSections: readonly AdminSection[] = [
  { path: 'cities', labelKey: 'admin.sections.cities', permission: 'cities.read.any' },
  { path: 'polls', labelKey: 'admin.sections.polls', permission: 'polls.read.any' },
]
