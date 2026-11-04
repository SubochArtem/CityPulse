import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { useDebouncedValue } from './useDebouncedValue'

function readPage(value: string | null) {
  const page = Number(value)
  return Number.isInteger(page) && page > 0 ? page : 1
}

function withParam(current: URLSearchParams, key: string, value: string | undefined) {
  const params = new URLSearchParams(current)
  if (value) {
    params.set(key, value)
  } else {
    params.delete(key)
  }
  params.delete('page')
  return params
}

export function useListSearchParams() {
  const [searchParams, setSearchParams] = useSearchParams()
  const search = searchParams.get('q') ?? ''
  const page = readPage(searchParams.get('page'))
  const [searchInput, setSearchInput] = useState(search)
  const debouncedInput = useDebouncedValue(searchInput.trim())

  const setParam = (key: string, value: string | undefined) => {
    setSearchParams((current) => withParam(current, key, value), { replace: true })
  }

  useEffect(() => {
    if (debouncedInput === search) {
      return
    }
    setSearchParams((current) => withParam(current, 'q', debouncedInput), { replace: true })
  }, [debouncedInput, search, setSearchParams])

  const goToPage = (next: number) => {
    const params = new URLSearchParams(searchParams)
    if (next > 1) {
      params.set('page', String(next))
    } else {
      params.delete('page')
    }
    setSearchParams(params)
  }

  return {
    searchParams,
    search,
    page,
    searchInput,
    setSearchInput,
    setParam,
    goToPage,
  }
}
