import InsightsRoundedIcon from '@mui/icons-material/InsightsRounded'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

type ReportInsightsProps = {
  insights: string
}

export function ReportInsights({ insights }: ReportInsightsProps) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 1 }}>
        <InsightsRoundedIcon color="primary" fontSize="small" />
        <Typography sx={{ fontWeight: 600 }} variant="subtitle1">
          Insights
        </Typography>
      </Stack>
      <Typography
        color="text.secondary"
        sx={{ whiteSpace: 'pre-line' }}
        variant="body2"
      >
        {insights}
      </Typography>
    </Paper>
  )
}
