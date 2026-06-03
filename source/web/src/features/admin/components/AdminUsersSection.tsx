import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded'
import EditRoundedIcon from '@mui/icons-material/EditRounded'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'

import type { UserDto } from '@/api/generated/model'

import type { UserActionHandler } from './admin-panel-types'
import { AdminSectionHeader } from './AdminSectionHeader'

type AdminUsersSectionProps = {
  currentUserId?: string | null
  isError: boolean
  isLoading: boolean
  onDelete: UserActionHandler
  onEdit: UserActionHandler
  users: UserDto[]
}

export function AdminUsersSection({
  currentUserId,
  isError,
  isLoading,
  onDelete,
  onEdit,
  users,
}: AdminUsersSectionProps) {
  return (
    <Box
      sx={{
        borderColor: 'divider',
        borderRadius: 2,
        borderStyle: 'solid',
        borderWidth: 1,
        overflow: 'hidden',
      }}
    >
      <Stack spacing={2} sx={{ p: { xs: 2, md: 3 }, pb: 0 }}>
        <AdminSectionHeader
          description="Review accounts and edit usernames, emails, roles, or password reset state."
          title="Users"
        />
      </Stack>

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : isError ? (
        <Box sx={{ p: { xs: 2, md: 3 } }}>
          <Alert severity="error">Unable to load users.</Alert>
        </Box>
      ) : users.length === 0 ? (
        <Box sx={{ p: { xs: 2, md: 3 } }}>
          <Alert severity="info">No users have been created yet.</Alert>
        </Box>
      ) : (
        <TableContainer>
          <Table aria-label="Users">
            <TableHead>
              <TableRow>
                <TableCell>User</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Role</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {users.map((user) => {
                const isCurrentUser =
                  Boolean(currentUserId) && user.id === currentUserId

                return (
                  <TableRow key={user.id} hover>
                    <TableCell>
                      <Typography sx={{ fontWeight: 600 }} variant="body2">
                        {user.userName}
                      </Typography>
                    </TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{user.role}</TableCell>
                    <TableCell align="right">
                      <Tooltip title={`Edit ${user.userName}`}>
                        <IconButton
                          aria-label={`Edit ${user.userName}`}
                          onClick={() => onEdit(user)}
                          size="small"
                        >
                          <EditRoundedIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip
                        title={
                          isCurrentUser
                            ? 'You cannot delete your own account'
                            : `Delete ${user.userName}`
                        }
                      >
                        <Box component="span">
                          <IconButton
                            aria-label={`Delete ${user.userName}`}
                            color="error"
                            disabled={isCurrentUser}
                            onClick={() => onDelete(user)}
                            size="small"
                          >
                            <DeleteRoundedIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  )
}
