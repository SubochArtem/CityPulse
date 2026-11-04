import { CircleAlert, ImagePlus, X } from 'lucide-react'
import { type ChangeEvent, useEffect, useId, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'

export const MAX_IMAGES = 5
export const MAX_IMAGE_SIZE_MB = 5

const maxImageSizeBytes = MAX_IMAGE_SIZE_MB * 1024 * 1024
const allowedExtensions = new Set(['jpg', 'jpeg', 'png', 'webp'])
const accept = '.jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp'

const fileIds = new WeakMap<File, number>()
let nextFileId = 0

function fileKey(file: File) {
  let id = fileIds.get(file)
  if (id === undefined) {
    id = nextFileId++
    fileIds.set(file, id)
  }
  return id
}

function extensionOf(file: File) {
  return file.name.split('.').pop()?.toLowerCase() ?? ''
}

interface ImagePickerProps {
  files: File[]
  onChange: (files: File[]) => void
  disabled?: boolean
}

export function ImagePicker({ files, onChange, disabled }: Readonly<ImagePickerProps>) {
  const { t } = useTranslation()
  const inputId = useId()
  const [errors, setErrors] = useState<string[]>([])

  const handleSelect = (event: ChangeEvent<HTMLInputElement>) => {
    const selected = [...(event.target.files ?? [])]
    event.target.value = ''

    const accepted: File[] = []
    const rejected: string[] = []
    for (const file of selected) {
      if (!allowedExtensions.has(extensionOf(file))) {
        rejected.push(t('imagePicker.invalidFormat', { name: file.name }))
      } else if (file.size > maxImageSizeBytes) {
        rejected.push(t('imagePicker.tooLarge', { name: file.name, max: MAX_IMAGE_SIZE_MB }))
      } else {
        accepted.push(file)
      }
    }

    const free = MAX_IMAGES - files.length
    if (accepted.length > free) {
      rejected.push(t('imagePicker.tooMany', { max: MAX_IMAGES }))
    }

    setErrors(rejected)
    onChange([...files, ...accepted.slice(0, free)])
  }

  const remove = (file: File) => {
    setErrors([])
    onChange(files.filter((item) => item !== file))
  }

  return (
    <div className="space-y-3">
      <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3">
        {files.map((file) => (
          <li key={fileKey(file)} className="relative">
            <ImagePreview file={file} />
            <Button
              type="button"
              variant="secondary"
              size="icon-sm"
              className="absolute top-2 right-2"
              aria-label={t('imagePicker.remove', { name: file.name })}
              disabled={disabled}
              onClick={() => remove(file)}
            >
              <X />
            </Button>
          </li>
        ))}
        {files.length < MAX_IMAGES && (
          <li>
            <label
              htmlFor={inputId}
              className="flex aspect-video cursor-pointer flex-col items-center justify-center gap-1 rounded-lg border border-dashed border-input p-2 text-center text-sm text-muted-foreground transition-colors hover:border-ring hover:text-foreground has-focus-visible:border-ring has-focus-visible:ring-3 has-focus-visible:ring-ring/50 has-disabled:pointer-events-none has-disabled:opacity-50"
            >
              <ImagePlus className="size-6" />
              {t('imagePicker.add')}
              <input
                id={inputId}
                type="file"
                accept={accept}
                multiple
                disabled={disabled}
                onChange={handleSelect}
                className="sr-only"
              />
            </label>
          </li>
        )}
      </ul>
      <p className="text-sm text-muted-foreground">
        {t('imagePicker.hint', { max: MAX_IMAGES, size: MAX_IMAGE_SIZE_MB })}
      </p>
      {errors.length > 0 && (
        <Alert variant="destructive">
          <CircleAlert />
          <AlertDescription>
            <ul>
              {errors.map((error) => (
                <li key={error}>{error}</li>
              ))}
            </ul>
          </AlertDescription>
        </Alert>
      )}
    </div>
  )
}

function ImagePreview({ file }: Readonly<{ file: File }>) {
  const imageRef = useRef<HTMLImageElement>(null)

  useEffect(() => {
    const url = URL.createObjectURL(file)
    if (imageRef.current) {
      imageRef.current.src = url
    }
    return () => URL.revokeObjectURL(url)
  }, [file])

  return (
    <img
      ref={imageRef}
      alt={file.name}
      className="aspect-video w-full rounded-lg bg-muted object-cover"
    />
  )
}
