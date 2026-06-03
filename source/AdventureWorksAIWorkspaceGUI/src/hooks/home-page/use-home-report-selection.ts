import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  getGetReportDetailsQueryKey,
  getGetReportsQueryKey,
  getReportDetails,
  getReports,
} from '@/api/generated/reports/reports'
import { createReportViewData } from '@/components/home/home-report-utils'

export function useHomeReportSelection() {
  const queryClient = useQueryClient()
  const [selectedReportId, setSelectedReportId] = useState<string | null>(null)
  const [isNewReportSelected, setIsNewReportSelected] = useState(false)

  const reportsQuery = useQuery({
    queryKey: getGetReportsQueryKey(),
    queryFn: ({ signal }) => getReports({ signal }),
    retry: false,
  })

  const reports =
    reportsQuery.data?.status === 200 ? reportsQuery.data.data.reports : []

  const activeReportId = isNewReportSelected
    ? null
    : (selectedReportId ?? reports[0]?.id ?? null)

  const selectedReportSummary =
    reports.find((item) => item.id === activeReportId) ?? null

  const reportDetailsQuery = useQuery({
    queryKey: getGetReportDetailsQueryKey(activeReportId ?? undefined),
    queryFn: ({ signal }) => getReportDetails(activeReportId!, { signal }),
    enabled: Boolean(activeReportId),
    retry: false,
  })

  const selectedReport =
    reportDetailsQuery.data?.status === 200
      ? reportDetailsQuery.data.data
      : null
  const reportViewData = selectedReport
    ? createReportViewData(selectedReport)
    : null

  const openReport = (reportId: string) => {
    setSelectedReportId(reportId)
    setIsNewReportSelected(false)
  }

  const startNewReport = () => {
    setSelectedReportId(null)
    setIsNewReportSelected(true)
  }

  const removeDeletedReport = (reportId: string) => {
    if (selectedReportId === reportId) {
      setSelectedReportId(null)
    }

    setIsNewReportSelected(false)
  }

  const refreshReports = () => {
    void queryClient.invalidateQueries({ queryKey: getGetReportsQueryKey() })
  }

  const refreshActiveReport = () => {
    if (!activeReportId) {
      return
    }

    void queryClient.invalidateQueries({
      queryKey: getGetReportDetailsQueryKey(activeReportId),
    })
  }

  return {
    activeReportId,
    isNewReportSelected,
    openReport,
    refreshActiveReport,
    refreshReports,
    removeDeletedReport,
    reportDetailsQuery,
    reportViewData,
    reports,
    reportsQuery,
    selectedReport,
    selectedReportSummary,
    startNewReport,
  }
}
