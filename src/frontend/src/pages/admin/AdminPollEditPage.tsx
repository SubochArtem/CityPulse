import { CircleAlert } from 'lucide-react'
import { type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import { ApiError } from '@/api/errors'
import { useAdminCityOptions } from '@/admin/cities/useAdminCityOptions'
import { FormHeader } from '@/admin/components/FormHeader'
import { FormSkeleton } from '@/admin/components/FormSkeleton'
import { PollForm } from '@/admin/polls/PollForm'
import { pollUpdateErrorKey } from '@/admin/polls/pollErrors'
import { useAdminPoll } from '@/admin/polls/useAdminPoll'
import { useUpdatePoll } from '@/admin/polls/usePollMutations'
import { useListPath } from '@/admin/useListPath'
import { Alert, AlertTitle } from '@/components/ui/alert'

export function AdminPollEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { pollId = '' } = useParams()
  const listPath = useListPath('/admin/polls')
  const { data: poll, error, isPending } = useAdminPoll(pollId)
  const { data: cities } = useAdminCityOptions()
  const updatePoll = useUpdatePoll(pollId)

  let content: ReactNode
  if (isPending) {
    content = <FormSkeleton />
  } else if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    content = (
      <Alert variant="destructive" className="max-w-md">
        <CircleAlert />
        <AlertTitle>
          {notFound ? t('admin.pollForm.notFound') : t('admin.pollForm.loadFailed')}
        </AlertTitle>
      </Alert>
    )
  } else {
    content = (
      <PollForm
        key={poll.id}
        poll={poll}
        cities={cities ?? []}
        submitLabel={t('admin.pollForm.save')}
        isSubmitting={updatePoll.isPending}
        errorMessage={updatePoll.error ? t(pollUpdateErrorKey(updatePoll.error)) : null}
        cancelTo={listPath}
        onSubmit={(values) =>
          updatePoll.mutate(values, { onSuccess: () => void navigate(listPath) })
        }
      />
    )
  }

  return (
    <div className="space-y-6">
      <FormHeader
        title={
          poll?.title
            ? t('admin.pollForm.editTitle', { title: poll.title })
            : t('admin.pollForm.editFallback')
        }
        backTo={listPath}
        backLabel={t('admin.pollForm.back')}
      />
      {content}
    </div>
  )
}
