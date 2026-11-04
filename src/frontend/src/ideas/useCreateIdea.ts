import { useMutation, useQueryClient } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'

export interface CreateIdeaInput {
  title: string
  description: string
  images: File[]
}

function toFormData({ title, description, images }: CreateIdeaInput) {
  const formData = new FormData()
  formData.append('Title', title)
  if (description) {
    formData.append('Description', description)
  }
  for (const image of images) {
    formData.append('Images', image, image.name)
  }
  return formData
}

export function useCreateIdea(pollId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateIdeaInput) =>
      unwrap(
        pollsApi.POST('/api/v1/ideas/polls/{pollId}', {
          params: { path: { pollId } },
          body: { Title: input.title, Description: input.description },
          bodySerializer: () => toFormData(input),
        }),
      ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['polls', pollId, 'ideas'] }),
  })
}
