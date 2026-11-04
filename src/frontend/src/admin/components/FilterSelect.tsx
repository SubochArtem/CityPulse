import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

const ALL = 'all'

interface FilterSelectProps {
  label: string
  allLabel: string
  value: string | undefined
  options: ReadonlyArray<{ value: string; label: string }>
  onChange: (value: string | undefined) => void
}

export function FilterSelect({
  label,
  allLabel,
  value,
  options,
  onChange,
}: Readonly<FilterSelectProps>) {
  return (
    <Select
      value={value ?? ALL}
      onValueChange={(next) => onChange(next === ALL ? undefined : next)}
    >
      <SelectTrigger className="sm:w-44" aria-label={label}>
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={ALL}>{allLabel}</SelectItem>
        {options.map((option) => (
          <SelectItem key={option.value} value={option.value}>
            {option.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
