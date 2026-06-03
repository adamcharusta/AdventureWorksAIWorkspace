import CheckRoundedIcon from '@mui/icons-material/CheckRounded'
import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import TextField from '@mui/material/TextField'
import Tooltip from '@mui/material/Tooltip'
import type { FormEvent } from 'react'

type ReportTitleEditorProps = {
  isSaving: boolean
  onCancel: () => void
  onChange: (value: string) => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  titleDraft: string
}

export function ReportTitleEditor({
  isSaving,
  onCancel,
  onChange,
  onSubmit,
  titleDraft,
}: ReportTitleEditorProps) {
  return (
    <Box
      component="form"
      onSubmit={onSubmit}
      sx={{
        alignItems: 'center',
        display: 'flex',
        flex: '1 1 auto',
        gap: 1,
        minWidth: 0,
      }}
    >
      <TextField
        disabled={isSaving}
        fullWidth
        onChange={(event) => onChange(event.target.value)}
        size="small"
        slotProps={{
          htmlInput: { 'aria-label': 'Report title', maxLength: 256 },
        }}
        value={titleDraft}
      />
      <Tooltip title="Save title">
        <span>
          <IconButton
            aria-label="Save title"
            color="primary"
            disabled={isSaving || !titleDraft.trim()}
            type="submit"
          >
            <CheckRoundedIcon />
          </IconButton>
        </span>
      </Tooltip>
      <Tooltip title="Cancel rename">
        <span>
          <IconButton
            aria-label="Cancel rename"
            disabled={isSaving}
            onClick={onCancel}
            type="button"
          >
            <CloseRoundedIcon />
          </IconButton>
        </span>
      </Tooltip>
    </Box>
  )
}
