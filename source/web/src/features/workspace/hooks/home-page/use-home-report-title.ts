import { useMutation, useQueryClient } from '@tanstack/react-query'
import { type FormEvent, useState } from 'react'

import type { ReportDetailsDto, ReportSummaryDto } from '@/api/generated/model'
import {
  getGetReportDetailsQueryKey,
  getGetReportsQueryKey,
  renameReport,
} from '@/api/generated/reports/reports'
import { getApiErrorMessage } from '@/shared/lib/api-error'
import { toast } from '@/shared/lib/toast'

type UseHomeReportTitleOptions = {
  activeReportId: string | null
  selectedReport: ReportDetailsDto | null
  selectedReportSummary: ReportSummaryDto | null
}

export function useHomeReportTitle({
  activeReportId,
  selectedReport,
  selectedReportSummary,
}: UseHomeReportTitleOptions) {
  const queryClient = useQueryClient()
  const [isEditingTitle, setIsEditingTitle] = useState(false)
  const [titleDraft, setTitleDraft] = useState('')

  const renameReportMutation = useMutation({
    mutationFn: ({ reportId, title }: { reportId: string; title: string }) =>
      renameReport(reportId, { title }),
    onSuccess: async (response, variables) => {
      if (response.status !== 200) {
        return
      }

      setIsEditingTitle(false)
      setTitleDraft(response.data.title)
      toast.success('Report title has been updated.', 'Reports')

      await Promise.all([
        queryClient.invalidateQueries({ queryKey: getGetReportsQueryKey() }),
        queryClient.invalidateQueries({
          queryKey: getGetReportDetailsQueryKey(variables.reportId),
        }),
      ])
    },
    onError: (error) => {
      toast.error(
        getApiErrorMessage(error, 'The report title could not be updated.'),
        'Reports',
      )
    },
  })

  const getCurrentTitle = () =>
    selectedReport?.title ?? selectedReportSummary?.title ?? ''

  const startRename = () => {
    setTitleDraft(getCurrentTitle())
    setIsEditingTitle(true)
  }

  const cancelRename = () => {
    setTitleDraft(getCurrentTitle())
    setIsEditingTitle(false)
  }

  const closeEditor = () => {
    setIsEditingTitle(false)
  }

  const saveTitle = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!activeReportId) {
      return
    }

    const title = titleDraft.trim()
    if (!title) {
      toast.error('Report title cannot be empty.', 'Reports')
      return
    }

    renameReportMutation.mutate({ reportId: activeReportId, title })
  }

  return {
    cancelRename,
    closeEditor,
    isEditingTitle,
    isRenamePending: renameReportMutation.isPending,
    saveTitle,
    setTitleDraft,
    startRename,
    titleDraft,
  }
}
