import { Link } from 'react-router'

export function NotFoundPage() {
  return (
    <p className="text-slate-700">
      Page not found.{' '}
      <Link to="/" className="underline">
        Go home
      </Link>
    </p>
  )
}
