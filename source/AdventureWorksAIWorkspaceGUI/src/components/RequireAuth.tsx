import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import { useEffect, useState } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useAuth } from '../hooks/use-auth'
import { ensureValidSession } from '../lib/auth-service'

export function RequireAuth() {
  const location = useLocation()
  const { isAuthenticated } = useAuth()
  const [isCheckingSession, setIsCheckingSession] = useState(true)
  const [isSessionValid, setIsSessionValid] = useState(false)

  useEffect(() => {
    let isActive = true

    const validateSession = async () => {
      if (!isAuthenticated) {
        if (isActive) {
          setIsSessionValid(false)
          setIsCheckingSession(false)
        }
        return
      }

      const valid = await ensureValidSession()
      if (isActive) {
        setIsSessionValid(valid)
        setIsCheckingSession(false)
      }
    }

    void validateSession()

    return () => {
      isActive = false
    }
  }, [isAuthenticated])

  if (isCheckingSession) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!isSessionValid) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return <Outlet />
}
