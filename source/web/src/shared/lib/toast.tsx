import type { AlertColor } from '@mui/material/Alert'
import Alert from '@mui/material/Alert'
import AlertTitle from '@mui/material/AlertTitle'
import type { ReactNode } from 'react'
import { toast as sonner } from 'sonner'

interface MuiToastOptions {
  title?: ReactNode
  message: ReactNode
  severity: AlertColor
}

function muiAlertToast({ title, message, severity }: MuiToastOptions) {
  return sonner.custom((id) => (
    <Alert
      severity={severity}
      variant="filled"
      onClose={() => sonner.dismiss(id)}
      sx={{ width: '100%', minWidth: 320, boxShadow: 6 }}
    >
      {title ? <AlertTitle>{title}</AlertTitle> : null}
      {message}
    </Alert>
  ))
}

export const toast = {
  success: (message: ReactNode, title?: ReactNode) =>
    muiAlertToast({ message, title, severity: 'success' }),
  error: (message: ReactNode, title?: ReactNode) =>
    muiAlertToast({ message, title, severity: 'error' }),
  info: (message: ReactNode, title?: ReactNode) =>
    muiAlertToast({ message, title, severity: 'info' }),
  warning: (message: ReactNode, title?: ReactNode) =>
    muiAlertToast({ message, title, severity: 'warning' }),
  dismiss: sonner.dismiss,
  custom: sonner.custom,
}
