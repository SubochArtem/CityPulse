export interface AdminSection {
  path: string
  labelKey:
    | 'admin.sections.cities'
    | 'admin.sections.polls'
    | 'admin.sections.ideas'
    | 'admin.sections.users'
  permission: string
}

export const adminSections: readonly AdminSection[] = [
  { path: 'cities', labelKey: 'admin.sections.cities', permission: 'cities.read.any' },
  { path: 'polls', labelKey: 'admin.sections.polls', permission: 'polls.read.any' },
  { path: 'ideas', labelKey: 'admin.sections.ideas', permission: 'ideas.read.any' },
  { path: 'users', labelKey: 'admin.sections.users', permission: 'users.read.any' },
]
