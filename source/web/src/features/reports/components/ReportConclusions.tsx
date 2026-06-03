import LightbulbRoundedIcon from '@mui/icons-material/LightbulbRounded'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

type ReportConclusionsProps = {
  conclusions: string
}

/**
 * Optional, AI-written conclusions for a report turn: deeper analysis, trend interpretation, or
 * recommendations that go beyond the always-present insights. Render this only when the model
 * actually produced conclusions; an absent value should render nothing at all.
 */
export function ReportConclusions({ conclusions }: ReportConclusionsProps) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 1 }}>
        <LightbulbRoundedIcon color="warning" fontSize="small" />
        <Typography sx={{ fontWeight: 600 }} variant="subtitle1">
          Conclusions
        </Typography>
      </Stack>
      <Typography
        color="text.secondary"
        sx={{ whiteSpace: 'pre-line' }}
        variant="body2"
      >
        {conclusions}
      </Typography>
    </Paper>
  )
}
