import { type FormEvent, useId, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { components } from '@/api/schema/polls'
import { ImagesAndActions, TitleDescriptionFields } from '@/admin/components/AdminFormFields'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
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
      <TitleDescriptionFields
        title={{
          label: t('admin.cityForm.title'),
          value: title,
          max: MAX_TITLE_LENGTH,
          onChange: setTitle,
        }}
        description={{
          label: t('admin.cityForm.description'),
          value: description,
          max: MAX_DESCRIPTION_LENGTH,
          onChange: setDescription,
        }}
        disabled={isSubmitting}
      />

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

      <ImagesAndActions
        images={images}
        onImagesChange={setImages}
        existingImages={existingImages}
        onRemoveExisting={(id) => setImagesToDelete((current) => [...current, id])}
        errorMessage={errorMessage}
        isSubmitting={isSubmitting}
        canSubmit={isValid}
        submitLabel={submitLabel}
        cancelTo={cancelTo}
      />
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
