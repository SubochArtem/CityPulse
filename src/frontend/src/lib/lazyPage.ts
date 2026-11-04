import { type ComponentType, lazy } from 'react'

export function lazyPage<TName extends string, TModule extends Record<TName, ComponentType>>(
  load: () => Promise<TModule>,
  name: TName,
) {
  return lazy(async () => {
    const module = await load()
    return { default: module[name] }
  })
}
