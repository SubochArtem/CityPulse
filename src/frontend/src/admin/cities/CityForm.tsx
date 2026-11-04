import { CircleAlert, LoaderCircle } from 'lucide-react'
import { type FormEvent, useId, useState } from 'react'
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
import type { CityFormValues } from './useCityMutations'

type City = components['schemas']['CityDto']

const MAX_TITLE_LENGTH = 100
const MAX_DESCRIPTION_LENGTH = 500
const coordinatePattern = /^-?\d+(\.\d+)?$/

function parseCoordinate(value: string, limit: number) {
  const normalized = value.trim().replace(',', '.')
  if (!coordinatePattern.test(normalized)) {
    return undefined
  }
  const number = Number(normalized)
  return Math.abs(number) <= limit ? number : undefined
}

function formatCoordinate(value: number | undefined) {
  return value === undefined ? '' : String(value)
}

interface CityFormProps {
  city?: City
  submitLabel: string
  isSubmitting: boolean
  errorMessage: string | null
  cancelTo: string
  onSubmit: (values: CityFormValues) => void
}

export function CityForm({
  city,
  submitLabel,
  isSubmitting,
  errorMessage,
  cancelTo,
  onSubmit,
}: Readonly<CityFormProps>) {
  const { t } = useTranslation()
  const ids = {
    title: useId(),
    description: useId(),
    latitude: useId(),
    longitude: useId(),
  }
  const [title, setTitle] = useState(city?.title ?? '')
  const [description, setDescription] = useState(city?.description ?? '')
  const [latitude, setLatitude] = useState(formatCoordinate(city?.coordinates.latitude))
  const [longitude, setLongitude] = useState(formatCoordinate(city?.coordinates.longitude))
  const [images, setImages] = useState<File[]>([])
  const [imagesToDelete, setImagesToDelete] = useState<string[]>([])

  const parsedLatitude = parseCoordinate(latitude, 90)
  const parsedLongitude = parseCoordinate(longitude, 180)
  const latitudeInvalid = latitude.trim() !== '' && parsedLatitude === undefined
  const longitudeInvalid = longitude.trim() !== '' && parsedLongitude === undefined
  const existingImages = (city?.images ?? []).filter(
    (image) => image.id && !imagesToDelete.includes(image.id),
  )
  const isValid =
    title.trim().length > 0 && parsedLatitude !== undefined && parsedLongitude !== undefined

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!isValid || isSubmitting || parsedLatitude === undefined || parsedLongitude === undefined) {
      return
    }
    onSubmit({
      title: title.trim(),
      description: description.trim(),
      latitude: parsedLatitude,
      longitude: parsedLongitude,
      images,
      imagesToDelete,
    })
  }

  return (
    <form onSubmit={handleSubmit} className="max-w-2xl space-y-6" noValidate>
      <div className="space-y-2">
        <Label htmlFor={ids.title}>{t('admin.cityForm.title')}</Label>
        <Input
          id={ids.title}
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          maxLength={MAX_TITLE_LENGTH}
          disabled={isSubmitting}
          required
        />
        <CharacterCount value={title} max={MAX_TITLE_LENGTH} />
      </div>

      <div className="space-y-2">
        <Label htmlFor={ids.description}>
          {t('admin.cityForm.description')}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </Label>
        <Textarea
          id={ids.description}
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          maxLength={MAX_DESCRIPTION_LENGTH}
          disabled={isSubmitting}
          rows={4}
          className="min-h-24"
        />
        <CharacterCount value={description} max={MAX_DESCRIPTION_LENGTH} />
      </div>

      <fieldset className="space-y-2">
        <legend className="text-sm font-medium">{t('admin.cityForm.coordinates')}</legend>
        <div className="grid gap-4 sm:grid-cols-2">
          <CoordinateField
            id={ids.latitude}
            label={t('admin.cityForm.latitude')}
            value={latitude}
            onChange={setLatitude}
            invalid={latitudeInvalid}
            error={t('admin.cityForm.latitudeInvalid')}
            disabled={isSubmitting}
          />
          <CoordinateField
            id={ids.longitude}
            label={t('admin.cityForm.longitude')}
            value={longitude}
            onChange={setLongitude}
            invalid={longitudeInvalid}
            error={t('admin.cityForm.longitudeInvalid')}
            disabled={isSubmitting}
          />
        </div>
        <p className="text-sm text-muted-foreground">{t('admin.cityForm.coordinatesHint')}</p>
      </fieldset>

      <div className="space-y-2">
        <p className="flex items-center gap-2 text-sm font-medium">
          {t('admin.cityForm.images')}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </p>
        <ImagePicker
          files={images}
          onChange={setImages}
          existing={existingImages}
          onRemoveExisting={(id) => setImagesToDelete((current) => [...current, id])}
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
        <Button type="submit" disabled={!isValid || isSubmitting}>
          {isSubmitting && <LoaderCircle className="animate-spin" />}
          {isSubmitting ? t('admin.cityForm.saving') : submitLabel}
        </Button>
        <Button type="button" variant="ghost" asChild>
          <Link to={cancelTo}>{t('admin.cancel')}</Link>
        </Button>
      </div>
    </form>
  )
}

interface CoordinateFieldProps {
  id: string
  label: string
  value: string
  onChange: (value: string) => void
  invalid: boolean
  error: string
  disabled: boolean
}

function CoordinateField({
  id,
  label,
  value,
  onChange,
  invalid,
  error,
  disabled,
}: Readonly<CoordinateFieldProps>) {
  const errorId = `${id}-error`

  return (
    <div className="space-y-2">
      <Label htmlFor={id} className="font-normal text-muted-foreground">
        {label}
      </Label>
      <Input
        id={id}
        inputMode="decimal"
        autoComplete="off"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        aria-invalid={invalid}
        aria-describedby={invalid ? errorId : undefined}
        disabled={disabled}
        required
      />
      {invalid && (
        <p id={errorId} className="text-sm text-destructive">
          {error}
        </p>
      )}
    </div>
  )
}
