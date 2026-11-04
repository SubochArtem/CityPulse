import { Search } from 'lucide-react'
import { Input } from '@/components/ui/input'

interface AdminSearchInputProps {
  value: string
  onChange: (value: string) => void
  placeholder: string
}

export function AdminSearchInput({
  value,
  onChange,
  placeholder,
}: Readonly<AdminSearchInputProps>) {
  return (
    <div className="relative sm:max-w-sm sm:flex-1">
      <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        type="search"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        aria-label={placeholder}
        className="pl-8"
      />
    </div>
  )
}
