import AutoAwesomeRoundedIcon from '@mui/icons-material/AutoAwesomeRounded'
import ChatRoundedIcon from '@mui/icons-material/ChatRounded'
import SendRoundedIcon from '@mui/icons-material/SendRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { type FormEvent, useState } from 'react'

import type { ReportMessageDto, ReportMessageRole } from '@/lib/report-api'

import { WorkspaceDrawer } from './WorkspaceDrawer'

type ChatDrawerProps = {
  isSubmitting?: boolean
  messages?: ReportMessageDto[]
  onSubmit?: (message: string) => void
  onToggle: () => void
  open: boolean
  subtitle?: string
}

const messageRoleLabels: Record<string, string> = {
  '0': 'User',
  '1': 'Assistant',
  '2': 'System',
  User: 'User',
  Assistant: 'Assistant',
  System: 'System',
}

function getMessageRoleLabel(role: ReportMessageRole): string {
  return messageRoleLabels[String(role)] ?? String(role)
}

function formatMessageTime(value: string): string {
  return new Intl.DateTimeFormat(undefined, {
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))
}

export function ChatDrawer({
  isSubmitting = false,
  messages = [],
  onSubmit,
  onToggle,
  open,
  subtitle = 'Report refinement',
}: ChatDrawerProps) {
  const [message, setMessage] = useState('')
  const sortedMessages = [...messages].sort(
    (left, right) => left.sortOrder - right.sortOrder,
  )

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmed = message.trim()
    if (!trimmed || isSubmitting) {
      return
    }

    onSubmit?.(trimmed)
    setMessage('')
  }

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
    >
      <Divider sx={{ my: 1 }} />

      <Stack sx={{ flex: '1 1 auto', minHeight: 0 }}>
        <Stack
          spacing={1}
          sx={{
            flex: '1 1 auto',
            minHeight: 0,
            overflowY: 'auto',
            py: 1.5,
          }}
        >
          {sortedMessages.length > 0 ? (
            sortedMessages.map((item) => (
              <Box
                key={item.id}
                sx={(theme) => ({
                  bgcolor:
                    getMessageRoleLabel(item.role) === 'User'
                      ? theme.palette.action.hover
                      : 'transparent',
                  borderColor: 'divider',
                  borderRadius: 1,
                  borderStyle: 'solid',
                  borderWidth: 1,
                  p: 1.25,
                  whiteSpace: 'normal',
                })}
              >
                <Stack
                  direction="row"
                  spacing={1}
                  sx={{ alignItems: 'center', justifyContent: 'space-between' }}
                >
                  <Typography sx={{ fontWeight: 600 }} variant="caption">
                    {getMessageRoleLabel(item.role)}
                  </Typography>
                  <Typography color="text.secondary" variant="caption">
                    {formatMessageTime(item.createdAt)}
                  </Typography>
                </Stack>
                <Typography
                  sx={{ mt: 0.75, whiteSpace: 'pre-wrap' }}
                  variant="body2"
                >
                  {item.content}
                </Typography>
              </Box>
            ))
          ) : (
            <Stack
              spacing={1}
              sx={{
                alignItems: 'center',
                color: 'text.secondary',
                flex: '1 1 auto',
                justifyContent: 'center',
                textAlign: 'center',
              }}
            >
              <AutoAwesomeRoundedIcon color="primary" />
              <Typography variant="body2">No messages yet</Typography>
            </Stack>
          )}
        </Stack>

        <Divider />

        <Box component="form" onSubmit={handleSubmit} sx={{ py: 1.5 }}>
          <TextField
            disabled={isSubmitting}
            fullWidth
            maxRows={5}
            minRows={3}
            multiline
            onChange={(event) => setMessage(event.target.value)}
            placeholder="Ask about AdventureWorks data"
            size="small"
            slotProps={{
              htmlInput: {
                'aria-label': 'Chat message',
              },
            }}
            value={message}
          />
          <Button
            disabled={isSubmitting || !message.trim()}
            fullWidth
            startIcon={
              isSubmitting ? (
                <CircularProgress color="inherit" size={16} />
              ) : (
                <SendRoundedIcon />
              )
            }
            sx={{ mt: 1 }}
            type="submit"
            variant="contained"
          >
            Send
          </Button>
        </Box>
      </Stack>
    </WorkspaceDrawer>
  )
}
