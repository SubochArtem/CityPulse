import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { CityForm } from '@/admin/cities/CityForm'
import { FormHeader } from '@/admin/components/FormHeader'
import { citySaveErrorKey } from '@/admin/cities/cityErrors'
import { useListPath } from '@/admin/useListPath'
import { useCreateCity } from '@/admin/cities/useCityMutations'

export function AdminCityCreatePage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const listPath = useListPath('/admin/cities')
  const createCity = useCreateCity()

  return (
    <div className="space-y-6">
      <FormHeader
        title={t('admin.cityForm.newTitle')}
        backTo={listPath}
        backLabel={t('admin.cityForm.back')}
      />
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
