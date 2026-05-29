import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'

type ReportTitleSummaryProps = {
  description: string
  title: string
}

export function ReportTitleSummary({
  description,
  title,
}: ReportTitleSummaryProps) {
  return (
    <Box sx={{ minWidth: 0 }}>
      <Typography
        component="h1"
        noWrap
        sx={{
          fontWeight: 700,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
        }}
        variant="h5"
      >
        {title}
      </Typography>
      <Typography color="text.secondary" noWrap variant="caption">
        {description}
      </Typography>
    </Box>
  )
}
