import { ArrowLeft } from 'lucide-react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'

interface FormHeaderProps {
  title: string
  backTo: string
  backLabel: string
}

export function FormHeader({ title, backTo, backLabel }: Readonly<FormHeaderProps>) {
  return (
    <div className="space-y-2">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to={backTo}>
          <ArrowLeft />
          {backLabel}
        </Link>
      </Button>
      <h2 className="text-3xl font-medium text-heading">{title}</h2>
    </div>
  )
}
