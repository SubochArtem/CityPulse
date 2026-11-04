import { CircleAlert } from 'lucide-react'
import { Alert, AlertTitle } from '@/components/ui/alert'

export function ActionErrorAlert({ message }: Readonly<{ message: string }>) {
  return (
    <Alert variant="destructive">
      <CircleAlert />
      <AlertTitle>{message}</AlertTitle>
    </Alert>
  )
}
