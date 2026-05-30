import ContentCopyRoundedIcon from '@mui/icons-material/ContentCopyRounded'
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded'
import EditRoundedIcon from '@mui/icons-material/EditRounded'
import FingerprintRoundedIcon from '@mui/icons-material/FingerprintRounded'
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import Menu from '@mui/material/Menu'
import MenuItem from '@mui/material/MenuItem'
import Tooltip from '@mui/material/Tooltip'
import { type MouseEvent, useState } from 'react'

import { toast } from '@/lib/toast'

type ReportActionsMenuProps = {
  activeReportId: string | null
  hasActiveReport: boolean
  hasSqlQueries: boolean
  onCopySql: () => Promise<void> | void
  onOpenDeleteDialog: () => void
  onStartRename: () => void
}

export function ReportActionsMenu({
  activeReportId,
  hasActiveReport,
  hasSqlQueries,
  onCopySql,
  onOpenDeleteDialog,
  onStartRename,
}: ReportActionsMenuProps) {
  const [actionsAnchor, setActionsAnchor] = useState<null | HTMLElement>(null)
  const isActionsMenuOpen = Boolean(actionsAnchor)

  const handleOpenActionsMenu = (event: MouseEvent<HTMLElement>) => {
    setActionsAnchor(event.currentTarget)
  }

  const handleCloseActionsMenu = () => {
    setActionsAnchor(null)
  }

  const handleCopyReportId = async () => {
    handleCloseActionsMenu()
    if (!activeReportId) {
      return
    }

    try {
      await navigator.clipboard.writeText(activeReportId)
      toast.success('Report ID copied to clipboard.', 'Reports')
    } catch {
      toast.error('Could not copy the report ID to the clipboard.', 'Reports')
    }
  }

  return (
    <Box sx={{ flexShrink: 0 }}>
      <Tooltip title="Report actions">
        <IconButton
          aria-controls={isActionsMenuOpen ? 'report-actions-menu' : undefined}
          aria-expanded={isActionsMenuOpen ? 'true' : undefined}
          aria-haspopup="true"
          aria-label="Report actions"
          onClick={handleOpenActionsMenu}
        >
          <MoreVertRoundedIcon />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={actionsAnchor}
        id="report-actions-menu"
        onClose={handleCloseActionsMenu}
        open={isActionsMenuOpen}
      >
        <MenuItem
          disabled={!hasActiveReport}
          onClick={() => {
            onStartRename()
            handleCloseActionsMenu()
          }}
        >
          <ListItemIcon>
            <EditRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Rename report</ListItemText>
        </MenuItem>
        <MenuItem
          disabled={!hasSqlQueries}
          onClick={() => {
            void onCopySql()
            handleCloseActionsMenu()
          }}
        >
          <ListItemIcon>
            <ContentCopyRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Copy SQL queries</ListItemText>
        </MenuItem>
        <MenuItem
          disabled={!hasActiveReport}
          onClick={() => {
            void handleCopyReportId()
          }}
        >
          <ListItemIcon>
            <FingerprintRoundedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Copy report ID</ListItemText>
        </MenuItem>
        <MenuItem
          disabled={!hasActiveReport}
          onClick={() => {
            onOpenDeleteDialog()
            handleCloseActionsMenu()
          }}
          sx={{ color: 'error.main' }}
        >
          <ListItemIcon>
            <DeleteOutlineRoundedIcon color="error" fontSize="small" />
          </ListItemIcon>
          <ListItemText>Delete report</ListItemText>
        </MenuItem>
      </Menu>
    </Box>
  )
}
