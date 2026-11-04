import { Link, Outlet } from 'react-router'
import { AuthButton } from './AuthButton'
import { AuthError } from './AuthError'

export function Layout() {
  return (
    <>
      <header className="flex items-center justify-between p-6">
        <Link to="/" className="text-2xl font-semibold text-slate-900">
          CityPulse
        </Link>
        <nav className="flex items-center gap-4">
          <Link to="/profile" className="text-sm text-slate-700 hover:underline">
            Profile
          </Link>
          <AuthButton />
        </nav>
      </header>
      <AuthError />
      <main className="p-6">
        <Outlet />
      </main>
    </>
  )
}
