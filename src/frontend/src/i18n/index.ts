import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import { en } from './locales/en'
import { ru } from './locales/ru'

export const languages = [
  { code: 'en', label: 'English' },
  { code: 'ru', label: 'Русский' },
] as const

export type Language = (typeof languages)[number]['code']

i18n.on('languageChanged', (language) => {
  document.documentElement.lang = language
})

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      ru: { translation: ru },
    },
    supportedLngs: languages.map((language) => language.code),
    fallbackLng: 'en',
    interpolation: { escapeValue: false },
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: 'citypulse-lang',
      caches: ['localStorage'],
      convertDetectedLanguage: (language) => language.split('-')[0],
    },
  })

export default i18n
