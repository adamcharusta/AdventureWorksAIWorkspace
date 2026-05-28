import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import { type ComponentPropsWithoutRef, useMemo, useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'

import { ApiError } from '@/api/customFetch'
import { useSetFirstPassword } from '@/api/generated/authentication/authentication'
import { AuthCard } from '@/components/auth/AuthCard'
import { PasswordInput } from '@/components/auth/PasswordInput'
import { useAuth } from '@/hooks/use-auth'
import { getApiErrorMessage } from '@/lib/api-error'
import { toast } from '@/lib/toast'

type SetFirstPasswordLocationState = {
  identifier?: string
  fromPath?: string
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
  const [showPassword, setShowPassword] = useState(false)

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

  const handleSubmit: NonNullable<
    ComponentPropsWithoutRef<'form'>['onSubmit']
  > = async (event) => {
    event.preventDefault()

    if (newPassword !== confirmNewPassword) {
      toast.error('Passwords do not match.', 'Set password')
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
        toast.error(
          'Unable to set a new password. Please try again.',
          'Set password',
        )
        return
      }

      login(response.data.accessToken, response.data.refreshToken)
      toast.success('Password updated. You are now signed in.', 'Welcome')
      navigate(fromPath, { replace: true })
    } catch (error) {
      if (error instanceof ApiError && error.status === 400) {
        toast.error(
          getApiErrorMessage(error, 'Please verify your data and try again.'),
          'Set password',
        )
        return
      }

      if (
        error instanceof ApiError &&
        (error.status === 401 || error.status === 403)
      ) {
        toast.error(
          'This password reset link is no longer valid. Sign in again.',
          'Set password',
        )
        return
      }

      toast.error(
        'Unable to set your password right now. Please try again later.',
        'Set password',
      )
    }
  }

  return (
    <AuthCard
      title="Set your password"
      subtitle="This account requires a password update on first sign in."
      onSubmit={handleSubmit}
    >
      <TextField label="Identifier" value={identifier} fullWidth disabled />

      <PasswordInput
        label="New password"
        value={newPassword}
        onChange={(event) => setNewPassword(event.target.value)}
        required
        fullWidth
        showPassword={showPassword}
        handleClickShowPassword={() => setShowPassword((prev) => !prev)}
        autoComplete="new-password"
      />

      <PasswordInput
        label="Confirm new password"
        value={confirmNewPassword}
        onChange={(event) => setConfirmNewPassword(event.target.value)}
        required
        fullWidth
        showPassword={showPassword}
        handleClickShowPassword={() => setShowPassword((prev) => !prev)}
        autoComplete="new-password"
      />

      <Button
        type="submit"
        variant="contained"
        disabled={setFirstPasswordMutation.isPending}
      >
        {setFirstPasswordMutation.isPending ? 'Saving...' : 'Save password'}
      </Button>
    </AuthCard>
  )
}
