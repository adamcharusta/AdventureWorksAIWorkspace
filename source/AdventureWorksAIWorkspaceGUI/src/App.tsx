import { Route, Routes } from 'react-router-dom'

import { RequireAdmin } from './components/auth/RequireAdmin'
import { RequireAuth } from './components/auth/RequireAuth'
import AdminPanelPage from './pages/AdminPanelPage'
import HomePage from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { SetFirstPasswordPage } from './pages/SetFirstPasswordPage'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/set-first-password" element={<SetFirstPasswordPage />} />

      <Route element={<RequireAuth />}>
        <Route path="/" element={<HomePage />} />
        <Route element={<RequireAdmin />}>
          <Route path="/admin" element={<AdminPanelPage />} />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default App
