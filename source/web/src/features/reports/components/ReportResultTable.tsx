import Paper from '@mui/material/Paper'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'

import type { TabularResult } from '@/features/reports/lib/report-types'

const MAX_VISIBLE_ROWS = 100

type ReportResultTableProps = {
  result: TabularResult
}

function formatCell(value: unknown): string {
  if (value === null || value === undefined) {
    return '—'
  }

  return String(value)
}

export function ReportResultTable({ result }: ReportResultTableProps) {
  const rows = result.rows.slice(0, MAX_VISIBLE_ROWS)

  return (
    <TableContainer
      component={Paper}
      variant="outlined"
      sx={{ maxHeight: 360 }}
    >
      <Table size="small" stickyHeader>
        <TableHead>
          <TableRow>
            {result.columns.map((column) => (
              <TableCell key={column.name}>{column.name}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row, rowIndex) => (
            <TableRow key={rowIndex}>
              {result.columns.map((column, columnIndex) => (
                <TableCell key={column.name}>
                  {formatCell(row[columnIndex])}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
