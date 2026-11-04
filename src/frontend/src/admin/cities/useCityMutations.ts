import { type QueryClient, useMutation, useQueryClient } from '@tanstack/react-query'
import { pollsApi } from '@/api/client'
import { unwrap } from '@/api/errors'
import type { components } from '@/api/schema/polls'

type CityStatus = components['schemas']['CityStatus']

export interface CityFormValues {
  title: string
  description: string
  latitude: number
  longitude: number
  images: File[]
  imagesToDelete: string[]
}

function toFormData(values: CityFormValues, imagesField: 'Images' | 'ImagesToAdd') {
  const formData = new FormData()
  formData.append('Title', values.title)
  if (values.description) {
    formData.append('Description', values.description)
  }
  formData.append('Coordinates.Latitude', String(values.latitude))
  formData.append('Coordinates.Longitude', String(values.longitude))
  for (const image of values.images) {
    formData.append(imagesField, image, image.name)
  }
  for (const id of values.imagesToDelete) {
    formData.append('ImagesToDelete', id)
  }
  return formData
}

function invalidateCities(queryClient: QueryClient) {
  return Promise.all([
    queryClient.invalidateQueries({ queryKey: ['admin', 'cities'] }),
    queryClient.invalidateQueries({ queryKey: ['cities'] }),
    queryClient.invalidateQueries({ queryKey: ['polls'] }),
    queryClient.invalidateQueries({ queryKey: ['ideas'] }),
  ])
}

export function useChangeCityStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: CityStatus }) =>
      unwrap(
        pollsApi.PATCH('/api/v1/admin/cities/{id}/status', {
          params: { path: { id } },
          body: { newStatus: status },
        }),
      ),
    onSuccess: () => invalidateCities(queryClient),
  })
}

export function useDeleteCity() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      unwrap(pollsApi.DELETE('/api/v1/admin/cities/{id}', { params: { path: { id } } })),
    onSuccess: () => invalidateCities(queryClient),
  })
}

export function useCreateCity() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: CityFormValues) =>
      unwrap(
        pollsApi.POST('/api/v1/admin/cities', {
          body: { Title: values.title },
          bodySerializer: () => toFormData(values, 'Images'),
        }),
      ),
    onSuccess: () => invalidateCities(queryClient),
  })
}

export function useUpdateCity(cityId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: CityFormValues) =>
      unwrap(
        pollsApi.PUT('/api/v1/admin/cities/{id}', {
          params: { path: { id: cityId } },
          body: { Title: values.title },
          bodySerializer: () => toFormData(values, 'ImagesToAdd'),
        }),
      ),
    onSuccess: () => invalidateCities(queryClient),
  })
}
