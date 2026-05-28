import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { ComponentPropsWithoutRef, ReactNode } from 'react'

import { ThemeModeSwitch } from '@/components/theme/ThemeModeSwitch'

type AuthCardProps = {
  title: string
  subtitle: string
  onSubmit: ComponentPropsWithoutRef<'form'>['onSubmit']
  children: ReactNode
}

export function AuthCard({
  title,
  subtitle,
  onSubmit,
  children,
}: AuthCardProps) {
  return (
    <Container
      maxWidth="sm"
      sx={{
        minWidth: '100vw',
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <Box sx={{ position: 'absolute', top: 16, right: 16 }}>
        <ThemeModeSwitch />
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 1,
        }}
      >
        <Typography variant="h4" component="h1" gutterBottom>
          AdventureWorksAIWorkspace
        </Typography>
        <Card
          sx={{
            width: {
              xs: 'calc(100vw - 32px)',
              sm: 480,
              md: 480,
            },
            maxWidth: '100%',
          }}
        >
          <CardContent>
            <Stack component="form" spacing={2} onSubmit={onSubmit}>
              <Box>
                <Typography variant="h6" component="h2" gutterBottom>
                  {title}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {subtitle}
                </Typography>
              </Box>
              {children}
            </Stack>
          </CardContent>
        </Card>
      </Box>
    </Container>
  )
}
