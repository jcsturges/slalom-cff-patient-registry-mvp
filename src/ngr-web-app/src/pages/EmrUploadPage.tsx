import { useCallback, useRef, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  LinearProgress,
  List,
  ListItem,
  ListItemText,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import CloseIcon from '@mui/icons-material/Close';
import { useRoles } from '../hooks/useRoles';
import { useProgram } from '../contexts/ProgramContext';
import { emrImportService, type ImportJobDetailDto, type ImportJobDto } from '../services/emrImport';

// ── Status chip ───────────────────────────────────────────────────────────────

function StatusChip({ status }: { status: string }) {
  const map: Record<string, { color: 'success' | 'error' | 'warning' | 'info' | 'default'; label: string }> = {
    Completed:        { color: 'success', label: 'Completed' },
    Failed:           { color: 'error',   label: 'Failed' },
    Processing:       { color: 'info',    label: 'Processing…' },
    Pending:          { color: 'info',    label: 'Queued' },
    ValidationFailed: { color: 'warning', label: 'Validation Failed' },
  };
  const cfg = map[status] ?? { color: 'default', label: status };
  return <Chip label={cfg.label} color={cfg.color} size="small" variant="outlined" />;
}

// ── Error detail dialog ───────────────────────────────────────────────────────

function ErrorDetailDialog({
  open,
  jobId,
  onClose,
}: {
  open: boolean;
  jobId: number | null;
  onClose: () => void;
}) {
  const { data, isLoading } = useQuery<ImportJobDetailDto>({
    queryKey: ['import-job-detail', jobId],
    queryFn: () => emrImportService.getJobErrors(jobId!),
    enabled: open && jobId != null,
  });

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        Import Report
        <IconButton onClick={onClose} size="small" aria-label="Close"><CloseIcon /></IconButton>
      </DialogTitle>
      <DialogContent dividers>
        {isLoading && <CircularProgress size={24} />}
        {data && (
          <Stack spacing={2}>
            <Stack direction="row" spacing={2}>
              <Typography variant="body2"><strong>File:</strong> {data.fileName}</Typography>
              <Typography variant="body2"><strong>Rows processed:</strong> {data.processedRows ?? '—'}</Typography>
            </Stack>

            {data.errors.length > 0 && (
              <Box>
                <Typography variant="subtitle2" color="error" gutterBottom>
                  <ErrorOutlineIcon fontSize="small" sx={{ mr: 0.5, verticalAlign: 'middle' }} />
                  Errors ({data.errors.length})
                </Typography>
                <List dense disablePadding>
                  {data.errors.map((e, i) => (
                    <ListItem key={i} disableGutters>
                      <ListItemText
                        primary={e.message}
                        secondary={[
                          e.rowNumber != null && `Row ${e.rowNumber}`,
                          e.csvColumn && `Column: ${e.csvColumn}`,
                        ].filter(Boolean).join(' · ')}
                      />
                    </ListItem>
                  ))}
                </List>
              </Box>
            )}

            {data.warnings.length > 0 && (
              <Box>
                <Divider />
                <Typography variant="subtitle2" color="warning.dark" gutterBottom sx={{ mt: 1 }}>
                  <WarningAmberIcon fontSize="small" sx={{ mr: 0.5, verticalAlign: 'middle' }} />
                  Warnings ({data.warnings.length})
                </Typography>
                <List dense disablePadding>
                  {data.warnings.map((w, i) => (
                    <ListItem key={i} disableGutters>
                      <ListItemText
                        primary={w.message}
                        secondary={[
                          w.rowNumber != null && `Row ${w.rowNumber}`,
                          w.csvColumn && `Column: ${w.csvColumn}`,
                        ].filter(Boolean).join(' · ')}
                      />
                    </ListItem>
                  ))}
                </List>
              </Box>
            )}

            {data.errors.length === 0 && data.warnings.length === 0 && (
              <Alert severity="success">No issues found — import completed cleanly.</Alert>
            )}
          </Stack>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function EmrUploadPage() {
  const { canCreatePatient } = useRoles();
  const { selectedProgram } = useProgram();
  const queryClient = useQueryClient();

  const fileInputRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploadSuccess, setUploadSuccess] = useState<string | null>(null);
  const [detailJobId, setDetailJobId] = useState<number | null>(null);

  const programId = selectedProgram?.programId ?? 0;

  const { data: jobs = [], isLoading } = useQuery<ImportJobDto[]>({
    queryKey: ['import-jobs', programId],
    queryFn: () => emrImportService.getJobs(programId),
    enabled: programId > 0,
    refetchInterval: (query) => {
      const data = query.state.data as ImportJobDto[] | undefined;
      const hasActive = data?.some(j => j.status === 'Processing' || j.status === 'Pending');
      return hasActive ? 3000 : false;
    },
  });

  if (!canCreatePatient) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        You do not have permission to upload EMR data. Requires ClinicalUser role or higher.
      </Alert>
    );
  }

  const handleFile = useCallback(async (file: File) => {
    if (!file.name.toLowerCase().endsWith('.csv')) {
      setUploadError('Only .csv files are accepted.');
      return;
    }
    if (file.size > 50 * 1024 * 1024) {
      setUploadError('File exceeds the 50 MB maximum size.');
      return;
    }
    if (!programId) {
      setUploadError('No program selected. Please select a program first.');
      return;
    }

    setUploadError(null);
    setUploadSuccess(null);
    setUploading(true);
    setUploadProgress(0);

    try {
      const job = await emrImportService.upload(file, programId, setUploadProgress);
      await queryClient.invalidateQueries({ queryKey: ['import-jobs', programId] });

      if (job.status === 'ValidationFailed') {
        setUploadError('File validation failed. Click the report icon next to the job to see details.');
      } else {
        setUploadSuccess(`File "${file.name}" uploaded successfully. Processing has started.`);
      }
    } catch (err) {
      setUploadError((err as Error).message ?? 'Upload failed.');
    } finally {
      setUploading(false);
      setUploadProgress(0);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  }, [programId, queryClient]);

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) void handleFile(file);
  }, [handleFile]);

  const onFileChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) void handleFile(file);
  }, [handleFile]);

  return (
    <Box>
      <Typography variant="h4" gutterBottom>EMR Upload</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Upload a CSV export from your EMR system to pre-populate Demographics, Encounter, and
        Labs &amp; Tests forms for patients with established Registry records.
      </Typography>

      {/* ── Drop zone ─────────────────────────────────────────────────────── */}
      <Paper
        variant="outlined"
        onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
        onDragLeave={() => setDragging(false)}
        onDrop={onDrop}
        sx={{
          p: 5,
          mb: 3,
          textAlign: 'center',
          borderStyle: 'dashed',
          borderWidth: 2,
          borderColor: dragging ? 'primary.main' : 'divider',
          bgcolor: dragging ? 'action.hover' : 'background.paper',
          cursor: uploading ? 'not-allowed' : 'pointer',
          transition: 'border-color 0.15s, background-color 0.15s',
        }}
        onClick={() => !uploading && fileInputRef.current?.click()}
        role="button"
        aria-label="Drop CSV file here or click to browse"
        tabIndex={0}
        onKeyDown={(e) => e.key === 'Enter' && !uploading && fileInputRef.current?.click()}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept=".csv"
          hidden
          onChange={onFileChange}
          aria-label="CSV file input"
        />

        {uploading ? (
          <Stack spacing={2} alignItems="center">
            <CircularProgress size={40} />
            <Typography variant="body1">Uploading… {uploadProgress}%</Typography>
            <Box sx={{ width: '100%', maxWidth: 400 }}>
              <LinearProgress variant="determinate" value={uploadProgress} />
            </Box>
          </Stack>
        ) : (
          <Stack spacing={1} alignItems="center">
            <CloudUploadIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
            <Typography variant="h6">Drag &amp; drop a CSV file here</Typography>
            <Typography variant="body2" color="text.secondary">or click to browse</Typography>
            <Typography variant="caption" color="text.disabled">
              Accepted: .csv · Max size: 50 MB
            </Typography>
          </Stack>
        )}
      </Paper>

      {/* ── Feedback ──────────────────────────────────────────────────────── */}
      {uploadError && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setUploadError(null)}>
          {uploadError}
        </Alert>
      )}
      {uploadSuccess && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setUploadSuccess(null)}>
          {uploadSuccess}
        </Alert>
      )}

      {/* ── Format note ───────────────────────────────────────────────────── */}
      <Alert severity="info" icon={<InfoOutlinedIcon />} sx={{ mb: 3 }}>
        <strong>Required columns:</strong> CFF_ID, MRN. Only patients with an active assignment
        to the selected program will be updated. Forms updated by EMR will be flagged for review.
      </Alert>

      {/* ── Upload history ────────────────────────────────────────────────── */}
      <Typography variant="h6" gutterBottom>Upload History</Typography>

      {!programId ? (
        <Alert severity="warning">Select a program to view upload history.</Alert>
      ) : isLoading ? (
        <CircularProgress size={24} />
      ) : jobs.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">No uploads yet for this program.</Typography>
        </Paper>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>File</TableCell>
                <TableCell align="right">Rows</TableCell>
                <TableCell align="right">Processed</TableCell>
                <TableCell align="right">Errors</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Uploaded by</TableCell>
                <TableCell align="center">Report</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {jobs.map((job) => (
                <TableRow key={job.id} hover>
                  <TableCell>
                    {new Date(job.createdAt).toLocaleString()}
                  </TableCell>
                  <TableCell sx={{ maxWidth: 240, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    <Tooltip title={job.fileName}>
                      <span>{job.fileName}</span>
                    </Tooltip>
                  </TableCell>
                  <TableCell align="right">{job.totalRows ?? '—'}</TableCell>
                  <TableCell align="right">{job.processedRows ?? '—'}</TableCell>
                  <TableCell align="right">
                    {(job.errorRows ?? 0) > 0 ? (
                      <Typography variant="body2" color="error.main" fontWeight={600}>
                        {job.errorRows}
                      </Typography>
                    ) : (
                      job.errorRows ?? '—'
                    )}
                  </TableCell>
                  <TableCell><StatusChip status={job.status} /></TableCell>
                  <TableCell>{job.createdBy}</TableCell>
                  <TableCell align="center">
                    <Tooltip title="View errors & warnings">
                      <span>
                        <IconButton
                          size="small"
                          onClick={() => setDetailJobId(job.id)}
                          aria-label={`View report for ${job.fileName}`}
                        >
                          {job.status === 'Completed' && (job.errorRows ?? 0) === 0
                            ? <CheckCircleOutlineIcon fontSize="small" color="success" />
                            : job.status === 'Failed' || (job.errorRows ?? 0) > 0
                            ? <ErrorOutlineIcon fontSize="small" color="error" />
                            : <InfoOutlinedIcon fontSize="small" color="action" />}
                        </IconButton>
                      </span>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <ErrorDetailDialog
        open={detailJobId != null}
        jobId={detailJobId}
        onClose={() => setDetailJobId(null)}
      />
    </Box>
  );
}
