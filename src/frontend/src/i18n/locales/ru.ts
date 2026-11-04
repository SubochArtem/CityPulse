import type { Translation } from './en'

type RussianTranslation = Translation & {
  readonly profile: { readonly permissions_few: string; readonly permissions_many: string }
  readonly city: { readonly activePolls_few: string; readonly activePolls_many: string }
}

export const ru: RussianTranslation = {
  nav: {
    cities: 'Города',
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
  cities: {
    title: 'Города',
    subtitle: 'Выберите город, чтобы увидеть его опросы и идеи.',
    searchPlaceholder: 'Поиск городов',
    empty: 'Городов пока нет.',
    noMatches: 'Нет городов по запросу «{{search}}».',
    loadFailed: 'Не удалось загрузить города',
  },
  city: {
    back: 'Все города',
    notFound: 'Город не найден',
    loadFailed: 'Не удалось загрузить город',
    activePolls_one: '{{count}} активный опрос',
    activePolls_few: '{{count}} активных опроса',
    activePolls_many: '{{count}} активных опросов',
    activePolls_other: '{{count}} активного опроса',
    noPolls: 'В этом городе пока нет активных опросов.',
    endsAt: 'До {{date}}',
    budget: 'Бюджет {{amount}}',
    pollType: {
      userVote: 'Голосование жителей',
      managerVote: 'Решение менеджера',
    },
  },
  pagination: {
    previous: 'Назад',
    next: 'Вперёд',
    pageOf: 'Страница {{page}} из {{total}}',
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
