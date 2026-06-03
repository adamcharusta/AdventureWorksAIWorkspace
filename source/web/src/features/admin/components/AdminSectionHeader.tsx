import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'

type AdminSectionHeaderProps = {
  description: string
  title: string
}

export function AdminSectionHeader({
  description,
  title,
}: AdminSectionHeaderProps) {
  return (
    <Box>
      <Typography sx={{ fontWeight: 700 }} variant="h6">
        {title}
      </Typography>
      <Typography color="text.secondary" variant="body2">
        {description}
      </Typography>
    </Box>
  )
}
