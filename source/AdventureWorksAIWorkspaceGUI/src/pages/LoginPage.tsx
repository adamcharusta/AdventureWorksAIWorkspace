import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import { type ComponentPropsWithoutRef, useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'

import { ApiError } from '../api/customFetch'
import { useLogin } from '../api/generated/authentication/authentication'
import { AuthCard } from '../components/AuthCard'
import { PasswordInput } from '../components/PasswordInput'
import { useAuth } from '../hooks/use-auth'
import { toast } from '../lib/toast'

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
  const [showPassword, setShowPassword] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  const fromPath =
    (location.state as LocationState | null)?.from?.pathname ?? '/'

  const handleSubmit: NonNullable<
    ComponentPropsWithoutRef<'form'>['onSubmit']
  > = async (event) => {
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
    <AuthCard
      title="Sign in"
      subtitle="Use your username or email and password."
      onSubmit={handleSubmit}
    >
      {errorMessage ? <Alert severity="error">{errorMessage}</Alert> : null}

      <TextField
        label="Username or email"
        value={identifier}
        onChange={(event) => setIdentifier(event.target.value)}
        required
        fullWidth
        autoComplete="username"
      />

      <PasswordInput
        label="Password"
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        required
        fullWidth
        showPassword={showPassword}
        handleClickShowPassword={() => setShowPassword((prev) => !prev)}
      />

      <Button
        type="submit"
        variant="contained"
        disabled={loginMutation.isPending}
      >
        {loginMutation.isPending ? 'Signing in...' : 'Sign in'}
      </Button>
    </AuthCard>
  )
}
