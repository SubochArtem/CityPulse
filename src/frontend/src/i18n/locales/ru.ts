import type { Translation } from './en'

type RussianTranslation = Translation & {
  readonly profile: { readonly permissions_few: string; readonly permissions_many: string }
}

export const ru: RussianTranslation = {
  nav: {
    profile: 'Профиль',
  },
  auth: {
    logIn: 'Войти',
    logOut: 'Выйти',
    signInFailed: 'Не удалось войти',
    loading: 'Загрузка…',
  },
  theme: {
    label: 'Тема',
    light: 'Светлая',
    dark: 'Тёмная',
    system: 'Системная',
  },
  language: {
    label: 'Язык',
  },
  home: {
    title: 'Добро пожаловать в CityPulse',
    subtitle: 'Идеи, опросы и отзывы для вашего города.',
  },
  notFound: {
    title: 'Страница не найдена',
    goHome: 'На главную',
  },
  profile: {
    notFound: 'Профиль не найден',
    contactSupport: 'Обратитесь в поддержку.',
    loadFailed: 'Не удалось загрузить профиль',
    permissions_one: '{{count}} разрешение',
    permissions_few: '{{count}} разрешения',
    permissions_many: '{{count}} разрешений',
    permissions_other: '{{count}} разрешения',
  },
}
