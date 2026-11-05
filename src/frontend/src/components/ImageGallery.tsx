import { useState } from 'react'
import type { components } from '@/api/schema/polls'
import { sortImages } from '@/lib/images'
import { tiltFor } from '@/lib/tilt'

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
    <div className="space-y-6">
      <div className="paper poster tape -rotate-[0.4deg] p-2 sm:p-3">
        <img src={current.url} alt={alt} className="aspect-21/9 w-full object-cover" />
      </div>
      {ordered.length > 1 && (
        <div className="-mx-2 flex gap-4 overflow-x-auto px-2 pt-2 pb-3">
          {ordered.map((image, index) => (
            <button
              key={image.id}
              type="button"
              onClick={() => setSelected(index)}
              aria-pressed={index === selected}
              style={{ rotate: `${tiltFor(image.id ?? String(index))}deg` }}
              className="paper poster shrink-0 p-1 pb-3 outline-none transition-[rotate,translate,box-shadow,background-color] duration-200 hover:-translate-y-0.5 focus-visible:ring-3 focus-visible:ring-ring/50 aria-pressed:rotate-0! aria-pressed:bg-fluo aria-pressed:shadow-[3px_3px_0_var(--color-ink)] motion-reduce:transition-none"
            >
              <img
                src={image.url ?? ''}
                alt=""
                className="aspect-video h-16 object-cover grayscale-[0.6] transition-[filter] [button:hover>&]:grayscale-0 [button[aria-pressed=true]>&]:grayscale-0"
              />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
