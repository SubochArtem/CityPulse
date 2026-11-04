import { useTranslation } from 'react-i18next'

export function CharacterCount({ value, max }: Readonly<{ value: string; max: number }>) {
  const { t } = useTranslation()

  return (
    <p className="text-right text-xs text-muted-foreground tabular-nums">
      {t('form.counter', { current: value.length, max })}
    </p>
  )
}
