import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import { getApiErrorMessage } from '@/lib/api-error'
import {
  deleteReport,
  type ReportDetailsDto,
  reportQueryKeys,
  type ReportSummaryDto,
} from '@/lib/report-api'
import { toast } from '@/lib/toast'

type UseHomeReportDeleteOptions = {
  activeReportId: string | null
  onDeleted: (reportId: string) => void
  selectedReport: ReportDetailsDto | null
  selectedReportSummary: ReportSummaryDto | null
}

export function useHomeReportDelete({
  activeReportId,
  onDeleted,
  selectedReport,
  selectedReportSummary,
}: UseHomeReportDeleteOptions) {
  const queryClient = useQueryClient()
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)

  const deleteReportMutation = useMutation({
    mutationFn: (reportId: string) => deleteReport(reportId),
    onSuccess: async (_response, reportId) => {
      setIsDeleteDialogOpen(false)
      onDeleted(reportId)
      toast.success('Report has been deleted.', 'Reports')

      queryClient.removeQueries({
        queryKey: reportQueryKeys.details(reportId),
      })
      await queryClient.invalidateQueries({ queryKey: reportQueryKeys.list() })
    },
    onError: (error) => {
      toast.error(
        getApiErrorMessage(error, 'The report could not be deleted.'),
        'Reports',
      )
    },
  })

  const closeDeleteDialog = () => {
    if (!deleteReportMutation.isPending) {
      setIsDeleteDialogOpen(false)
    }
  }

  const confirmDelete = () => {
    if (activeReportId) {
      deleteReportMutation.mutate(activeReportId)
    }
  }

  return {
    deleteDialog: {
      isDeleting: deleteReportMutation.isPending,
      onClose: closeDeleteDialog,
      onConfirm: confirmDelete,
      open: isDeleteDialogOpen,
      reportName:
        selectedReport?.title ?? selectedReportSummary?.title ?? 'this report',
    },
    openDeleteDialog: () => setIsDeleteDialogOpen(true),
  }
}
