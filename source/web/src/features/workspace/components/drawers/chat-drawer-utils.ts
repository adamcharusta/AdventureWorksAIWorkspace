import type {
  ChatAdapter,
  ChatMessage,
  ChatMessageChunk,
  ChatRole,
  ChatUser,
} from '@mui/x-chat/headless'

import type {
  ReportChatResponse,
  ReportMessageDto,
  ReportMessageRole,
} from '@/api/generated/model'

/**
 * Fixed chat participants used by MUI X Chat. Backend report messages only store the role, so the
 * frontend supplies stable author objects for rendering avatars, names, and message alignment.
 */
export const chatUsers = {
  assistant: {
    id: 'adventureworks-ai',
    displayName: 'AdventureWorks AI',
    role: 'assistant',
  },
  user: {
    id: 'current-user',
    displayName: 'You',
    role: 'user',
  },
} satisfies Record<string, ChatUser>

type SubmitChatMessage = (
  message: string,
) => Promise<ReportChatResponse | void> | ReportChatResponse | void

// API enums may arrive either as strings (current contract) or numbers (older generated clients and
// tests). Keep both forms so persisted data can still be mapped into MUI's chat role names.
const messageRoleMap: Record<string, ChatRole> = {
  '0': 'user',
  '1': 'assistant',
  '2': 'system',
  Assistant: 'assistant',
  System: 'system',
  User: 'user',
}

/**
 * Builds a remount key for the chat surface from persisted message ids and timestamps.
 *
 * MUI X Chat keeps some internal message state. When the saved conversation changes underneath it,
 * this key lets the caller remount the chat only when the actual persisted message set changed.
 */
export function getChatMessagesKey(messages: ChatMessage[]): string {
  return messages
    .map((message) => `${message.id}:${message.updatedAt ?? message.createdAt}`)
    .join('|')
}

/**
 * Converts backend report messages into MUI X Chat messages.
 *
 * Input requirements:
 * - `sortOrder` must express conversation order.
 * - `role` must be a backend report role (`User`, `Assistant`, `System`) or its numeric enum value.
 * - `content` is treated as a completed text part; streaming is only used for newly submitted turns.
 */
export function mapReportMessagesToChatMessages(
  messages: ReportMessageDto[],
): ChatMessage[] {
  return [...messages]
    .sort((left, right) => left.sortOrder - right.sortOrder)
    .map((message) => {
      const role = mapReportMessageRole(message.role)

      return {
        id: message.id,
        role,
        author: getChatAuthor(role),
        createdAt: message.createdAt,
        status: 'sent',
        parts: [{ type: 'text', text: message.content, state: 'done' }],
      }
    })
}

/**
 * Creates the MUI X Chat adapter that bridges a user message to the report API.
 *
 * The adapter extracts plain text from the outgoing MUI message, delegates persistence/report
 * generation to `onSubmit`, then returns the assistant response as a short ReadableStream because
 * MUI X Chat consumes assistant replies as streamed chunks.
 */
export function createReportChatAdapter(
  onSubmit?: SubmitChatMessage,
): ChatAdapter {
  return {
    async sendMessage({ message, signal }) {
      const text = getChatMessageText(message)

      if (!text || !onSubmit) {
        return createAssistantMessageStream({
          messageId: crypto.randomUUID(),
          signal,
          text: '',
        })
      }

      const response = await onSubmit(text)

      return createAssistantMessageStream({
        messageId: response?.assistantMessage.id ?? crypto.randomUUID(),
        signal,
        text: response?.assistantMessage.content ?? '',
      })
    },
  }
}

function mapReportMessageRole(role: ReportMessageRole): ChatRole {
  return messageRoleMap[String(role)] ?? 'assistant'
}

function getChatAuthor(role: ChatRole): ChatUser {
  if (role === 'user') {
    return chatUsers.user
  }

  return chatUsers.assistant
}

function getChatMessageText(message: ChatMessage): string {
  // The current product only sends text. Joining all text parts keeps the adapter tolerant of MUI
  // splitting one user message into multiple text parts in future versions.
  return message.parts
    .map((part) => (part.type === 'text' ? part.text : ''))
    .join('\n')
    .trim()
}

function createAssistantMessageStream({
  messageId,
  signal,
  text,
}: {
  messageId: string
  signal: AbortSignal
  text: string
}) {
  // MUI X Chat expects assistant output as a sequence of protocol chunks even when the API already
  // returned the whole message. Emit one complete text part and respect aborts from the chat UI.
  const textPartId = `${messageId}-text`
  const chunks: ChatMessageChunk[] = [
    { type: 'start', messageId, author: chatUsers.assistant },
    { type: 'text-start', id: textPartId },
    { type: 'text-delta', id: textPartId, delta: text },
    { type: 'text-end', id: textPartId },
    { type: 'finish', messageId },
  ]

  return new ReadableStream<ChatMessageChunk>({
    start(controller) {
      for (const chunk of chunks) {
        if (signal.aborted) {
          controller.close()
          return
        }

        controller.enqueue(chunk)
      }

      controller.close()
    },
  })
}
