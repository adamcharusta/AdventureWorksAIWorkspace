import Stack from '@mui/material/Stack'
import type { FormEvent } from 'react'

import { ReportActionsMenu } from './ReportActionsMenu'
import { ReportTitleEditor } from './ReportTitleEditor'
import { ReportTitleSummary } from './ReportTitleSummary'

type HomeHeaderProps = {
  description: string
  hasActiveReport: boolean
  hasSqlQueries: boolean
  isEditingTitle: boolean
  isRenamePending: boolean
  onCancelRename: () => void
  onCopySql: () => Promise<void> | void
  onOpenDeleteDialog: () => void
  onSaveTitle: (event: FormEvent<HTMLFormElement>) => void
  onStartRename: () => void
  onTitleDraftChange: (value: string) => void
  title: string
  titleDraft: string
}

export function HomeHeader({
  description,
  hasActiveReport,
  hasSqlQueries,
  isEditingTitle,
  isRenamePending,
  onCancelRename,
  onCopySql,
  onOpenDeleteDialog,
  onSaveTitle,
  onStartRename,
  onTitleDraftChange,
  title,
  titleDraft,
}: HomeHeaderProps) {
  return (
    <Stack
      direction="row"
      spacing={2}
      sx={{
        alignItems: 'flex-start',
        justifyContent: 'space-between',
      }}
    >
      {isEditingTitle && hasActiveReport ? (
        <ReportTitleEditor
          isSaving={isRenamePending}
          onCancel={onCancelRename}
          onChange={onTitleDraftChange}
          onSubmit={onSaveTitle}
          titleDraft={titleDraft}
        />
      ) : (
        <ReportTitleSummary description={description} title={title} />
      )}

      <ReportActionsMenu
        hasActiveReport={hasActiveReport}
        hasSqlQueries={hasSqlQueries}
        onCopySql={onCopySql}
        onOpenDeleteDialog={onOpenDeleteDialog}
        onStartRename={onStartRename}
      />
    </Stack>
  )
}
