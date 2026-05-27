import AutoAwesomeRoundedIcon from '@mui/icons-material/AutoAwesomeRounded'
import ChatRoundedIcon from '@mui/icons-material/ChatRounded'
import InsightsRoundedIcon from '@mui/icons-material/InsightsRounded'
import TuneRoundedIcon from '@mui/icons-material/TuneRounded'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import List from '@mui/material/List'

import {
  WorkspaceDrawer,
  WorkspaceDrawerAction,
  type WorkspaceDrawerActionItem,
} from './WorkspaceDrawer'

type ChatDrawerProps = {
  open: boolean
  onToggle: () => void
}

const chatItems: WorkspaceDrawerActionItem[] = [
  { label: 'AI chat', icon: <ChatRoundedIcon />, selected: true },
  { label: 'Follow-up questions', icon: <AutoAwesomeRoundedIcon /> },
  { label: 'Business insights', icon: <InsightsRoundedIcon /> },
  { label: 'Report refinement', icon: <TuneRoundedIcon /> },
]

export function ChatDrawer({ open, onToggle }: ChatDrawerProps) {
  return (
    <WorkspaceDrawer
      anchor="right"
      collapsedIcon={<ChatRoundedIcon />}
      collapseLabel="Collapse chat drawer"
      expandLabel="Expand chat drawer"
      open={open}
      onToggle={onToggle}
      title="Chat drawer"
      subtitle="Report refinement"
    >
      <Box sx={{ py: 1.5 }}>
        <WorkspaceDrawerAction
          anchor="right"
          open={open}
          item={{ label: 'Ask AI', icon: <AutoAwesomeRoundedIcon /> }}
        />
      </Box>

      <Divider />

      <List sx={{ display: 'grid', gap: 0.5, py: 1.5 }}>
        {chatItems.map((item) => (
          <WorkspaceDrawerAction
            anchor="right"
            key={item.label}
            item={item}
            open={open}
          />
        ))}
      </List>

      <Box sx={{ flexGrow: 1 }} />
    </WorkspaceDrawer>
  )
}
