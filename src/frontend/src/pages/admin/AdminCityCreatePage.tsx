import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { CityForm } from '@/admin/cities/CityForm'
import { CityFormHeader } from '@/admin/cities/CityFormHeader'
import { citySaveErrorKey } from '@/admin/cities/cityErrors'
import { useCitiesListPath } from '@/admin/cities/useCitiesListPath'
import { useCreateCity } from '@/admin/cities/useCityMutations'

export function AdminCityCreatePage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const listPath = useCitiesListPath()
  const createCity = useCreateCity()

  return (
    <div className="space-y-6">
      <CityFormHeader title={t('admin.cityForm.newTitle')} backTo={listPath} />
      <CityForm
        submitLabel={t('admin.cityForm.create')}
        isSubmitting={createCity.isPending}
        errorMessage={createCity.error ? t(citySaveErrorKey(createCity.error)) : null}
        cancelTo={listPath}
        onSubmit={(values) =>
          createCity.mutate(values, { onSuccess: () => void navigate(listPath) })
        }
      />
    </div>
  )
}
