import MenuItem from '@mui/material/MenuItem'
import TextField, { type TextFieldProps } from '@mui/material/TextField'

import type { RoleFieldState } from './admin-panel-types'

type RoleSelectFieldProps = {
  fieldState: RoleFieldState
  fullWidth?: boolean
  onChange: (role: string) => void
  roleOptions: string[]
  size?: TextFieldProps['size']
  value: string
}

export function RoleSelectField({
  fieldState,
  fullWidth,
  onChange,
  roleOptions,
  size,
  value,
}: RoleSelectFieldProps) {
  return (
    <TextField
      disabled={fieldState.isLoading || fieldState.isUnavailable}
      fullWidth={fullWidth}
      helperText={fieldState.helperText}
      label="Role"
      onChange={(event) => onChange(event.target.value)}
      select
      size={size}
      value={value}
    >
      {roleOptions.map((role) => (
        <MenuItem key={role} value={role}>
          {role}
        </MenuItem>
      ))}
    </TextField>
  )
}
