import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { type FormEvent, useMemo, useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'

import { ApiError } from '../api/customFetch'
import { useSetFirstPassword } from '../api/generated/authentication/authentication'
import type { HttpValidationProblemDetails } from '../api/generated/model'
import { ThemeModeSwitch } from '../components/ThemeModeSwitch'
import { toast } from '../lib/toast'
import { useAuth } from '../lib/use-auth'

type SetFirstPasswordLocationState = {
  identifier?: string
  fromPath?: string
}

function getValidationMessage(problem: HttpValidationProblemDetails): string {
  const firstError = Object.values(problem.errors ?? {})[0]?.[0]
  return firstError ?? 'Please verify your data and try again.'
}

function isValidationProblem(
  body: unknown,
): body is HttpValidationProblemDetails {
  if (!body || typeof body !== 'object') {
    return false
  }

  return 'errors' in body
}

export function SetFirstPasswordPage() {
  const { isAuthenticated, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const setFirstPasswordMutation = useSetFirstPassword()

  const state = location.state as SetFirstPasswordLocationState | null
  const identifier = state?.identifier ?? ''
  const fromPath = state?.fromPath ?? '/'

  const [newPassword, setNewPassword] = useState('')
  const [confirmNewPassword, setConfirmNewPassword] = useState('')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const isMissingIdentifier = useMemo(
    () => identifier.trim().length === 0,
    [identifier],
  )

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  if (isMissingIdentifier) {
    return <Navigate to="/login" replace />
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)

    if (newPassword !== confirmNewPassword) {
      setErrorMessage('Passwords do not match.')
      return
    }

    try {
      const response = await setFirstPasswordMutation.mutateAsync({
        data: {
          identifier,
          newPassword,
          confirmNewPassword,
        },
      })

      if (response.status !== 200) {
        setErrorMessage('Unable to set a new password. Please try again.')
        return
      }

      login(response.data.accessToken, response.data.refreshToken)
      toast.success('Password updated. You are now signed in.', 'Welcome')
      navigate(fromPath, { replace: true })
    } catch (error) {
      if (
        error instanceof ApiError &&
        error.status === 400 &&
        isValidationProblem(error.body)
      ) {
        setErrorMessage(getValidationMessage(error.body))
        return
      }

      if (
        error instanceof ApiError &&
        (error.status === 401 || error.status === 403)
      ) {
        setErrorMessage(
          'This password reset link is no longer valid. Sign in again.',
        )
        return
      }

      setErrorMessage(
        'Unable to set your password right now. Please try again later.',
      )
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
                Set your password
              </Typography>
              <Typography variant="body2" color="text.secondary">
                This account requires a password update on first sign in.
              </Typography>
            </Box>

            {errorMessage ? (
              <Alert severity="error">{errorMessage}</Alert>
            ) : null}

            <TextField
              label="Identifier"
              value={identifier}
              fullWidth
              disabled
            />

            <TextField
              label="New password"
              type="password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
              required
              fullWidth
              autoComplete="new-password"
            />

            <TextField
              label="Confirm new password"
              type="password"
              value={confirmNewPassword}
              onChange={(event) => setConfirmNewPassword(event.target.value)}
              required
              fullWidth
              autoComplete="new-password"
            />

            <Button
              type="submit"
              variant="contained"
              disabled={setFirstPasswordMutation.isPending}
            >
              {setFirstPasswordMutation.isPending
                ? 'Saving...'
                : 'Save password'}
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Container>
  )
}
