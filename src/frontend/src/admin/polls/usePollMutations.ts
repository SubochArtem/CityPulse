import { type QueryClient, useMutation, useQueryClient } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

type PollStatus = components['schemas']['PollStatus']
type PollType = components['schemas']['PollType']

export interface PollFormValues {
  title: string
  description: string
  endsAt: Date
  budgetAmount: number
  images: File[]
  imagesToDelete: string[]
}

export interface CreatePollValues extends PollFormValues {
  cityId: string
  type: PollType
}

function toFormData(values: PollFormValues, imagesField: 'Images' | 'ImagesToAdd') {
  const formData = new FormData()
  formData.append('Title', values.title)
  if (values.description) {
    formData.append('Description', values.description)
  }
  formData.append('EndsAt', values.endsAt.toISOString())
  formData.append('BudgetAmount', String(values.budgetAmount))
  for (const image of values.images) {
    formData.append(imagesField, image, image.name)
  }
  for (const id of values.imagesToDelete) {
    formData.append('ImagesToDelete', id)
  }
  return formData
}

function invalidatePolls(queryClient: QueryClient) {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: ['admin', 'polls'] }),
    queryClient.invalidateQueries({ queryKey: ['polls'] }),
    queryClient.invalidateQueries({ queryKey: ['cities'] }),
    queryClient.invalidateQueries({ queryKey: ['ideas'] }),
  ])
}

export function useChangePollStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: PollStatus }) =>
      unwrap(
        pollsApi.PATCH('/api/v1/admin/polls/{id}/status', {
          params: { path: { id } },
          body: { newStatus: status },
        }),
      ),
    onSuccess: () => invalidatePolls(queryClient),
  })
}

export function useDeletePoll() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(pollsApi.DELETE('/api/v1/admin/polls/{id}', { params: { path: { id } } })),
    onSuccess: () => invalidatePolls(queryClient),
  })
}

export function useCreatePoll() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: CreatePollValues) =>
      unwrap(
        pollsApi.POST('/api/v1/admin/polls/cities/{cityId}', {
          params: { path: { cityId: values.cityId } },
          body: {
            Title: values.title,
            Type: values.type,
            EndsAt: values.endsAt.toISOString(),
            BudgetAmount: values.budgetAmount,
          },
          bodySerializer: () => {
            const formData = toFormData(values, 'Images')
            formData.append('Type', String(values.type))
            return formData
          },
        }),
      ),
    onSuccess: () => invalidatePolls(queryClient),
  })
}

export function useUpdatePoll(pollId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: PollFormValues) =>
      unwrap(
        pollsApi.PUT('/api/v1/admin/polls/{id}', {
          params: { path: { id: pollId } },
          body: {
            Title: values.title,
            EndsAt: values.endsAt.toISOString(),
            BudgetAmount: values.budgetAmount,
          },
          bodySerializer: () => toFormData(values, 'ImagesToAdd'),
        }),
      ),
    onSuccess: () => invalidatePolls(queryClient),
  })
}
