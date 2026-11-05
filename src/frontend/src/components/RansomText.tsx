import { cn } from '@/lib/utils'

const pieceClasses = [
  'bg-ink font-heading text-paper',
  'bg-fluo font-sans font-extrabold text-ink',
  'border-2 border-ink bg-paper font-label font-bold text-riot',
  'bg-riot font-heading text-paper',
  'bg-white font-sans text-[0.85em] font-extrabold text-ink italic',
]

const tilts = [-4, 2, -1, 3, -3, 1, 4, -2, 0]

interface Letter {
  char: string
  index: number
}

function toWords(text: string) {
  let index = 0
  return text.split(' ').map((word) =>
    Array.from(word, (char): Letter => {
      const letter = { char, index }
      index += 1
      return letter
    }),
  )
}

interface RansomTextProps {
  text: string
  label?: string
  className?: string
}

export function RansomText({ text, label, className }: Readonly<RansomTextProps>) {
  const words = toWords(text)

  return (
    <span className={cn('inline-flex flex-wrap items-center gap-y-1 leading-none', className)}>
      <span className="sr-only">{label ?? text}</span>
      {words.map((letters) => (
        <span
          key={letters[0]?.index ?? 0}
          aria-hidden="true"
          className="mr-[0.3em] inline-flex last:mr-0"
        >
          {letters.map(({ char, index }) => (
            <span
              key={index}
              className={cn(
                'mx-px inline-block px-[0.08em] py-[0.04em] shadow-[1px_1px_0_oklch(0_0_0/0.3)]',
                pieceClasses[(index * 3 + 1) % pieceClasses.length],
              )}
              style={{ rotate: `${tilts[(index * 7) % tilts.length]}deg` }}
            >
              {char}
            </span>
          ))}
        </span>
      ))}
    </span>
  )
}
