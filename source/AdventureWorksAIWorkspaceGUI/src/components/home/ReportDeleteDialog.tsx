import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogContentText from '@mui/material/DialogContentText'
import DialogTitle from '@mui/material/DialogTitle'

type ReportDeleteDialogProps = {
  isDeleting: boolean
  onClose: () => void
  onConfirm: () => void
  open: boolean
  reportName: string
}

export function ReportDeleteDialog({
  isDeleting,
  onClose,
  onConfirm,
  open,
  reportName,
}: ReportDeleteDialogProps) {
  return (
    <Dialog
      aria-labelledby="delete-report-dialog-title"
      onClose={onClose}
      open={open}
    >
      <DialogTitle id="delete-report-dialog-title">Delete report?</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Are you sure you want to delete "{reportName}"? This permanently
          removes the report along with its conversation and generated SQL
          history, and cannot be undone.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button disabled={isDeleting} onClick={onClose}>
          Cancel
        </Button>
        <Button
          color="error"
          disabled={isDeleting}
          onClick={onConfirm}
          startIcon={
            isDeleting ? (
              <CircularProgress color="inherit" size={16} />
            ) : (
              <DeleteOutlineRoundedIcon />
            )
          }
          variant="contained"
        >
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  )
}
