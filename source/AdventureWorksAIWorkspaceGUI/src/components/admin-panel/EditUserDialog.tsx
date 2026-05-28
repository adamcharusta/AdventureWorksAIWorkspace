import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import FormControlLabel from '@mui/material/FormControlLabel'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import type { SubmitEvent } from 'react'

import type {
  EditUserFormState,
  FormFieldChangeHandler,
  RoleFieldState,
} from './admin-panel-types'
import { RoleSelectField } from './RoleSelectField'

type EditUserDialogProps = {
  form: EditUserFormState
  isSaving: boolean
  onChange: FormFieldChangeHandler<EditUserFormState>
  onClose: () => void
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void
  open: boolean
  roleFieldState: RoleFieldState
  roleOptions: string[]
}

export function EditUserDialog({
  form,
  isSaving,
  onChange,
  onClose,
  onSubmit,
  open,
  roleFieldState,
  roleOptions,
}: EditUserDialogProps) {
  return (
    <Dialog fullWidth maxWidth="sm" onClose={onClose} open={open}>
      <DialogTitle>Edit user</DialogTitle>
      <DialogContent>
        <Stack
          component="form"
          id="edit-user-form"
          onSubmit={onSubmit}
          spacing={2}
          sx={{ pt: 1 }}
        >
          <TextField
            autoComplete="username"
            label="Username"
            onChange={(event) => onChange('userName', event.target.value)}
            required
            value={form.userName}
          />
          <TextField
            autoComplete="email"
            label="Email"
            onChange={(event) => onChange('email', event.target.value)}
            required
            type="email"
            value={form.email}
          />
          <RoleSelectField
            fieldState={roleFieldState}
            onChange={(role) => onChange('role', role)}
            roleOptions={roleOptions}
            value={form.role}
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={form.resetPassword}
                onChange={(event) =>
                  onChange('resetPassword', event.target.checked)
                }
              />
            }
            label="Reset password to the template password"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          disabled={isSaving}
          form="edit-user-form"
          type="submit"
          variant="contained"
        >
          {isSaving ? 'Saving...' : 'Save changes'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
