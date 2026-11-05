import { ArrowLeft } from 'lucide-react'
import { BoxAction } from '@/components/BoxAction'

interface FormHeaderProps {
  title: string
  backTo: string
  backLabel: string
}

export function FormHeader({ title, backTo, backLabel }: Readonly<FormHeaderProps>) {
  return (
    <div className="flex flex-col items-start gap-5">
      <BoxAction icon={ArrowLeft} label={backLabel} to={backTo} />
      <h2 className="text-3xl text-heading [overflow-wrap:anywhere]">{title}</h2>
    </div>
  )
}
