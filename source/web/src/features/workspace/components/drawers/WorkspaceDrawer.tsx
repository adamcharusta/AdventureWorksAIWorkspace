import ChevronLeftRoundedIcon from '@mui/icons-material/ChevronLeftRounded'
import ChevronRightRoundedIcon from '@mui/icons-material/ChevronRightRounded'
import MenuRoundedIcon from '@mui/icons-material/MenuRounded'
import Box from '@mui/material/Box'
import Drawer from '@mui/material/Drawer'
import IconButton from '@mui/material/IconButton'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import Stack from '@mui/material/Stack'
import { alpha, useTheme } from '@mui/material/styles'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import type { ReactNode } from 'react'

export const drawerWidth = 280
export const miniDrawerWidth = 72
// Shared height of the header band across the drawers and the main workspace, so their
// dividers line up horizontally.
export const workspaceHeaderHeight = 64

export type WorkspaceDrawerAnchor = 'left' | 'right'

export type WorkspaceDrawerActionItem = {
  label: string
  icon: ReactNode
  onClick?: () => void
  selected?: boolean
  disabled?: boolean
}

type WorkspaceDrawerProps = {
  anchor?: WorkspaceDrawerAnchor
  children: ReactNode
  collapsedIcon?: ReactNode
  collapseLabel: string
  expandLabel: string
  headerActions?: ReactNode
  onToggle: () => void
  open: boolean
  subtitle: string
  title: string
  width?: number
}

type WorkspaceDrawerActionProps = {
  anchor?: WorkspaceDrawerAnchor
  item: WorkspaceDrawerActionItem
  open: boolean
}

export function WorkspaceDrawerAction({
  anchor = 'left',
  item,
  open,
}: WorkspaceDrawerActionProps) {
  const tooltipPlacement = anchor === 'right' ? 'left' : 'right'

  const button = (
    <ListItemButton
      aria-label={item.label}
      disabled={item.disabled}
      onClick={item.onClick}
      selected={item.selected}
      sx={(theme) => ({
        borderRadius: 1,
        justifyContent: open ? 'flex-start' : 'center',
        minHeight: 44,
        mx: open ? 0 : 'auto',
        px: open ? 1.5 : 0,
        transition: theme.transitions.create(
          ['background-color', 'padding', 'width'],
          {
            duration: theme.transitions.duration.standard,
            easing: theme.transitions.easing.easeInOut,
          },
        ),
        width: open ? '100%' : 48,
      })}
    >
      <ListItemIcon
        sx={{
          color: 'inherit',
          flex: '0 0 24px',
          justifyContent: 'center',
          minWidth: 0,
          width: 24,
          '& .MuiSvgIcon-root': {
            fontSize: 22,
          },
        }}
      >
        {item.icon}
      </ListItemIcon>
      <ListItemText
        aria-hidden={!open}
        primary={item.label}
        slotProps={{ primary: { noWrap: true, variant: 'body2' } }}
        sx={(theme) => ({
          flex: open ? '1 1 auto' : '0 0 0px',
          maxWidth: open ? 190 : 0,
          minWidth: 0,
          ml: open ? 1.5 : 0,
          opacity: open ? 1 : 0,
          overflow: 'hidden',
          transition: theme.transitions.create(
            ['margin', 'max-width', 'opacity'],
            {
              duration: theme.transitions.duration.standard,
              easing: theme.transitions.easing.easeInOut,
            },
          ),
          whiteSpace: 'nowrap',
        })}
      />
    </ListItemButton>
  )

  return (
    <Tooltip
      disableFocusListener={open}
      disableHoverListener={open}
      disableTouchListener={open}
      title={open ? '' : item.label}
      placement={tooltipPlacement}
    >
      {button}
    </Tooltip>
  )
}

export function WorkspaceDrawer({
  anchor = 'left',
  children,
  collapsedIcon = <MenuRoundedIcon />,
  collapseLabel,
  expandLabel,
  headerActions,
  onToggle,
  open,
  subtitle,
  title,
  width = drawerWidth,
}: WorkspaceDrawerProps) {
  const theme = useTheme()
  const isRight = anchor === 'right'
  const currentWidth = open ? width : miniDrawerWidth

  return (
    <Drawer
      anchor={anchor}
      variant="permanent"
      open={open}
      sx={{
        flexShrink: 0,
        transition: theme.transitions.create('width', {
          duration: theme.transitions.duration.standard,
          easing: theme.transitions.easing.easeInOut,
        }),
        width: currentWidth,
        whiteSpace: 'nowrap',
        '& .MuiDrawer-paper': {
          bgcolor:
            theme.palette.mode === 'dark'
              ? alpha(theme.palette.common.white, 0.04)
              : theme.palette.grey[50],
          borderColor: 'divider',
          boxSizing: 'border-box',
          overflowX: 'hidden',
          overflowY: 'hidden',
          transition: theme.transitions.create('width', {
            duration: theme.transitions.duration.standard,
            easing: theme.transitions.easing.easeInOut,
          }),
          willChange: 'width',
          width: currentWidth,
        },
      }}
    >
      <Stack
        sx={{
          boxSizing: 'border-box',
          height: '100%',
          minHeight: 0,
          overflow: 'hidden',
          pb: 1.5,
          px: 1.25,
        }}
      >
        <Stack
          direction="row"
          sx={{
            alignItems: 'center',
            flexShrink: 0,
            height: workspaceHeaderHeight,
            justifyContent: 'center',
          }}
        >
          <Box
            sx={{
              alignItems: 'center',
              display: 'flex',
              flexDirection: isRight ? 'row-reverse' : 'row',
              justifyContent: 'space-between',
              minWidth: 40,
              transition: theme.transitions.create('width', {
                duration: theme.transitions.duration.standard,
                easing: theme.transitions.easing.easeInOut,
              }),
              width: open ? '100%' : 40,
            }}
          >
            <Box
              sx={{
                flex: open ? '1 1 auto' : '0 0 0px',
                maxWidth: open ? 192 : 0,
                minWidth: 0,
                opacity: open ? 1 : 0,
                overflow: 'hidden',
                pointerEvents: open ? 'auto' : 'none',
                textAlign: isRight ? 'right' : 'left',
                transition: theme.transitions.create(['max-width', 'opacity'], {
                  duration: theme.transitions.duration.standard,
                  easing: theme.transitions.easing.easeInOut,
                }),
              }}
            >
              <Typography noWrap sx={{ fontWeight: 700 }} variant="subtitle1">
                {title}
              </Typography>
              <Typography noWrap variant="caption" color="text.secondary">
                {subtitle}
              </Typography>
            </Box>

            {open && headerActions ? (
              <Box
                sx={{
                  alignItems: 'center',
                  display: 'flex',
                  flex: '0 0 auto',
                  gap: 0.5,
                }}
              >
                {headerActions}
              </Box>
            ) : null}

            <Tooltip title={open ? collapseLabel : expandLabel}>
              <IconButton
                aria-label={open ? collapseLabel : expandLabel}
                onClick={onToggle}
                size="small"
                sx={{
                  flex: '0 0 auto',
                  height: 40,
                  width: 40,
                }}
              >
                {open ? (
                  isRight ? (
                    <ChevronRightRoundedIcon />
                  ) : (
                    <ChevronLeftRoundedIcon />
                  )
                ) : (
                  collapsedIcon
                )}
              </IconButton>
            </Tooltip>
          </Box>
        </Stack>

        {children}
      </Stack>
    </Drawer>
  )
}
