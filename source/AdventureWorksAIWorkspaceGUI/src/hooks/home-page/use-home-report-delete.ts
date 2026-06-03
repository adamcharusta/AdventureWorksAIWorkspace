import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import type { ReportDetailsDto, ReportSummaryDto } from '@/api/generated/model'
import {
  deleteReport,
  getGetReportDetailsQueryKey,
  getGetReportsQueryKey,
} from '@/api/generated/reports/reports'
import { getApiErrorMessage } from '@/lib/api-error'
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
        queryKey: getGetReportDetailsQueryKey(reportId),
      })
      await queryClient.invalidateQueries({ queryKey: getGetReportsQueryKey() })
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
