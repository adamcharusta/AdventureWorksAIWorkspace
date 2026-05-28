import type {
  CreateUserFormState,
  EditUserFormState,
} from './admin-panel-types'

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
