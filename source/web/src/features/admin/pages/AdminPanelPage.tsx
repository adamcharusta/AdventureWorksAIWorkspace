import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'

import { AdminCreateUserForm } from '@/features/admin/components/AdminCreateUserForm'
import { AdminPanelHeader } from '@/features/admin/components/AdminPanelHeader'
import { AdminUsersSection } from '@/features/admin/components/AdminUsersSection'
import { DeleteUserDialog } from '@/features/admin/components/DeleteUserDialog'
import { EditUserDialog } from '@/features/admin/components/EditUserDialog'
import { useAdminPanelController } from '@/features/admin/hooks/use-admin-panel-controller'

const AdminPanelPage = () => {
  const admin = useAdminPanelController()

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
          <AdminPanelHeader
            onGoHome={admin.handleGoHome}
            onLogout={admin.handleLogout}
          />

          <AdminCreateUserForm
            form={admin.createForm}
            isSubmitting={admin.isCreating}
            onChange={admin.handleCreateFormChange}
            onSubmit={admin.handleCreateUser}
            roleFieldState={admin.roleFieldState}
            roleOptions={admin.createRoleOptions}
            roleValue={admin.createRoleValue}
          />

          <AdminUsersSection
            currentUserId={admin.currentUserId}
            isError={admin.usersError}
            isLoading={admin.usersLoading}
            onDelete={admin.handleStartDelete}
            onEdit={admin.handleStartEdit}
            users={admin.users}
          />
        </Stack>
      </Box>

      <EditUserDialog
        form={admin.editForm}
        isSaving={admin.isSaving}
        onChange={admin.handleEditFormChange}
        onClose={admin.handleCloseEdit}
        onSubmit={admin.handleUpdateUser}
        open={Boolean(admin.editingUser)}
        roleFieldState={admin.roleFieldState}
        roleOptions={admin.editRoleOptions}
      />

      <DeleteUserDialog
        isDeleting={admin.isDeleting}
        onClose={admin.handleCloseDelete}
        onConfirm={admin.handleDeleteUser}
        user={admin.deletingUser}
      />
    </Box>
  )
}

export default AdminPanelPage
