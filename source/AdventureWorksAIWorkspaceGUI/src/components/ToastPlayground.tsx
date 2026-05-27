import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

import { toast } from '../lib/toast'

export function ToastPlayground() {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Toast playground
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Each button fires a sonner toast that renders an MUI Alert through the
          shared `toast` wrapper.
        </Typography>
        <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
          <Button
            variant="contained"
            color="success"
            onClick={() =>
              toast.success('Report saved to your workspace.', 'Saved')
            }
          >
            Success
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={() =>
              toast.error(
                'SQL validation rejected destructive commands.',
                'Blocked',
              )
            }
          >
            Error
          </Button>
          <Button
            variant="contained"
            color="warning"
            onClick={() =>
              toast.warning('Query returned more than 1000 rows.', 'Heads up')
            }
          >
            Warning
          </Button>
          <Button
            variant="contained"
            color="info"
            onClick={() => toast.info('AI is generating SQL…', 'Working on it')}
          >
            Info
          </Button>
        </Stack>
      </CardContent>
    </Card>
  )
}
