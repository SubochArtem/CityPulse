export function readEnum<T extends number>(
  value: string | null,
  allowed: Partial<Record<T, string>>,
) {
  const number = Number(value)
  return value && number in allowed ? (number as T) : undefined
}
