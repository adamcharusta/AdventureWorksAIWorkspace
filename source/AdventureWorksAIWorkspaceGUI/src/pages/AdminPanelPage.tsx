import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { type SubmitEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { customFetch } from '@/api/customFetch'
import type { UserDto } from '@/api/generated/model'
import {
  getGetUsersQueryKey,
  useCreateUser,
  useDeleteUser,
  useGetUsers,
  useUpdateUser,
} from '@/api/generated/users/users'
import type {
  CreateUserFormState,
  EditUserFormState,
  GetAssignableRolesApiResponse,
} from '@/components/admin-panel/admin-panel-types'
import {
  adminRequestOptions,
  emptyRoles,
  getCreateRoleValue,
  getRoleOptions,
  initialCreateForm,
  initialEditForm,
} from '@/components/admin-panel/admin-panel-utils'
import { AdminCreateUserForm } from '@/components/admin-panel/AdminCreateUserForm'
import { AdminUsersSection } from '@/components/admin-panel/AdminUsersSection'
import { DeleteUserDialog } from '@/components/admin-panel/DeleteUserDialog'
import { EditUserDialog } from '@/components/admin-panel/EditUserDialog'
import { ThemeModeSwitch } from '@/components/theme/ThemeModeSwitch'
import { useAuth } from '@/hooks/use-auth'
import { getApiErrorMessage } from '@/lib/api-error'
import { toast } from '@/lib/toast'

const AdminPanelPage = () => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { userId: currentUserId, logout } = useAuth()
  const [createForm, setCreateForm] =
    useState<CreateUserFormState>(initialCreateForm)
  const [editingUser, setEditingUser] = useState<UserDto | null>(null)
  const [editForm, setEditForm] = useState<EditUserFormState>(initialEditForm)
  const [deletingUser, setDeletingUser] = useState<UserDto | null>(null)

  const rolesQuery = useQuery({
    queryKey: ['/api/users/roles'],
    queryFn: ({ signal }) =>
      customFetch<GetAssignableRolesApiResponse>('/api/users/roles', {
        ...adminRequestOptions,
        method: 'GET',
        signal,
      }),
    retry: false,
  })

  const usersQuery = useGetUsers({
    query: {
      retry: false,
    },
    request: adminRequestOptions,
  })

  const createUserMutation = useCreateUser({
    mutation: {
      onSuccess: () => {
        setCreateForm(initialCreateForm)
        toast.success('User account has been created.', 'Admin panel')
        void queryClient.invalidateQueries({ queryKey: getGetUsersQueryKey() })
      },
      onError: (error) => {
        toast.error(
          getApiErrorMessage(error, 'The user account could not be created.'),
          'Admin panel',
        )
      },
    },
    request: adminRequestOptions,
  })

  const updateUserMutation = useUpdateUser({
    mutation: {
      onSuccess: () => {
        setEditingUser(null)
        setEditForm(initialEditForm)
        toast.success('User account has been updated.', 'Admin panel')
        void queryClient.invalidateQueries({ queryKey: getGetUsersQueryKey() })
      },
      onError: (error) => {
        toast.error(
          getApiErrorMessage(error, 'The user account could not be updated.'),
          'Admin panel',
        )
      },
    },
    request: adminRequestOptions,
  })

  const deleteUserMutation = useDeleteUser({
    mutation: {
      onSuccess: () => {
        setDeletingUser(null)
        toast.success('User account has been deleted.', 'Admin panel')
        void queryClient.invalidateQueries({ queryKey: getGetUsersQueryKey() })
      },
      onError: (error) => {
        toast.error(
          getApiErrorMessage(error, 'The user account could not be deleted.'),
          'Admin panel',
        )
      },
    },
    request: adminRequestOptions,
  })

  const users =
    usersQuery.data?.status === 200 ? usersQuery.data.data.users : []
  const assignableRoles =
    rolesQuery.data?.status === 200 ? rolesQuery.data.data.roles : emptyRoles
  const createRoleValue = getCreateRoleValue(createForm.role, assignableRoles)
  const createRoleOptions = getRoleOptions(assignableRoles, createRoleValue)
  const editRoleOptions = getRoleOptions(assignableRoles, editForm.role)
  const rolesAreUnavailable =
    rolesQuery.isError || (rolesQuery.isSuccess && assignableRoles.length === 0)
  const roleFieldState = {
    helperText: rolesQuery.isLoading
      ? 'Loading roles...'
      : rolesQuery.isError
        ? 'Unable to load roles.'
        : rolesQuery.isSuccess && assignableRoles.length === 0
          ? 'No assignable roles available.'
          : undefined,
    isLoading: rolesQuery.isLoading,
    isUnavailable: rolesAreUnavailable,
  }

  const handleCreateFormChange = <TField extends keyof CreateUserFormState>(
    field: TField,
    value: CreateUserFormState[TField],
  ) => {
    setCreateForm((current) => ({
      ...current,
      [field]: value,
    }))
  }

  const handleEditFormChange = <TField extends keyof EditUserFormState>(
    field: TField,
    value: EditUserFormState[TField],
  ) => {
    setEditForm((current) => ({
      ...current,
      [field]: value,
    }))
  }

  const handleCreateUser = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    createUserMutation.mutate({
      data: {
        userName: createForm.userName.trim(),
        email: createForm.email.trim(),
        role: createRoleValue,
      },
    })
  }

  const handleStartEdit = (user: UserDto) => {
    setEditingUser(user)
    setEditForm({
      userName: user.userName,
      email: user.email,
      role: user.role || 'User',
      resetPassword: false,
    })
  }

  const handleCloseEdit = () => {
    if (!updateUserMutation.isPending) {
      setEditingUser(null)
      setEditForm(initialEditForm)
    }
  }

  const handleUpdateUser = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!editingUser) {
      return
    }

    updateUserMutation.mutate({
      userId: editingUser.id,
      data: {
        userId: editingUser.id,
        userName: editForm.userName.trim(),
        email: editForm.email.trim(),
        role: editForm.role,
        resetPassword: editForm.resetPassword,
      },
    })
  }

  const handleStartDelete = (user: UserDto) => {
    if (currentUserId && user.id === currentUserId) {
      toast.error('You cannot delete your own account.', 'Admin panel')
      return
    }

    setDeletingUser(user)
  }

  const handleCloseDelete = () => {
    if (!deleteUserMutation.isPending) {
      setDeletingUser(null)
    }
  }

  const handleDeleteUser = () => {
    if (!deletingUser) {
      return
    }

    deleteUserMutation.mutate({
      userId: deletingUser.id,
    })
  }

  const handleGoHome = () => {
    navigate('/')
  }

  const handleLogout = () => {
    logout()
    toast.info('Your session has been closed.', 'Signed out')
    navigate('/login', { replace: true })
  }

  return (
    <Box
      sx={{
        bgcolor: 'background.default',
        color: 'text.primary',
        display: 'flex',
        minHeight: '100vh',
      }}
    >
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          minWidth: 0,
        }}
      >
        <Stack
          spacing={3}
          sx={{
            mx: 'auto',
            maxWidth: 1240,
            px: { xs: 2, md: 4 },
            py: { xs: 2, md: 3 },
          }}
        >
          <Box
            sx={{
              alignItems: 'center',
              display: 'flex',
              gap: 2,
              flexWrap: 'wrap',
              justifyContent: 'space-between',
            }}
          >
            <Box>
              <Typography component="h1" sx={{ fontWeight: 700 }} variant="h4">
                Admin Panel
              </Typography>
              <Typography color="text.secondary" variant="body2">
                Manage application users, roles, and account resets.
              </Typography>
            </Box>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <Tooltip title="Back to home">
                <IconButton
                  aria-label="Back to home"
                  color="inherit"
                  onClick={handleGoHome}
                >
                  <HomeRoundedIcon />
                </IconButton>
              </Tooltip>
              <ThemeModeSwitch />
              <Tooltip title="Log out">
                <IconButton
                  aria-label="Log out"
                  color="inherit"
                  onClick={handleLogout}
                >
                  <LogoutRoundedIcon />
                </IconButton>
              </Tooltip>
            </Stack>
          </Box>

          <AdminCreateUserForm
            form={createForm}
            isSubmitting={createUserMutation.isPending}
            onChange={handleCreateFormChange}
            onSubmit={handleCreateUser}
            roleFieldState={roleFieldState}
            roleOptions={createRoleOptions}
            roleValue={createRoleValue}
          />

          <AdminUsersSection
            currentUserId={currentUserId}
            isError={usersQuery.isError}
            isLoading={usersQuery.isLoading}
            onDelete={handleStartDelete}
            onEdit={handleStartEdit}
            users={users}
          />
        </Stack>
      </Box>

      <EditUserDialog
        form={editForm}
        isSaving={updateUserMutation.isPending}
        onChange={handleEditFormChange}
        onClose={handleCloseEdit}
        onSubmit={handleUpdateUser}
        open={Boolean(editingUser)}
        roleFieldState={roleFieldState}
        roleOptions={editRoleOptions}
      />

      <DeleteUserDialog
        isDeleting={deleteUserMutation.isPending}
        onClose={handleCloseDelete}
        onConfirm={handleDeleteUser}
        user={deletingUser}
      />
    </Box>
  )
}

export default AdminPanelPage
