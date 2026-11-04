import { Route, Routes } from 'react-router'
import { AdminLayout } from './admin/AdminLayout'
import { RequirePermission } from './auth/RequirePermission'
import { RequireAuth } from './auth/RequireAuth'
import { Layout } from './components/Layout'
import { CitiesPage } from './pages/CitiesPage'
import { AdminCitiesPage } from './pages/admin/AdminCitiesPage'
import { AdminCityCreatePage } from './pages/admin/AdminCityCreatePage'
import { AdminCityEditPage } from './pages/admin/AdminCityEditPage'
import { AdminIdeasPage } from './pages/admin/AdminIdeasPage'
import { AdminPollCreatePage } from './pages/admin/AdminPollCreatePage'
import { AdminPollEditPage } from './pages/admin/AdminPollEditPage'
import { AdminPollsPage } from './pages/admin/AdminPollsPage'
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
              <Route element={<RequirePermission permission="cities.create.any" />}>
                <Route path="cities/new" element={<AdminCityCreatePage />} />
              </Route>
              <Route element={<RequirePermission permission="cities.update.any" />}>
                <Route path="cities/:cityId/edit" element={<AdminCityEditPage />} />
              </Route>
            </Route>
            <Route element={<RequirePermission permission="polls.read.any" />}>
              <Route path="polls" element={<AdminPollsPage />} />
              <Route element={<RequirePermission permission="polls.create.any" />}>
                <Route path="polls/new" element={<AdminPollCreatePage />} />
              </Route>
              <Route element={<RequirePermission permission="polls.update.any" />}>
                <Route path="polls/:pollId/edit" element={<AdminPollEditPage />} />
              </Route>
            </Route>
            <Route element={<RequirePermission permission="ideas.read.any" />}>
              <Route path="ideas" element={<AdminIdeasPage />} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
