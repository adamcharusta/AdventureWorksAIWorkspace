import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { Link as RouterLink } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      <Stack spacing={1}>
        <Typography variant="h4" component="h1">
          Page not found
        </Typography>
        <Typography variant="body1" color="text.secondary">
          The route does not exist. Go back to the home page.
        </Typography>
        <Typography component={RouterLink} to="/" color="primary">
          Back to Weather forecasts
        </Typography>
      </Stack>
    </Container>
  )
}
