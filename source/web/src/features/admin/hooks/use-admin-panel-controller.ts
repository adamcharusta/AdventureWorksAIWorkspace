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
} from '@/features/admin/components/admin-panel-types'
import {
  adminRequestOptions,
  emptyRoles,
  getCreateRoleValue,
  getRoleOptions,
  initialCreateForm,
  initialEditForm,
} from '@/features/admin/components/admin-panel-utils'
import { useAuth } from '@/features/auth/hooks/use-auth'
import { getApiErrorMessage } from '@/shared/lib/api-error'
import { toast } from '@/shared/lib/toast'

/**
 * Owns all Admin Panel state, queries, mutations, and event handlers, keeping the page component a
 * thin composition of presentational sections (mirrors the home page's controller-hook pattern).
 */
export function useAdminPanelController() {
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

  return {
    createForm,
    createRoleOptions,
    createRoleValue,
    currentUserId,
    deletingUser,
    editForm,
    editingUser,
    editRoleOptions,
    handleCloseDelete,
    handleCloseEdit,
    handleCreateFormChange,
    handleCreateUser,
    handleDeleteUser,
    handleEditFormChange,
    handleGoHome,
    handleLogout,
    handleStartDelete,
    handleStartEdit,
    handleUpdateUser,
    isCreating: createUserMutation.isPending,
    isDeleting: deleteUserMutation.isPending,
    isSaving: updateUserMutation.isPending,
    roleFieldState,
    users,
    usersError: usersQuery.isError,
    usersLoading: usersQuery.isLoading,
  }
}
