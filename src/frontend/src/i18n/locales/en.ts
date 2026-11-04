export const en = {
  nav: {
    cities: 'Cities',
    profile: 'Profile',
  },
  auth: {
    logIn: 'Log in',
    logOut: 'Log out',
    signInFailed: 'Sign-in failed',
    loading: 'Loading…',
  },
  theme: {
    label: 'Theme',
    light: 'Light',
    dark: 'Dark',
    system: 'System',
  },
  language: {
    label: 'Language',
  },
  home: {
    title: 'Welcome to CityPulse',
    subtitle: 'Ideas, polls and feedback for your city.',
  },
  cities: {
    title: 'Cities',
    subtitle: 'Pick a city to see its polls and ideas.',
    searchPlaceholder: 'Search cities',
    empty: 'No cities yet.',
    noMatches: 'No cities match “{{search}}”.',
    loadFailed: 'Could not load cities',
  },
  city: {
    back: 'All cities',
    notFound: 'City not found',
    loadFailed: 'Could not load the city',
    activePolls: 'Active polls',
    noPolls: 'No active polls in this city yet.',
  },
  poll: {
    back: 'Back to city',
    notFound: 'Poll not found',
    loadFailed: 'Could not load the poll',
    ideas: 'Ideas',
    noIdeas: 'No ideas in this poll yet.',
    endsAt: 'Ends {{date}}',
    budget: 'Budget {{amount}}',
    type: {
      userVote: 'Residents vote',
      managerVote: 'Manager decision',
    },
  },
  idea: {
    back: 'Back to poll',
    notFound: 'Idea not found',
    loadFailed: 'Could not load the idea',
    author: 'By {{nickname}}',
    unknownAuthor: 'Unknown author',
    createdAt: 'Added {{date}}',
    updatedAt: 'Updated {{date}}',
    poll: 'Poll',
    approval: {
      approved: 'Approved',
      rejected: 'Rejected',
      deferred: 'Deferred',
      pending: 'Pending review',
    },
  },
  pagination: {
    previous: 'Previous',
    next: 'Next',
    pageOf: 'Page {{page}} of {{total}}',
  },
  notFound: {
    title: 'Page not found',
    goHome: 'Go home',
  },
  profile: {
    notFound: 'Profile not found',
    contactSupport: 'Please contact support.',
    loadFailed: 'Could not load your profile',
    permissions_one: '{{count}} permission',
    permissions_other: '{{count}} permissions',
  },
} as const

type Messages<T> = { readonly [K in keyof T]: T[K] extends string ? string : Messages<T[K]> }

export type Translation = Messages<typeof en>
