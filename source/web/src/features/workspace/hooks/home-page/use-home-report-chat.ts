import { useMutation, useQueryClient } from '@tanstack/react-query'

import type { ReportChatResponse } from '@/api/generated/model'
import {
  addReportMessage,
  createReport,
  getGetReportDetailsQueryKey,
  getGetReportsQueryKey,
} from '@/api/generated/reports/reports'
import { getApiErrorMessage } from '@/shared/lib/api-error'
import { toast } from '@/shared/lib/toast'

// The generated fetch functions return a discriminated union of success/error responses. The custom
// fetch mutator throws on non-2xx, so a resolved value is always the 200 variant; this shape captures
// what the success handler needs from either mutation.
type ReportChatSuccess = {
  data: ReportChatResponse
  status: number
  headers: Headers
}

type UseHomeReportChatOptions = {
  activeReportId: string | null
  onReportReady: (reportId: string) => void
  onSuccess: () => void
}

export function useHomeReportChat({
  activeReportId,
  onReportReady,
  onSuccess,
}: UseHomeReportChatOptions) {
  const queryClient = useQueryClient()

  const handleReportChatSuccess = async (response: ReportChatSuccess) => {
    const report = response.data.report

    onReportReady(report.id)
    onSuccess()

    queryClient.setQueryData(getGetReportDetailsQueryKey(report.id), {
      data: report,
      status: response.status,
      headers: response.headers,
    })

    await queryClient.invalidateQueries({ queryKey: getGetReportsQueryKey() })
  }

  const createReportMutation = useMutation({
    mutationFn: (message: string) => createReport({ message }),
    onSuccess: (response) => {
      if (response.status === 200) {
        void handleReportChatSuccess(response)
      }
    },
    onError: (error) => {
      toast.error(
        getApiErrorMessage(error, 'The report could not be generated.'),
        'AI report',
      )
    },
  })

  const addMessageMutation = useMutation({
    mutationFn: ({
      reportId,
      message,
    }: {
      reportId: string
      message: string
    }) => addReportMessage(reportId, { message }),
    onSuccess: (response) => {
      if (response.status === 200) {
        void handleReportChatSuccess(response)
      }
    },
    onError: (error) => {
      toast.error(
        getApiErrorMessage(error, 'The report could not be refined.'),
        'AI report',
      )
    },
  })

  const submitMessage = async (
    message: string,
  ): Promise<ReportChatResponse> => {
    const response = activeReportId
      ? await addMessageMutation.mutateAsync({
          reportId: activeReportId,
          message,
        })
      : await createReportMutation.mutateAsync(message)

    if (response.status !== 200) {
      throw new Error('The report request did not complete successfully.')
    }

    return response.data
  }

  return {
    isSubmitting:
      createReportMutation.isPending || addMessageMutation.isPending,
    submitMessage,
  }
}
