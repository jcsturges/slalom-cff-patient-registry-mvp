import { useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Collapse,
  Link,
  Stack,
  Typography,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import { useNavigate } from 'react-router-dom';
import { DataTable, type DataTableColumn } from './DataTable';
import type { ReportResultDto } from '../types';

interface ReportResultsViewProps {
  result: ReportResultDto;
  onDownload: (format: 'csv' | 'excel') => void;
  downloading?: boolean;
}

export function ReportResultsView({ result, onDownload, downloading = false }: ReportResultsViewProps) {
  const navigate = useNavigate();
  const [queryExpanded, setQueryExpanded] = useState(false);

  const columns: DataTableColumn<Record<string, unknown>>[] = result.columns.map((col) => ({
    id: col,
    label: col.replace(/([A-Z])/g, ' $1').replace(/^./, (s) => s.toUpperCase()),
    render: col === 'cffId'
      ? (row: Record<string, unknown>) => (
          <Link
            component="span"
            sx={{ cursor: 'pointer', fontWeight: 600 }}
            onClick={(e: React.MouseEvent) => {
              e.stopPropagation();
              // Find patient ID from the row — CFF ID is shown, navigate by search
              navigate(`/patients?search=${row.cffId}`);
            }}
          >
            {String(row[col] ?? '')}
          </Link>
        )
      : undefined,
  }));

  return (
    <Box>
      {/* ── Report Metadata ───────────────────────────────────── */}
      <Box sx={{ mb: 3, p: 2, bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider', borderRadius: 1 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Box>
            <Typography variant="h5" fontWeight={600}>{result.reportTitle}</Typography>
            <Typography variant="body2" color="text.secondary">
              Executed by {result.executedBy} on {new Date(result.executedAt).toLocaleString()}
            </Typography>
            <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
              <Chip label={`${result.recordCount} records`} size="small" />
              <Chip label={`${result.executionTimeMs}ms`} size="small" variant="outlined" />
            </Stack>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button
              size="small"
              variant="outlined"
              startIcon={<DownloadIcon />}
              onClick={() => onDownload('csv')}
              disabled={downloading}
            >
              CSV
            </Button>
            <Button
              size="small"
              variant="outlined"
              startIcon={<DownloadIcon />}
              onClick={() => onDownload('excel')}
              disabled={downloading}
            >
              Excel
            </Button>
          </Stack>
        </Stack>

        {result.querySummary && (
          <Box sx={{ mt: 1 }}>
            <Button
              size="small"
              endIcon={queryExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
              onClick={() => setQueryExpanded(!queryExpanded)}
            >
              Query Details
            </Button>
            <Collapse in={queryExpanded}>
              <Typography variant="body2" sx={{ mt: 1, whiteSpace: 'pre-wrap', fontFamily: 'monospace', fontSize: '0.8rem' }}>
                {result.querySummary}
              </Typography>
            </Collapse>
          </Box>
        )}
      </Box>

      {/* ── Results Table ─────────────────────────────────────── */}
      <DataTable
        columns={columns}
        rows={result.rows}
        getRowId={(row) => String(row['cffId'] ?? JSON.stringify(row).slice(0, 20))}
        emptyMessage="No results found."
        searchPlaceholder="Search results..."
      />
    </Box>
  );
}
