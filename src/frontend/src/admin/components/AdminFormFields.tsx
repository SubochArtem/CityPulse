import { CircleAlert, LoaderCircle } from 'lucide-react'
import { useId } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { CharacterCount } from '@/components/CharacterCount'
import { ImagePicker } from '@/components/ImagePicker'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

type Image = components['schemas']['ImageDto']

interface TextValue {
  label: string
  value: string
  max: number
  onChange: (value: string) => void
}

interface TitleDescriptionFieldsProps {
  title: TextValue
  description: TextValue
  disabled: boolean
  descriptionRows?: number
}

export function TitleDescriptionFields({
  title,
  description,
  disabled,
  descriptionRows = 4,
}: Readonly<TitleDescriptionFieldsProps>) {
  const { t } = useTranslation()
  const titleId = useId()
  const descriptionId = useId()

  return (
    <>
      <div className="space-y-2">
        <Label htmlFor={titleId}>{title.label}</Label>
        <Input
          id={titleId}
          value={title.value}
          onChange={(event) => title.onChange(event.target.value)}
          maxLength={title.max}
          disabled={disabled}
          required
        />
        <CharacterCount value={title.value} max={title.max} />
      </div>

      <div className="space-y-2">
        <Label htmlFor={descriptionId}>
          {description.label}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </Label>
        <Textarea
          id={descriptionId}
          value={description.value}
          onChange={(event) => description.onChange(event.target.value)}
          maxLength={description.max}
          disabled={disabled}
          rows={descriptionRows}
          className="min-h-24"
        />
        <CharacterCount value={description.value} max={description.max} />
      </div>
    </>
  )
}

interface ImagesAndActionsProps {
  images: File[]
  onImagesChange: (files: File[]) => void
  existingImages: Image[]
  onRemoveExisting: (id: string) => void
  errorMessage: string | null
  isSubmitting: boolean
  canSubmit: boolean
  submitLabel: string
  cancelTo: string
}

export function ImagesAndActions({
  images,
  onImagesChange,
  existingImages,
  onRemoveExisting,
  errorMessage,
  isSubmitting,
  canSubmit,
  submitLabel,
  cancelTo,
}: Readonly<ImagesAndActionsProps>) {
  const { t } = useTranslation()

  return (
    <>
      <div className="space-y-2">
        <p className="flex items-center gap-2 text-sm font-medium">
          {t('admin.images')}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </p>
        <ImagePicker
          files={images}
          onChange={onImagesChange}
          existing={existingImages}
          onRemoveExisting={onRemoveExisting}
          disabled={isSubmitting}
        />
      </div>

      {errorMessage && (
        <Alert variant="destructive">
          <CircleAlert />
          <AlertTitle>{errorMessage}</AlertTitle>
        </Alert>
      )}

      <div className="flex flex-wrap gap-2">
        <Button type="submit" disabled={!canSubmit || isSubmitting}>
          {isSubmitting && <LoaderCircle className="animate-spin" />}
          {isSubmitting ? t('admin.saving') : submitLabel}
        </Button>
        <Button type="button" variant="ghost" asChild>
          <Link to={cancelTo}>{t('admin.cancel')}</Link>
        </Button>
      </div>
    </>
  )
}
