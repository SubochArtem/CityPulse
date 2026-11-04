import { CircleAlert } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useSearchParams } from 'react-router'
import { useAdminCityOptions } from '@/admin/cities/useAdminCityOptions'
import { FormHeader } from '@/admin/components/FormHeader'
import { FormSkeleton } from '@/admin/components/FormSkeleton'
import { PollForm } from '@/admin/polls/PollForm'
import { pollCreateErrorKey } from '@/admin/polls/pollErrors'
import { useCreatePoll } from '@/admin/polls/usePollMutations'
import { useListPath } from '@/admin/useListPath'
import { Alert, AlertTitle } from '@/components/ui/alert'

export function AdminPollCreatePage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const listPath = useListPath('/admin/polls')
  const { data: cities, error, isPending } = useAdminCityOptions()
  const createPoll = useCreatePoll()
  const requestedCityId = searchParams.get('city')

  let content: ReactNode
  if (isPending) {
    content = <FormSkeleton />
  } else if (error) {
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>{t('admin.pollForm.citiesFailed')}</AlertTitle>
      </Alert>
    )
  } else {
    content = (
      <PollForm
        cities={cities}
        initialCityId={cities.find((city) => city.id === requestedCityId)?.id}
        submitLabel={t('admin.pollForm.create')}
        isSubmitting={createPoll.isPending}
        errorMessage={createPoll.error ? t(pollCreateErrorKey(createPoll.error)) : null}
        cancelTo={listPath}
        onSubmit={(values) =>
          createPoll.mutate(values, { onSuccess: () => void navigate(listPath) })
        }
      />
    )
  }

  return (
    <div className="space-y-6">
      <FormHeader
        title={t('admin.pollForm.newTitle')}
        backTo={listPath}
        backLabel={t('admin.pollForm.back')}
      />
      {content}
    </div>
  )
}
