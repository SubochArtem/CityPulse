import { AuthButton } from './components/AuthButton'

export default function App() {
  return (
    <header className="flex items-center justify-between p-6">
      <h1 className="text-2xl font-semibold text-slate-900">CityPulse</h1>
      <AuthButton />
    </header>
  )
}
