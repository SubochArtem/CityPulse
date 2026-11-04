import { CircleAlert, LoaderCircle } from 'lucide-react'
import { type FormEvent, type ReactNode, useId, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { components } from '@/api/schema/polls'
import { CharacterCount } from '@/components/CharacterCount'
import { ImagePicker } from '@/components/ImagePicker'
import { Alert, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import type { CreatePollValues } from './usePollMutations'

type Poll = components['schemas']['PollDto']
type PollType = components['schemas']['PollType']

const MAX_TITLE_LENGTH = 300
const MAX_DESCRIPTION_LENGTH = 2000
const MIN_DURATION_DAYS = 3
const MAX_DURATION_DAYS = 180
const DEFAULT_DURATION_DAYS = 30
const MAX_BUDGET = 100_000_000
const DAY_MS = 24 * 60 * 60 * 1000
const HOUR_MS = 60 * 60 * 1000
const amountPattern = /^\d+(\.\d{1,2})?$/

const typeOptions = [
  { value: 1, key: 'poll.type.userVote' },
  { value: 2, key: 'poll.type.managerVote' },
] as const

function pad(value: number) {
  return String(value).padStart(2, '0')
}

function toLocalInput(date: Date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function ceilToHour(time: number) {
  return new Date(Math.ceil(time / HOUR_MS) * HOUR_MS)
}

function parseDate(value: string) {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? undefined : date
}

function parseAmount(value: string) {
  const normalized = value.replaceAll(/\s/g, '').replace(',', '.')
  if (!amountPattern.test(normalized)) {
    return undefined
  }
  const amount = Number(normalized)
  return amount > 0 && amount <= MAX_BUDGET ? amount : undefined
}

function endsAtRange(isEdit: boolean, now: number) {
  if (isEdit) {
    return { min: new Date(now), max: undefined }
  }
  return {
    min: new Date(now + MIN_DURATION_DAYS * DAY_MS),
    max: new Date(now + MAX_DURATION_DAYS * DAY_MS),
  }
}

interface PollFormProps {
  poll?: Poll
  cities: ReadonlyArray<{ id: string; title: string }>
  initialCityId?: string
  submitLabel: string
  isSubmitting: boolean
  errorMessage: string | null
  cancelTo: string
  onSubmit: (values: CreatePollValues) => void
}

export function PollForm({
  poll,
  cities,
  initialCityId,
  submitLabel,
  isSubmitting,
  errorMessage,
  cancelTo,
  onSubmit,
}: Readonly<PollFormProps>) {
  const { t, i18n } = useTranslation()
  const ids = {
    city: useId(),
    type: useId(),
    title: useId(),
    description: useId(),
    endsAt: useId(),
    budget: useId(),
  }
  const isEdit = poll !== undefined
  const [now] = useState(() => Date.now())
  const [cityId, setCityId] = useState(poll?.cityId ?? initialCityId ?? '')
  const [type, setType] = useState(String(poll?.type ?? typeOptions[0].value))
  const [title, setTitle] = useState(poll?.title ?? '')
  const [description, setDescription] = useState(poll?.description ?? '')
  const [endsAt, setEndsAt] = useState(() =>
    poll?.endsAt
      ? toLocalInput(new Date(poll.endsAt))
      : toLocalInput(ceilToHour(now + DEFAULT_DURATION_DAYS * DAY_MS)),
  )
  const [budget, setBudget] = useState(
    poll?.budgetAmount === undefined ? '' : String(poll.budgetAmount),
  )
  const [images, setImages] = useState<File[]>([])
  const [imagesToDelete, setImagesToDelete] = useState<string[]>([])

  const range = endsAtRange(isEdit, now)
  const parsedEndsAt = parseDate(endsAt)
  const parsedBudget = parseAmount(budget)
  const endsAtInvalid =
    endsAt !== '' &&
    (parsedEndsAt === undefined ||
      parsedEndsAt < range.min ||
      (range.max !== undefined && parsedEndsAt > range.max))
  const budgetInvalid = budget.trim() !== '' && parsedBudget === undefined
  const existingImages = (poll?.images ?? []).filter(
    (image) => image.id && !imagesToDelete.includes(image.id),
  )
  const isValid =
    cityId !== '' &&
    title.trim().length > 0 &&
    parsedEndsAt !== undefined &&
    !endsAtInvalid &&
    parsedBudget !== undefined

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!isValid || isSubmitting || parsedEndsAt === undefined || parsedBudget === undefined) {
      return
    }
    onSubmit({
      cityId,
      type: Number(type) as PollType,
      title: title.trim(),
      description: description.trim(),
      endsAt: parsedEndsAt,
      budgetAmount: parsedBudget,
      images,
      imagesToDelete,
    })
  }

  const endsAtError = isEdit
    ? t('admin.pollForm.endsAtPast')
    : t('admin.pollForm.endsAtRange', { min: MIN_DURATION_DAYS, max: MAX_DURATION_DAYS })

  return (
    <form onSubmit={handleSubmit} className="max-w-2xl space-y-6" noValidate>
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor={ids.city}>{t('admin.pollForm.city')}</Label>
          <Select value={cityId} onValueChange={setCityId} disabled={isEdit || isSubmitting}>
            <SelectTrigger id={ids.city} className="w-full">
              <SelectValue placeholder={t('admin.pollForm.cityPlaceholder')} />
            </SelectTrigger>
            <SelectContent>
              {cities.map((city) => (
                <SelectItem key={city.id} value={city.id}>
                  {city.title}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-2">
          <Label htmlFor={ids.type}>{t('admin.pollForm.type')}</Label>
          <Select value={type} onValueChange={setType} disabled={isEdit || isSubmitting}>
            <SelectTrigger id={ids.type} className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {typeOptions.map((option) => (
                <SelectItem key={option.value} value={String(option.value)}>
                  {t(option.key)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        {isEdit && (
          <p className="text-sm text-muted-foreground sm:col-span-2">
            {t('admin.pollForm.fixedFields')}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor={ids.title}>{t('admin.pollForm.title')}</Label>
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
          {t('admin.pollForm.description')}
          <span className="font-normal text-muted-foreground">{t('form.optional')}</span>
        </Label>
        <Textarea
          id={ids.description}
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          maxLength={MAX_DESCRIPTION_LENGTH}
          disabled={isSubmitting}
          rows={6}
          className="min-h-32"
        />
        <CharacterCount value={description} max={MAX_DESCRIPTION_LENGTH} />
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <Field
          id={ids.endsAt}
          label={t('admin.pollForm.endsAt')}
          error={endsAtInvalid ? endsAtError : null}
        >
          <Input
            id={ids.endsAt}
            type="datetime-local"
            value={endsAt}
            min={toLocalInput(range.min)}
            max={range.max ? toLocalInput(range.max) : undefined}
            onChange={(event) => setEndsAt(event.target.value)}
            aria-invalid={endsAtInvalid}
            aria-describedby={endsAtInvalid ? `${ids.endsAt}-error` : undefined}
            disabled={isSubmitting}
            className="dark:scheme-dark"
            required
          />
        </Field>
        <Field
          id={ids.budget}
          label={t('admin.pollForm.budget')}
          error={
            budgetInvalid
              ? t('admin.pollForm.budgetInvalid', {
                  max: new Intl.NumberFormat(i18n.language).format(MAX_BUDGET),
                })
              : null
          }
        >
          <Input
            id={ids.budget}
            inputMode="decimal"
            autoComplete="off"
            value={budget}
            onChange={(event) => setBudget(event.target.value)}
            aria-invalid={budgetInvalid}
            aria-describedby={budgetInvalid ? `${ids.budget}-error` : undefined}
            disabled={isSubmitting}
            required
          />
        </Field>
      </div>

      <div className="space-y-2">
        <p className="flex items-center gap-2 text-sm font-medium">
          {t('admin.pollForm.images')}
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
          {isSubmitting ? t('admin.pollForm.saving') : submitLabel}
        </Button>
        <Button type="button" variant="ghost" asChild>
          <Link to={cancelTo}>{t('admin.cancel')}</Link>
        </Button>
      </div>
    </form>
  )
}

interface FieldProps {
  id: string
  label: string
  error: string | null
  children: ReactNode
}

function Field({ id, label, error, children }: Readonly<FieldProps>) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>{label}</Label>
      {children}
      {error && (
        <p id={`${id}-error`} className="text-sm text-destructive">
          {error}
        </p>
      )}
    </div>
  )
}
