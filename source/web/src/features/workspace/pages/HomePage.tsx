import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'

import { ChatDrawer } from '@/features/workspace/components/drawers/ChatDrawer'
import { workspaceHeaderHeight } from '@/features/workspace/components/drawers/WorkspaceDrawer'
import { HomeHeader } from '@/features/workspace/components/home/HomeHeader'
import { HomeMenuDrawer } from '@/features/workspace/components/home/HomeMenuDrawer'
import { HomeWorkspaceContent } from '@/features/workspace/components/home/HomeWorkspaceContent'
import { ReportDeleteDialog } from '@/features/workspace/components/home/ReportDeleteDialog'
import { useHomePageController } from '@/features/workspace/hooks/use-home-page-controller'

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
        <Box
          component="section"
          sx={{
            alignItems: 'center',
            display: 'flex',
            flex: '0 0 auto',
            height: workspaceHeaderHeight,
            minWidth: 0,
            px: { xs: 2, md: 4 },
          }}
        >
          <HomeHeader {...header} />
        </Box>

        <Divider sx={{ mx: { xs: 2, md: 4 } }} />

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
