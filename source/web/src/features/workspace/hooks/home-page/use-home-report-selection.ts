import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'

import {
  getGetReportDetailsQueryKey,
  getGetReportsQueryKey,
  getReportDetails,
  getReports,
} from '@/api/generated/reports/reports'
import { createReportViewData } from '@/features/workspace/components/home/home-report-utils'

export function useHomeReportSelection() {
  const navigate = useNavigate()
  const { reportId: routeReportId } = useParams<{ reportId?: string }>()
  const queryClient = useQueryClient()

  const reportsQuery = useQuery({
    queryKey: getGetReportsQueryKey(),
    queryFn: ({ signal }) => getReports({ signal }),
    retry: false,
  })

  const reports =
    reportsQuery.data?.status === 200 ? reportsQuery.data.data.reports : []

  const activeReportId = routeReportId ?? null
  const isNewReportSelected = !activeReportId

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
    navigate(`/raport/${encodeURIComponent(reportId)}`)
  }

  const startNewReport = () => {
    navigate('/')
  }

  const removeDeletedReport = (reportId: string) => {
    if (activeReportId === reportId) {
      navigate('/')
    }
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
