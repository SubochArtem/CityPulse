import { execFileSync } from 'node:child_process'

const base = process.env.API_BASE_URL ?? 'http://localhost:8080'
const services = ['users', 'polls', 'feedback']

for (const service of services) {
  execFileSync(
    'npx',
    [
      '--yes',
      'openapi-typescript@7.13.0',
      `${base}/swagger/${service}/swagger.json`,
      '-o',
      `src/api/schema/${service}.ts`,
    ],
    { stdio: 'inherit' },
  )
}
