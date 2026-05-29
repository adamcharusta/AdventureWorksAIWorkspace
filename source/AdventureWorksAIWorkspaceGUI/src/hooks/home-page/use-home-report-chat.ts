import { useMutation, useQueryClient } from '@tanstack/react-query'

import { getApiErrorMessage } from '@/lib/api-error'
import {
  addReportMessage,
  createReport,
  reportQueryKeys,
} from '@/lib/report-api'
import { toast } from '@/lib/toast'

type ReportChatApiResponse = Awaited<ReturnType<typeof createReport>>

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

  const handleReportChatSuccess = async (response: ReportChatApiResponse) => {
    const report = response.data.report

    onReportReady(report.id)
    onSuccess()

    queryClient.setQueryData(reportQueryKeys.details(report.id), {
      data: report,
      status: response.status,
      headers: response.headers,
    })

    await queryClient.invalidateQueries({ queryKey: reportQueryKeys.list() })
  }

  const createReportMutation = useMutation({
    mutationFn: (message: string) => createReport({ message }),
    onSuccess: handleReportChatSuccess,
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
    onSuccess: handleReportChatSuccess,
    onError: (error) => {
      toast.error(
        getApiErrorMessage(error, 'The report could not be refined.'),
        'AI report',
      )
    },
  })

  const submitMessage = (message: string) => {
    if (activeReportId) {
      addMessageMutation.mutate({ reportId: activeReportId, message })
      return
    }

    createReportMutation.mutate(message)
  }

  return {
    isSubmitting:
      createReportMutation.isPending || addMessageMutation.isPending,
    submitMessage,
  }
}
