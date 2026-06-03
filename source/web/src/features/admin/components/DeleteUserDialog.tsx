import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

import type { UserDto } from '@/api/generated/model'

type DeleteUserDialogProps = {
  isDeleting: boolean
  onClose: () => void
  onConfirm: () => void
  user: UserDto | null
}

export function DeleteUserDialog({
  isDeleting,
  onClose,
  onConfirm,
  user,
}: DeleteUserDialogProps) {
  return (
    <Dialog fullWidth maxWidth="xs" onClose={onClose} open={Boolean(user)}>
      <DialogTitle>Delete user</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Typography>
            Delete {user?.userName}? This action cannot be undone.
          </Typography>
          <Alert severity="warning">
            The user will lose access to the application immediately.
          </Alert>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          color="error"
          disabled={isDeleting}
          onClick={onConfirm}
          variant="contained"
        >
          {isDeleting ? 'Deleting...' : 'Delete user'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
