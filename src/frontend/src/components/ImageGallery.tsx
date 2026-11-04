import { useState } from 'react'
import type { components } from '@/api/schema/polls'
import { sortImages } from '@/lib/images'

type Image = components['schemas']['ImageDto']

interface ImageGalleryProps {
  images: Image[] | null | undefined
  alt: string
}

export function ImageGallery({ images, alt }: Readonly<ImageGalleryProps>) {
  const ordered = sortImages(images).filter((image) => image.url)
  const [selected, setSelected] = useState(0)
  const current = ordered[selected]

  if (!current?.url) {
    return null
  }

  return (
    <div className="space-y-2">
      <img src={current.url} alt={alt} className="aspect-21/9 w-full rounded-xl object-cover" />
      {ordered.length > 1 && (
        <div className="-m-1 flex gap-2 overflow-x-auto p-1">
          {ordered.map((image, index) => (
            <button
              key={image.id}
              type="button"
              onClick={() => setSelected(index)}
              aria-pressed={index === selected}
              className="shrink-0 overflow-hidden rounded-lg opacity-60 ring-offset-background transition-opacity outline-none hover:opacity-100 focus-visible:ring-3 focus-visible:ring-ring/50 aria-pressed:opacity-100 aria-pressed:ring-2 aria-pressed:ring-primary"
            >
              <img src={image.url ?? ''} alt="" className="aspect-video h-16 object-cover" />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
