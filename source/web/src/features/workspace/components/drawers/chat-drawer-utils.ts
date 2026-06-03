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

const messageRoleMap: Record<string, ChatRole> = {
  '0': 'user',
  '1': 'assistant',
  '2': 'system',
  Assistant: 'assistant',
  System: 'system',
  User: 'user',
}

export function getChatMessagesKey(messages: ChatMessage[]): string {
  return messages
    .map((message) => `${message.id}:${message.updatedAt ?? message.createdAt}`)
    .join('|')
}

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
