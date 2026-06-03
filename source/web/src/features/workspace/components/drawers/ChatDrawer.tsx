import ChatRoundedIcon from '@mui/icons-material/ChatRounded'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import { useMemo } from 'react'

import type {
  ReportChatResponse,
  ReportMessageDto,
} from '@/api/generated/model'

import {
  getChatMessagesKey,
  mapReportMessagesToChatMessages,
} from './chat-drawer-utils'
import { ReportChatBox } from './ReportChatBox'
import { WorkspaceDrawer } from './WorkspaceDrawer'

const chatDrawerWidth = 440

export type ChatDrawerProps = {
  isSubmitting?: boolean
  messages?: ReportMessageDto[]
  onSubmit?: (
    message: string,
  ) => Promise<ReportChatResponse | void> | ReportChatResponse | void
  onToggle: () => void
  open: boolean
  subtitle?: string
}

export function ChatDrawer({
  isSubmitting = false,
  messages = [],
  onSubmit,
  onToggle,
  open,
  subtitle = 'Report refinement',
}: ChatDrawerProps) {
  const persistedMessages = useMemo(
    () => mapReportMessagesToChatMessages(messages),
    [messages],
  )

  return (
    <WorkspaceDrawer
      anchor="right"
      collapsedIcon={<ChatRoundedIcon />}
      collapseLabel="Collapse chat drawer"
      expandLabel="Expand chat drawer"
      open={open}
      onToggle={onToggle}
      title="AI chat"
      subtitle={subtitle}
      width={chatDrawerWidth}
    >
      {open ? (
        <>
          <Divider sx={{ mb: 0, mt: 0, mx: -1.25 }} />

          <ReportChatBox
            key={getChatMessagesKey(persistedMessages)}
            initialMessages={persistedMessages}
            isSubmitting={isSubmitting}
            onSubmit={onSubmit}
          />
        </>
      ) : (
        <Box
          aria-hidden="true"
          sx={{
            bgcolor: 'action.hover',
            borderColor: 'divider',
            borderTopStyle: 'solid',
            borderTopWidth: 1,
            flex: '1 1 auto',
            mb: -1.5,
            mt: 0,
            mx: -1.25,
            opacity: 0.55,
          }}
        />
      )}
    </WorkspaceDrawer>
  )
}
