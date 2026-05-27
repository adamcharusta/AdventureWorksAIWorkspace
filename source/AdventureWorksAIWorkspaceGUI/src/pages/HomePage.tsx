import { useNavigate } from 'react-router-dom'

import { toast } from '../lib/toast'
import { useAuth } from '../lib/use-auth'

const HomePage = () => {
  const navigate = useNavigate()
  const { logout } = useAuth()

  const handleLogout = () => {
    logout()
    toast.info('Your session has been closed.', 'Signed out')
    navigate('/login', { replace: true })
  }

  return (
    <div className="home-page">
      <header className="header">
        <h1>Adventure Works</h1>
        <p>Welcome to the AI Workspace</p>
        <button type="button" onClick={handleLogout}>
          Log out
        </button>
      </header>

      <main className="main-content">
        <section className="hero">
          <h2>Get Started</h2>
          <p>Explore our features and services</p>
        </section>

        <section className="features">
          <div className="feature-card">
            <h3>Feature 1</h3>
            <p>Description of feature 1</p>
          </div>
          <div className="feature-card">
            <h3>Feature 2</h3>
            <p>Description of feature 2</p>
          </div>
          <div className="feature-card">
            <h3>Feature 3</h3>
            <p>Description of feature 3</p>
          </div>
        </section>
      </main>

      <footer className="footer">
        <p>&copy; 2024 Adventure Works. All rights reserved.</p>
      </footer>
    </div>
  )
}

export default HomePage
