import type { UserDto } from '../../api/generated/model'

export type GetAssignableRolesResponse = {
  roles: string[]
}

export type GetAssignableRolesApiResponse = {
  data: GetAssignableRolesResponse
  status: 200
  headers: Headers
}

export type CreateUserFormState = {
  userName: string
  email: string
  role: string
}

export type EditUserFormState = CreateUserFormState & {
  resetPassword: boolean
}

export type RoleFieldState = {
  helperText?: string
  isLoading: boolean
  isUnavailable: boolean
}

export type FormFieldChangeHandler<TForm> = <TField extends keyof TForm>(
  field: TField,
  value: TForm[TField],
) => void

export type UserActionHandler = (user: UserDto) => void

export const adminRequestOptions = {
  clearSessionOnUnauthorized: false,
  skipAuthRefresh: true,
} as const

export const emptyRoles: string[] = []

export const initialCreateForm: CreateUserFormState = {
  userName: '',
  email: '',
  role: '',
}

export const initialEditForm: EditUserFormState = {
  userName: '',
  email: '',
  role: 'User',
  resetPassword: false,
}

export function getRoleOptions(
  assignableRoles: string[],
  selectedRole: string,
) {
  if (!selectedRole || assignableRoles.includes(selectedRole)) {
    return assignableRoles
  }

  return [selectedRole, ...assignableRoles]
}

export function getCreateRoleValue(
  selectedRole: string,
  assignableRoles: string[],
) {
  if (selectedRole && assignableRoles.includes(selectedRole)) {
    return selectedRole
  }

  return assignableRoles[0] ?? ''
}
