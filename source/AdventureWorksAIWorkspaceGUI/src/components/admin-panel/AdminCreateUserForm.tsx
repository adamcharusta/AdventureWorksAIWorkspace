import PersonAddRoundedIcon from '@mui/icons-material/PersonAddRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import type { SubmitEvent } from 'react'

import type {
  CreateUserFormState,
  FormFieldChangeHandler,
  RoleFieldState,
} from './admin-panel-types'
import { AdminSectionHeader } from './AdminSectionHeader'
import { RoleSelectField } from './RoleSelectField'

type AdminCreateUserFormProps = {
  form: CreateUserFormState
  isSubmitting: boolean
  onChange: FormFieldChangeHandler<CreateUserFormState>
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void
  roleFieldState: RoleFieldState
  roleOptions: string[]
  roleValue: string
}

export function AdminCreateUserForm({
  form,
  isSubmitting,
  onChange,
  onSubmit,
  roleFieldState,
  roleOptions,
  roleValue,
}: AdminCreateUserFormProps) {
  return (
    <Box
      aria-label="Add new user"
      component="form"
      onSubmit={onSubmit}
      sx={{
        borderColor: 'divider',
        borderRadius: 2,
        borderStyle: 'solid',
        borderWidth: 1,
        p: { xs: 2, md: 3 },
      }}
    >
      <Stack spacing={2}>
        <AdminSectionHeader
          description="Created users receive the configured first-login password flow."
          title="Add new user"
        />

        <Stack
          direction={{ xs: 'column', md: 'row' }}
          spacing={2}
          sx={{
            alignItems: { xs: 'stretch', md: 'flex-start' },
          }}
        >
          <Box sx={{ flexGrow: 3, minWidth: 0, display: 'flex', gap: 2 }}>
            <TextField
              autoComplete="username"
              label="Username"
              onChange={(event) => onChange('userName', event.target.value)}
              required
              size="small"
              fullWidth
              value={form.userName}
            />
            <TextField
              autoComplete="email"
              label="Email"
              onChange={(event) => onChange('email', event.target.value)}
              required
              size="small"
              type="email"
              fullWidth
              value={form.email}
            />
            <RoleSelectField
              fieldState={roleFieldState}
              fullWidth
              onChange={(role) => onChange('role', role)}
              roleOptions={roleOptions}
              size="small"
              value={roleValue}
            />
          </Box>
          <Button
            disabled={
              isSubmitting ||
              roleFieldState.isLoading ||
              roleFieldState.isUnavailable
            }
            startIcon={<PersonAddRoundedIcon />}
            sx={{ minHeight: 40, flexGrow: 1, width: 40 }}
            type="submit"
            variant="contained"
            fullWidth
          >
            {isSubmitting ? 'Creating...' : 'Create user'}
          </Button>
        </Stack>
      </Stack>
    </Box>
  )
}
