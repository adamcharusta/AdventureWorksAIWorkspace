import { customFetch } from '@/api/customFetch'
import type { ChartSpec, TabularResult } from '@/lib/report-types'

export type ReportStatus =
  | 'Draft'
  | 'Generating'
  | 'Ready'
  | 'Failed'
  | 'Archived'
  | number

export type ReportMessageRole = 'User' | 'Assistant' | 'System' | number

export type SqlValidationStatus = 'NotValidated' | 'Valid' | 'Rejected' | number

export type SqlExecutionStatus = 'NotExecuted' | 'Executed' | 'Failed' | number

type ApiResponse<TData, TStatus extends number = 200> = {
  data: TData
  status: TStatus
  headers: Headers
}

export type ReportSummaryDto = {
  id: string
  title: string
  status: ReportStatus
  isFavorite: boolean
  createdAt: string
  updatedAt: string
}

export type ReportMessageDto = {
  id: string
  role: ReportMessageRole
  content: string
  sortOrder: number
  relatedSqlQueryId?: string | null
  createdAt: string
}

export type GeneratedSqlQueryDto = {
  id: string
  sourceMessageId?: string | null
  userPrompt: string
  sqlText: string
  explanation?: string | null
  validationStatus: SqlValidationStatus
  validationMessage?: string | null
  executionStatus: SqlExecutionStatus
  executionMessage?: string | null
  inputTokens?: number | null
  outputTokens?: number | null
  resultRowCount?: number | null
  resultColumnCount?: number | null
  durationMs?: number | null
  createdAt: string
}

export type ReportSectionDto = {
  id: string
  sourceMessageId?: string | null
  question: string
  title: string
  insights: string
  conclusions?: string | null
  result?: TabularResult | null
  charts: ChartSpec[]
  createdAt: string
}

export type ReportDetailsDto = ReportSummaryDto & {
  originalPrompt: string
  summary?: string | null
  conclusions?: string | null
  result?: TabularResult | null
  charts: ChartSpec[]
  sections?: ReportSectionDto[]
  messages: ReportMessageDto[]
  generatedSqlQueries: GeneratedSqlQueryDto[]
}

export type GetReportsResponse = {
  reports: ReportSummaryDto[]
}

export type RenameReportRequest = {
  title: string
}

export type CreateReportRequest = {
  message: string
}

export type AddReportMessageRequest = {
  message: string
}

export type ReportOutcome =
  | 'Executed'
  | 'Rejected'
  | 'ExecutionFailed'
  | 'GenerationFailed'
  | number

export type ReportChatResponse = {
  report: ReportDetailsDto
  userMessage: ReportMessageDto
  assistantMessage: ReportMessageDto
  sqlQuery?: GeneratedSqlQueryDto | null
  outcome: ReportOutcome
  message?: string | null
  result?: TabularResult | null
  charts: ChartSpec[]
}

export const reportQueryKeys = {
  all: ['reports'] as const,
  list: () => [...reportQueryKeys.all, 'list'] as const,
  details: (reportId: string) =>
    [...reportQueryKeys.all, 'details', reportId] as const,
}

export function getReports(options?: RequestInit) {
  return customFetch<ApiResponse<GetReportsResponse>>('/api/reports', {
    ...options,
    method: 'GET',
  })
}

export function createReport(
  request: CreateReportRequest,
  options?: RequestInit,
) {
  return customFetch<ApiResponse<ReportChatResponse>>('/api/reports', {
    ...options,
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    body: JSON.stringify(request),
  })
}

export function addReportMessage(
  reportId: string,
  request: AddReportMessageRequest,
  options?: RequestInit,
) {
  return customFetch<ApiResponse<ReportChatResponse>>(
    `/api/reports/${reportId}/messages`,
    {
      ...options,
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...options?.headers },
      body: JSON.stringify(request),
    },
  )
}

export function deleteReport(reportId: string, options?: RequestInit) {
  return customFetch<ApiResponse<null, 204>>(`/api/reports/${reportId}`, {
    ...options,
    method: 'DELETE',
  })
}

export function getReportDetails(reportId: string, options?: RequestInit) {
  return customFetch<ApiResponse<ReportDetailsDto>>(
    `/api/reports/${reportId}`,
    {
      ...options,
      method: 'GET',
    },
  )
}

export function renameReportTitle(
  reportId: string,
  request: RenameReportRequest,
  options?: RequestInit,
) {
  return customFetch<ApiResponse<ReportSummaryDto>>(
    `/api/reports/${reportId}/title`,
    {
      ...options,
      method: 'PUT',
      headers: { 'Content-Type': 'application/json', ...options?.headers },
      body: JSON.stringify(request),
    },
  )
}
