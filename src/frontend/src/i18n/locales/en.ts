export const en = {
  nav: {
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
