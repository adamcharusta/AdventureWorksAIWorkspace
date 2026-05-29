import ChatRoundedIcon from '@mui/icons-material/ChatRounded'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import { ChatBox } from '@mui/x-chat'
import type { ChatMessage } from '@mui/x-chat/headless'
import { useMemo, useState } from 'react'

import type { ReportChatResponse, ReportMessageDto } from '@/lib/report-api'

import {
  chatUsers,
  createReportChatAdapter,
  mapReportMessagesToChatMessages,
} from './chat-drawer-utils'
import { WorkspaceDrawer } from './WorkspaceDrawer'

const chatDrawerWidth = 440

type ChatDrawerProps = {
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

type ReportChatBoxProps = {
  initialMessages: ChatMessage[]
  isSubmitting: boolean
  onSubmit?: ChatDrawerProps['onSubmit']
}

function ReportChatBox({
  initialMessages,
  isSubmitting,
  onSubmit,
}: ReportChatBoxProps) {
  const [chatMessages, setChatMessages] =
    useState<ChatMessage[]>(initialMessages)
  const adapter = useMemo(() => createReportChatAdapter(onSubmit), [onSubmit])

  return (
    <ChatBox
      adapter={adapter}
      currentUser={chatUsers.user}
      density="compact"
      features={{
        attachments: false,
        conversationHeader: false,
        helperText: false,
        suggestions: false,
      }}
      localeText={{
        composerInputAriaLabel: 'Chat message',
        composerInputPlaceholder: 'Ask about AdventureWorks data',
        composerSendButtonLabel: 'Send',
        threadNoMessagesHelperText:
          'Ask a question to create or refine a report.',
        threadNoMessagesLabel: 'No messages yet',
      }}
      members={[chatUsers.user, chatUsers.assistant]}
      messages={chatMessages}
      onMessagesChange={setChatMessages}
      slots={{
        messageAvatar: HiddenChatSlot,
        messageMeta: HiddenChatSlot,
      }}
      slotProps={{
        composerInput: {
          maxRows: 5,
          rows: 1,
          sx: {
            bgcolor: 'action.hover',
            borderRadius: '18px',
            borderWidth: 0,
            lineHeight: 1.45,
            maxHeight: 112,
            minHeight: 40,
            my: 0,
            overflowY: 'auto',
            px: 1.5,
            py: 1,
            '&:placeholder-shown': {
              height: '40px !important',
            },
          },
        },
        composerRoot: {
          disabled: isSubmitting,
          variant: 'compact',
          sx: {
            bgcolor: 'background.paper',
            border: 0,
            borderColor: 'transparent',
            borderRadius: 0,
            boxShadow: 'none',
            flexWrap: 'nowrap',
            gap: 0.75,
            m: 0,
            px: 1,
            py: 0.75,
            width: '100%',
            '&:focus-within:not([data-disabled])': {
              borderColor: 'transparent',
              boxShadow: 'none',
            },
          },
        },
        composerSendButton: {
          sx: {
            alignSelf: 'center',
            borderRadius: '50%',
            height: 40,
            m: 0,
            width: 40,
            '& svg': {
              fontSize: '1.35rem',
            },
          },
        },
        messageContent: {
          slotProps: {
            bubble: {
              style: {
                overflowWrap: 'anywhere',
              },
            },
          },
        },
        messageGroup: {
          slots: {
            authorName: HiddenChatSlot,
            groupTimestamp: HiddenChatSlot,
          },
          sx: {
            '--MuiChatMessage-avatarSize': '0px',
            marginBlockStart: 0,
          },
        },
        messageList: {
          slotProps: {
            messageListContent: {
              style: {
                width: '100%',
              },
            },
            messageListScroller: {
              style: {
                paddingRight: 0,
              },
            },
          },
          sx: {
            py: 0,
            width: '100%',
          },
        },
        messageRoot: {
          sx: {
            columnGap: 0,
            gridTemplateAreas: {
              xs: '"content"',
            },
            gridTemplateColumns: '1fr',
            maxWidth: '100%',
            px: 1,
            py: 0.125,
            width: '100%',
          },
        },
      }}
      streamFlushInterval={0}
      sx={{
        '& .MuiChatMessage-avatar': {
          display: 'none',
        },
        '& .MuiChatComposer-root': {
          border: 0,
          margin: 0,
        },
        '& .MuiChatComposer-root.MuiChatComposer-variantCompact': {
          border: 0,
          margin: 0,
        },
        '& .MuiChatMessage-content': {
          maxWidth: '100%',
          width: '100%',
        },
        '& .MuiChatMessage-bubble': {
          borderRadius: '18px',
          display: 'block',
          lineHeight: 1.45,
          maxWidth: '86%',
          px: 1.35,
          py: 0.875,
          width: 'fit-content',
        },
        '& .MuiChatMessage-groupAuthorName, & .MuiChatMessage-inlineMeta, & .MuiChatMessage-inlineMetaSpacer, & .MuiChatMessage-meta':
          {
            display: 'none',
          },
        '& .MuiChatMessage-roleAssistant .MuiChatMessage-avatar, & .MuiChatMessage-roleUser .MuiChatMessage-avatar':
          {
            display: 'none',
          },
        '& .MuiChatMessage-roleAssistant': {
          alignSelf: 'stretch',
          gridTemplateAreas: {
            xs: '"content"',
          },
          gridTemplateColumns: '1fr',
        },
        '& .MuiChatMessage-roleAssistant .MuiChatMessage-content': {
          alignItems: 'flex-start',
          justifySelf: 'stretch',
        },
        '& .MuiChatMessage-roleAssistant .MuiChatMessage-bubble': {
          bgcolor: 'action.hover',
          borderBottomLeftRadius: '6px',
          color: 'text.primary',
        },
        '& .MuiChatMessage-roleUser': {
          alignSelf: 'stretch',
          gridTemplateAreas: {
            xs: '"content"',
          },
          gridTemplateColumns: '1fr',
        },
        '& .MuiChatMessage-roleUser .MuiChatMessage-content': {
          alignItems: 'flex-end',
          justifySelf: 'stretch',
        },
        '& .MuiChatMessage-roleUser .MuiChatMessage-bubble': {
          bgcolor: 'primary.main',
          borderBottomRightRadius: '6px',
          color: 'primary.contrastText',
        },
        '& [data-message-list-row]': {
          width: '100%',
        },
        '& [data-message-list-row] + [data-message-list-row]': {
          marginTop: 1.5,
        },
        bgcolor: 'transparent',
        flex: '1 1 auto',
        mb: -1.5,
        mx: -1.25,
        minHeight: 0,
        width: 'calc(100% + 20px)',
      }}
      variant="default"
    />
  )
}

function HiddenChatSlot() {
  return null
}

function getChatMessagesKey(messages: ChatMessage[]) {
  return messages
    .map((message) => `${message.id}:${message.updatedAt ?? message.createdAt}`)
    .join('|')
}
