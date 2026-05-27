import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { type FormEvent, useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'

import { ApiError } from '../api/customFetch'
import { useLogin } from '../api/generated/authentication/authentication'
import { ThemeModeSwitch } from '../components/ThemeModeSwitch'
import { toast } from '../lib/toast'
import { useAuth } from '../lib/use-auth'

type LocationState = {
  from?: {
    pathname?: string
  }
}

export function LoginPage() {
  const { isAuthenticated, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const loginMutation = useLogin()

  const [identifier, setIdentifier] = useState('')
  const [password, setPassword] = useState('')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  const fromPath =
    (location.state as LocationState | null)?.from?.pathname ?? '/'

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)

    try {
      const response = await loginMutation.mutateAsync({
        data: { identifier, password },
      })

      if (response.status !== 200) {
        if (response.status === 401) {
          setErrorMessage('Invalid credentials. Please try again.')
          return
        }

        setErrorMessage('Unable to sign in right now. Please try again later.')
        return
      }

      login(response.data.accessToken, response.data.refreshToken)
      toast.success('You are now signed in.', 'Welcome back')
      navigate(fromPath, { replace: true })
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        setErrorMessage('Invalid credentials. Please try again.')
        return
      }

      if (error instanceof ApiError && error.status === 403) {
        navigate('/set-first-password', {
          replace: true,
          state: { identifier, fromPath },
        })
        return
      }

      setErrorMessage('Unable to sign in right now. Please try again later.')
    }
  }

  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Card>
        <CardContent>
          <Stack component="form" spacing={2} onSubmit={handleSubmit}>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
              <ThemeModeSwitch />
            </Box>

            <Box>
              <Typography variant="h4" component="h1" gutterBottom>
                Sign in
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Use your username or email and password.
              </Typography>
            </Box>

            {errorMessage ? (
              <Alert severity="error">{errorMessage}</Alert>
            ) : null}

            <TextField
              label="Username or email"
              value={identifier}
              onChange={(event) => setIdentifier(event.target.value)}
              required
              fullWidth
              autoComplete="username"
            />

            <TextField
              label="Password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              fullWidth
              autoComplete="current-password"
            />

            <Button
              type="submit"
              variant="contained"
              disabled={loginMutation.isPending}
            >
              {loginMutation.isPending ? 'Signing in...' : 'Sign in'}
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Container>
  )
}
