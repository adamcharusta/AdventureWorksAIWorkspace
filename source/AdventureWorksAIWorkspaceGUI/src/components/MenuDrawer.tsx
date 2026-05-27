import AddRoundedIcon from '@mui/icons-material/AddRounded'
import BarChartRoundedIcon from '@mui/icons-material/BarChartRounded'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import FolderRoundedIcon from '@mui/icons-material/FolderRounded'
import HistoryRoundedIcon from '@mui/icons-material/HistoryRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import StarRoundedIcon from '@mui/icons-material/StarRounded'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import List from '@mui/material/List'

import {
  WorkspaceDrawer,
  WorkspaceDrawerAction,
  type WorkspaceDrawerActionItem,
} from './WorkspaceDrawer'

type MenuDrawerProps = {
  open: boolean
  onToggle: () => void
  onLogout: () => void
}

const reportItems: WorkspaceDrawerActionItem[] = [
  { label: 'Workspace', icon: <DashboardRoundedIcon />, selected: true },
  { label: 'Recent reports', icon: <HistoryRoundedIcon /> },
  { label: 'Saved reports', icon: <FolderRoundedIcon /> },
  { label: 'Favorite reports', icon: <StarRoundedIcon /> },
  { label: 'Search reports', icon: <SearchRoundedIcon /> },
]

export function MenuDrawer({ open, onToggle, onLogout }: MenuDrawerProps) {
  return (
    <WorkspaceDrawer
      collapseLabel="Collapse menu"
      expandLabel="Expand menu"
      open={open}
      onToggle={onToggle}
      title="Adventure Works"
      subtitle="AI reporting workspace"
    >
      <Box sx={{ py: 1.5 }}>
        <WorkspaceDrawerAction
          open={open}
          item={{ label: 'New report', icon: <AddRoundedIcon /> }}
        />
      </Box>

      <Divider />

      <List sx={{ display: 'grid', gap: 0.5, py: 1.5 }}>
        {reportItems.map((item) => (
          <WorkspaceDrawerAction key={item.label} item={item} open={open} />
        ))}
      </List>

      <Box sx={{ flexGrow: 1 }} />

      <List sx={{ display: 'grid', gap: 0.5, py: 1 }}>
        <WorkspaceDrawerAction
          open={open}
          item={{ label: 'Analytics', icon: <BarChartRoundedIcon /> }}
        />
        <WorkspaceDrawerAction
          open={open}
          item={{
            label: 'Log out',
            icon: <LogoutRoundedIcon />,
            onClick: onLogout,
          }}
        />
      </List>
    </WorkspaceDrawer>
  )
}
