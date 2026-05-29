import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'

import { HomeHeader } from '@/components/home/HomeHeader'
import { HomeMenuDrawer } from '@/components/home/HomeMenuDrawer'
import { HomeWorkspaceContent } from '@/components/home/HomeWorkspaceContent'
import { ReportDeleteDialog } from '@/components/home/ReportDeleteDialog'
import { ChatDrawer } from '@/components/workspace/ChatDrawer'
import { useHomePageController } from '@/hooks/use-home-page-controller'

const HomePage = () => {
  const { chat, deleteDialog, header, menu, workspace } =
    useHomePageController()

  return (
    <Box
      sx={{
        bgcolor: 'background.default',
        color: 'text.primary',
        display: 'flex',
        height: '100vh',
        minWidth: 0,
        overflow: 'hidden',
      }}
    >
      <HomeMenuDrawer {...menu} />

      <Box
        component="main"
        sx={{
          display: 'flex',
          flexDirection: 'column',
          flexGrow: 1,
          height: '100vh',
          minHeight: 0,
          minWidth: 0,
          overflow: 'hidden',
        }}
      >
        <Stack
          spacing={3}
          sx={{
            flex: '0 0 auto',
            minWidth: 0,
            px: { xs: 2, md: 4 },
            pt: { xs: 2, md: 3 },
          }}
        >
          <HomeHeader {...header} />
        </Stack>

        <Box
          sx={{
            flex: '1 1 auto',
            minHeight: 0,
            minWidth: 0,
            overflowX: 'hidden',
            overflowY: 'auto',
            scrollbarGutter: 'stable',
          }}
        >
          <Box
            sx={{
              minHeight: '100%',
              px: { xs: 2, md: 4 },
              py: { xs: 2, md: 3 },
            }}
          >
            <HomeWorkspaceContent {...workspace} />
          </Box>
        </Box>
      </Box>

      <ChatDrawer {...chat} />
      <ReportDeleteDialog {...deleteDialog} />
    </Box>
  )
}

export default HomePage
