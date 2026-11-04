function required(name: keyof ImportMetaEnv): string {
  const value = import.meta.env[name]
  if (!value) {
    throw new Error(`Missing environment variable ${name}`)
  }
  return value
}

export const authConfig = {
  domain: required('VITE_AUTH0_DOMAIN'),
  clientId: required('VITE_AUTH0_CLIENT_ID'),
  audience: required('VITE_AUTH0_AUDIENCE'),
}
