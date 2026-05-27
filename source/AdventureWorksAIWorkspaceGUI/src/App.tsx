import { Navigate, Route, Routes } from 'react-router-dom'

import { RequireAuth } from './components/RequireAuth'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { SetFirstPasswordPage } from './pages/SetFirstPasswordPage'
import { WeatherForecastsPage } from './pages/WeatherForecastsPage'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/set-first-password" element={<SetFirstPasswordPage />} />

      <Route element={<RequireAuth />}>
        <Route path="/" element={<WeatherForecastsPage />} />
        <Route path="/weather" element={<Navigate to="/" replace />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default App
