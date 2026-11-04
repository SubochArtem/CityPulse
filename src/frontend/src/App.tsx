import { Route, Routes } from 'react-router'
import { RequirePermission } from './auth/RequirePermission'
import { RequireAuth } from './auth/RequireAuth'
import { Layout } from './components/Layout'
import { lazyPage } from './lib/lazyPage'

const AdminLayout = lazyPage(() => import('./admin/AdminLayout'), 'AdminLayout')
const CitiesPage = lazyPage(() => import('./pages/CitiesPage'), 'CitiesPage')
const AdminCitiesPage = lazyPage(() => import('./pages/admin/AdminCitiesPage'), 'AdminCitiesPage')
const AdminCityCreatePage = lazyPage(
  () => import('./pages/admin/AdminCityCreatePage'),
  'AdminCityCreatePage',
)
const AdminCityEditPage = lazyPage(
  () => import('./pages/admin/AdminCityEditPage'),
  'AdminCityEditPage',
)
const AdminIdeasPage = lazyPage(() => import('./pages/admin/AdminIdeasPage'), 'AdminIdeasPage')
const AdminPollCreatePage = lazyPage(
  () => import('./pages/admin/AdminPollCreatePage'),
  'AdminPollCreatePage',
)
const AdminPollEditPage = lazyPage(
  () => import('./pages/admin/AdminPollEditPage'),
  'AdminPollEditPage',
)
const AdminPollsPage = lazyPage(() => import('./pages/admin/AdminPollsPage'), 'AdminPollsPage')
const AdminUsersPage = lazyPage(() => import('./pages/admin/AdminUsersPage'), 'AdminUsersPage')
const CityPage = lazyPage(() => import('./pages/CityPage'), 'CityPage')
const CreateIdeaPage = lazyPage(() => import('./pages/CreateIdeaPage'), 'CreateIdeaPage')
const HomePage = lazyPage(() => import('./pages/HomePage'), 'HomePage')
const IdeaPage = lazyPage(() => import('./pages/IdeaPage'), 'IdeaPage')
const NotFoundPage = lazyPage(() => import('./pages/NotFoundPage'), 'NotFoundPage')
const PollPage = lazyPage(() => import('./pages/PollPage'), 'PollPage')
const ProfilePage = lazyPage(() => import('./pages/ProfilePage'), 'ProfilePage')

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
            <Route element={<RequirePermission permission="users.read.any" />}>
              <Route path="users" element={<AdminUsersPage />} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
