import { Route, Routes } from 'react-router-dom'

import AdminPanelPage from '@/features/admin/pages/AdminPanelPage'
import { RequireAdmin } from '@/features/auth/components/RequireAdmin'
import { RequireAuth } from '@/features/auth/components/RequireAuth'
import { LoginPage } from '@/features/auth/pages/LoginPage'
import { SetFirstPasswordPage } from '@/features/auth/pages/SetFirstPasswordPage'
import HomePage from '@/features/workspace/pages/HomePage'

import { NotFoundPage } from './pages/NotFoundPage'

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
