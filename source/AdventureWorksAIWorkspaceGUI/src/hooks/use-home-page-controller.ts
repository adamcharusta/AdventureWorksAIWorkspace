import { copyGeneratedSqlQueries } from '@/hooks/home-page/copy-generated-sql'
import { getHomeHeaderCopy } from '@/hooks/home-page/home-header-copy'
import { useHomeDrawers } from '@/hooks/home-page/use-home-drawers'
import { useHomeReportChat } from '@/hooks/home-page/use-home-report-chat'
import { useHomeReportDelete } from '@/hooks/home-page/use-home-report-delete'
import { useHomeReportSelection } from '@/hooks/home-page/use-home-report-selection'
import { useHomeReportTitle } from '@/hooks/home-page/use-home-report-title'
import { useHomeShell } from '@/hooks/home-page/use-home-shell'

export function useHomePageController() {
  const drawers = useHomeDrawers()
  const shell = useHomeShell()
  const selection = useHomeReportSelection()
  const titleActions = useHomeReportTitle({
    activeReportId: selection.activeReportId,
    selectedReport: selection.selectedReport,
    selectedReportSummary: selection.selectedReportSummary,
  })
  const chatActions = useHomeReportChat({
    activeReportId: selection.activeReportId,
    onReportReady: selection.openReport,
    onSuccess: titleActions.closeEditor,
  })
  const deleteActions = useHomeReportDelete({
    activeReportId: selection.activeReportId,
    onDeleted: (reportId) => {
      titleActions.closeEditor()
      selection.removeDeletedReport(reportId)
    },
    selectedReport: selection.selectedReport,
    selectedReportSummary: selection.selectedReportSummary,
  })

  const sqlQueries = selection.selectedReport?.generatedSqlQueries ?? []
  const hasActiveReport = Boolean(selection.activeReportId)

  const handleNewReport = () => {
    selection.startNewReport()
    titleActions.closeEditor()
  }

  const handleSelectReport = (reportId: string) => {
    selection.openReport(reportId)
    titleActions.closeEditor()
  }

  const headerCopy = getHomeHeaderCopy({
    hasActiveReport,
    isNewReportSelected: selection.isNewReportSelected,
    selectedReport: selection.selectedReport,
    selectedReportSummary: selection.selectedReportSummary,
  })

  return {
    chat: {
      isSubmitting: chatActions.isSubmitting,
      messages: selection.selectedReport?.messages ?? [],
      onSubmit: chatActions.submitMessage,
      onToggle: drawers.chat.onToggle,
      open: drawers.chat.open,
      subtitle: selection.selectedReport?.title ?? 'New report',
    },
    deleteDialog: deleteActions.deleteDialog,
    header: {
      description: headerCopy.description,
      hasActiveReport,
      hasSqlQueries: sqlQueries.length > 0,
      isEditingTitle: titleActions.isEditingTitle,
      isRenamePending: titleActions.isRenamePending,
      onCancelRename: titleActions.cancelRename,
      onCopySql: () => copyGeneratedSqlQueries(sqlQueries),
      onOpenDeleteDialog: deleteActions.openDeleteDialog,
      onSaveTitle: titleActions.saveTitle,
      onStartRename: titleActions.startRename,
      onTitleDraftChange: titleActions.setTitleDraft,
      title: headerCopy.title,
      titleDraft: titleActions.titleDraft,
    },
    menu: {
      activeReportId: selection.activeReportId,
      isAdmin: shell.isAdmin,
      isNewReportSelected: selection.isNewReportSelected,
      isReportsError: selection.reportsQuery.isError,
      isReportsLoading: selection.reportsQuery.isLoading,
      mode: shell.mode,
      onLogout: shell.onLogout,
      onNewReport: handleNewReport,
      onOpenAdminPanel: shell.onOpenAdminPanel,
      onRefreshReports: selection.refreshReports,
      onSelectReport: handleSelectReport,
      onToggle: drawers.menu.onToggle,
      onToggleTheme: shell.onToggleTheme,
      open: drawers.menu.open,
      reports: selection.reports,
      username: shell.username,
    },
    workspace: {
      activeReportId: selection.activeReportId,
      isReportDetailsError: selection.reportDetailsQuery.isError,
      isReportDetailsLoading: selection.reportDetailsQuery.isLoading,
      isReportsError: selection.reportsQuery.isError,
      isReportsLoading: selection.reportsQuery.isLoading,
      onRefreshActiveReport: selection.refreshActiveReport,
      onRefreshReports: selection.refreshReports,
      reportViewData: selection.reportViewData,
      reportsCount: selection.reports.length,
      selectedReport: selection.selectedReport,
    },
  }
}
