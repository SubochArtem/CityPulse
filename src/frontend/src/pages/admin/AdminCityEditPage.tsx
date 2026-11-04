import { CircleAlert } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import { CityForm } from '@/admin/cities/CityForm'
import { FormHeader } from '@/admin/components/FormHeader'
import { FormSkeleton } from '@/admin/components/FormSkeleton'
import { citySaveErrorKey } from '@/admin/cities/cityErrors'
import { useAdminCity } from '@/admin/cities/useAdminCity'
import { useListPath } from '@/admin/useListPath'
import { useUpdateCity } from '@/admin/cities/useCityMutations'
import { Alert, AlertTitle } from '@/components/ui/alert'

export function AdminCityEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { cityId = '' } = useParams()
  const listPath = useListPath('/admin/cities')
  const { data: city, error, isPending } = useAdminCity(cityId)
  const updateCity = useUpdateCity(cityId)

  let content: ReactNode
  if (isPending) {
    content = <FormSkeleton />
  } else if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>
          {notFound ? t('admin.cityForm.notFound') : t('admin.cityForm.loadFailed')}
        </AlertTitle>
      </Alert>
    )
  } else {
    content = (
      <CityForm
        key={city.id}
        city={city}
        submitLabel={t('admin.cityForm.save')}
        isSubmitting={updateCity.isPending}
        errorMessage={updateCity.error ? t(citySaveErrorKey(updateCity.error)) : null}
        cancelTo={listPath}
        onSubmit={(values) =>
          updateCity.mutate(values, { onSuccess: () => void navigate(listPath) })
        }
      />
    )
  }

  return (
    <div className="space-y-6">
      <FormHeader
        title={
          city?.title
            ? t('admin.cityForm.editTitle', { title: city.title })
            : t('admin.cityForm.editFallback')
        }
        backTo={listPath}
        backLabel={t('admin.cityForm.back')}
      />
      {content}
    </div>
  )
}
