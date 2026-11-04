import { Route, Routes } from 'react-router'
import { AdminLayout } from './admin/AdminLayout'
import { RequirePermission } from './auth/RequirePermission'
import { RequireAuth } from './auth/RequireAuth'
import { Layout } from './components/Layout'
import { CitiesPage } from './pages/CitiesPage'
import { AdminCitiesPage } from './pages/admin/AdminCitiesPage'
import { CityPage } from './pages/CityPage'
import { CreateIdeaPage } from './pages/CreateIdeaPage'
import { HomePage } from './pages/HomePage'
import { IdeaPage } from './pages/IdeaPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { PollPage } from './pages/PollPage'
import { ProfilePage } from './pages/ProfilePage'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="cities" element={<CitiesPage />} />
        <Route path="cities/:cityId" element={<CityPage />} />
        <Route path="polls/:pollId" element={<PollPage />} />
        <Route path="ideas/:ideaId" element={<IdeaPage />} />
        <Route element={<RequireAuth />}>
          <Route path="profile" element={<ProfilePage />} />
          <Route path="polls/:pollId/ideas/new" element={<CreateIdeaPage />} />
          <Route path="admin" element={<AdminLayout />}>
            <Route element={<RequirePermission permission="cities.read.any" />}>
              <Route path="cities" element={<AdminCitiesPage />} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
