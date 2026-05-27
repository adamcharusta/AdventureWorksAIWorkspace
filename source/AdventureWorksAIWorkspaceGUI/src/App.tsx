import { Navigate, Route, Routes } from 'react-router-dom'

import { NotFoundPage } from './pages/NotFoundPage'
import { WeatherForecastsPage } from './pages/WeatherForecastsPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<WeatherForecastsPage />} />
      <Route path="/weather" element={<Navigate to="/" replace />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default App
