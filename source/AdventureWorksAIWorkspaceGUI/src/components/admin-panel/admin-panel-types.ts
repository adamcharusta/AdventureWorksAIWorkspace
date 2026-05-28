import type { UserDto } from '@/api/generated/model'

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
