import { tiltFor } from '@/lib/tilt'
import { cn } from '@/lib/utils'

interface HalftonePosterProps {
  seed: string
  title?: string | null
  photoUrl?: string | null
  framed?: boolean
  className?: string
}

export function HalftonePoster({
  seed,
  title,
  photoUrl,
  framed = true,
  className,
}: Readonly<HalftonePosterProps>) {
  return (
    <div
      className={cn(framed && 'paper poster p-2', className)}
      style={framed ? { rotate: `${tiltFor(seed)}deg` } : undefined}
    >
      <div className="@container relative aspect-square overflow-hidden bg-paper">
        {photoUrl ? <PhotoPrint photoUrl={photoUrl} /> : <Stamp title={title} />}
      </div>
    </div>
  )
}

function PhotoPrint({ photoUrl }: Readonly<{ photoUrl: string }>) {
  return (
    <>
      <img
        src={photoUrl}
        alt=""
        loading="lazy"
        className="size-full object-cover contrast-125 grayscale transition duration-300 group-hover:contrast-100 group-hover:grayscale-0 group-focus-visible:contrast-100 group-focus-visible:grayscale-0"
      />
      <span
        aria-hidden="true"
        className="absolute inset-0 bg-riot mix-blend-lighten transition-opacity duration-300 group-hover:opacity-0 group-focus-visible:opacity-0"
      />
      <span
        aria-hidden="true"
        className="halftone absolute inset-0 text-ink/25 mix-blend-multiply transition-opacity duration-300 group-hover:opacity-0 group-focus-visible:opacity-0"
      />
    </>
  )
}

function stampTextClass(title: string) {
  if (title.length <= 16) {
    return 'text-[clamp(1.5rem,14cqi,4.5rem)]'
  }
  if (title.length <= 40) {
    return 'text-[clamp(1.25rem,9cqi,3rem)]'
  }
  return 'text-[clamp(1rem,6.5cqi,2.25rem)]'
}

function Stamp({ title }: Readonly<{ title?: string | null }>) {
  return (
    <>
      <span
        aria-hidden="true"
        className="halftone absolute inset-0 text-riot/30 [mask-image:linear-gradient(135deg,transparent_35%,black)]"
      />
      <span
        aria-hidden="true"
        className={cn(
          'absolute inset-0 grid place-items-center p-4 text-center font-heading leading-none text-riot [overflow-wrap:anywhere]',
          stampTextClass(title ?? ''),
        )}
      >
        <span className="-rotate-6 border-4 border-double border-riot px-3 py-2">{title}</span>
      </span>
    </>
  )
}
