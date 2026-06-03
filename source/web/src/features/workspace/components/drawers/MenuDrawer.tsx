import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import List from '@mui/material/List'
import type { ReactNode } from 'react'

import {
  WorkspaceDrawer,
  WorkspaceDrawerAction,
  type WorkspaceDrawerActionItem,
  type WorkspaceDrawerAnchor,
} from './WorkspaceDrawer'

export type MenuDrawerItem = WorkspaceDrawerActionItem

type MenuDrawerProps = {
  anchor?: WorkspaceDrawerAnchor
  bottomItems?: MenuDrawerItem[]
  collapsedIcon?: ReactNode
  collapseLabel?: string
  expandLabel?: string
  items: MenuDrawerItem[]
  open: boolean
  onToggle: () => void
  subtitle: string
  title: string
  topItems?: MenuDrawerItem[]
}

export function MenuDrawer({
  anchor = 'left',
  bottomItems = [],
  collapsedIcon,
  collapseLabel = 'Collapse menu',
  expandLabel = 'Expand menu',
  items,
  open,
  onToggle,
  subtitle,
  title,
  topItems = [],
}: MenuDrawerProps) {
  return (
    <WorkspaceDrawer
      anchor={anchor}
      collapsedIcon={collapsedIcon}
      collapseLabel={collapseLabel}
      expandLabel={expandLabel}
      open={open}
      onToggle={onToggle}
      title={title}
      subtitle={subtitle}
    >
      <Divider />

      {topItems.length > 0 && (
        <>
          <List sx={{ display: 'grid', gap: 0.5, pb: 1, pt: 0.5 }}>
            {topItems.map((item) => (
              <WorkspaceDrawerAction
                anchor={anchor}
                key={item.label}
                item={item}
                open={open}
              />
            ))}
          </List>
          <Divider />
        </>
      )}

      <List
        sx={{
          alignContent: 'start',
          display: 'grid',
          flex: '1 1 auto',
          gap: 0.5,
          gridAutoRows: 'max-content',
          minHeight: 0,
          overflowY: 'auto',
          py: 1.5,
        }}
      >
        {items.map((item) => (
          <WorkspaceDrawerAction
            anchor={anchor}
            key={item.label}
            item={item}
            open={open}
          />
        ))}
      </List>

      {bottomItems.length > 0 && (
        <>
          <Box sx={{ flex: '0 0 auto' }} />
          <Divider />
          <List sx={{ display: 'grid', gap: 0.5, py: 1 }}>
            {bottomItems.map((item) => (
              <WorkspaceDrawerAction
                anchor={anchor}
                key={item.label}
                item={item}
                open={open}
              />
            ))}
          </List>
        </>
      )}
    </WorkspaceDrawer>
  )
}
