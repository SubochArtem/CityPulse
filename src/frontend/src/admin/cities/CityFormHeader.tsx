import { ArrowLeft } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'

export function CityFormHeader({ title, backTo }: Readonly<{ title: string; backTo: string }>) {
  const { t } = useTranslation()

  return (
    <div className="space-y-2">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to={backTo}>
          <ArrowLeft />
          {t('admin.cityForm.back')}
        </Link>
      </Button>
      <h2 className="text-xl font-semibold tracking-tight">{title}</h2>
    </div>
  )
}
