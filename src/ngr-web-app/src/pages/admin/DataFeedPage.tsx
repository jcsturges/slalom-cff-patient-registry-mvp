import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
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
  FormControl,
  FormControlLabel,
  FormLabel,
  Radio,
  RadioGroup,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import BarChartIcon from '@mui/icons-material/BarChart';
import { dataFeedService, type FeedRun, type ReconciliationReport } from '../../services/dataFeed';

function StatusChip({ status }: { status: FeedRun['status'] }) {
  const color = {
    Pending:   'default',
    Running:   'info',
    Completed: 'success',
    Failed:    'error',
  }[status] as 'default' | 'info' | 'success' | 'error';
  return <Chip label={status} color={color} size="small" variant="outlined" />;
}

function ReconciliationModal({
  report,
  open,
  onClose,
}: {
  report: ReconciliationReport;
  open: boolean;
  onClose: () => void;
}) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Reconciliation Report</DialogTitle>
      <DialogContent>
        <Box display="flex" gap={4} mb={2} flexWrap="wrap">
          <Box>
            <Typography variant="caption" color="text.secondary">Window</Typography>
            <Typography variant="body2">
              {new Date(report.extractionWindowStart).toLocaleString()}
              {' → '}
              {new Date(report.extractionWindowEnd).toLocaleString()}
            </Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Run Type</Typography>
            <Typography variant="body2">{report.runType}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Total Records</Typography>
            <Typography variant="body2">{report.totalRecords.toLocaleString()}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Quality Rate</Typography>
            <Typography variant="body2" color={report.qualityRate >= 99.9 ? 'success.main' : 'warning.main'}>
              {report.qualityRate.toFixed(4)}%
            </Typography>
          </Box>
        </Box>
        <Divider sx={{ mb: 2 }} />
        <Typography variant="subtitle2" gutterBottom>Entity Counts</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Entity</TableCell>
              <TableCell align="right">Creates</TableCell>
              <TableCell align="right">Updates</TableCell>
              <TableCell align="right">Deletes</TableCell>
              <TableCell align="right">Total</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {report.entities.map((e) => (
              <TableRow key={e.entityType}>
                <TableCell>{e.entityType}</TableCell>
                <TableCell align="right">{e.creates}</TableCell>
                <TableCell align="right">{e.updates}</TableCell>
                <TableCell align="right">{e.deletes}</TableCell>
                <TableCell align="right"><strong>{e.total}</strong></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}

export function DataFeedPage() {
  const queryClient = useQueryClient();
  const [triggerOpen, setTriggerOpen] = useState(false);
  const [runType, setRunType] = useState<'Delta' | 'Full'>('Delta');
  const [windowOverride, setWindowOverride] = useState('');
  const [selectedReport, setSelectedReport] = useState<ReconciliationReport | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ['feedRuns'],
    queryFn: () => dataFeedService.getRuns(1, 50),
    refetchInterval: 15_000,
  });

  const triggerMutation = useMutation({
    mutationFn: () => dataFeedService.triggerRun({
      runType,
      windowOverrideStart: windowOverride || undefined,
    }),
    onSuccess: () => {
      setTriggerOpen(false);
      void queryClient.invalidateQueries({ queryKey: ['feedRuns'] });
    },
  });

  if (isLoading) {
    return <Box display="flex" justifyContent="center" py={6}><CircularProgress /></Box>;
  }

  if (error) {
    return (
      <Alert severity="error">
        Unable to load feed runs. Ensure you have System Admin access.
      </Alert>
    );
  }

  const runs = data?.runs ?? [];
  const hasRunning = runs.some((r) => r.status === 'Running');

  return (
    <Box>
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Nightly Data Feed</Typography>
          <Typography variant="body2" color="text.secondary" mt={0.5}>
            Outbound data feed to the CFF Data Warehouse. Runs automatically each night; manual runs available here.
          </Typography>
        </Box>
        <Tooltip title={hasRunning ? 'A feed run is already in progress' : ''}>
          <span>
            <Button
              variant="contained"
              startIcon={<PlayArrowIcon />}
              onClick={() => setTriggerOpen(true)}
              disabled={hasRunning}
            >
              Trigger Run
            </Button>
          </span>
        </Tooltip>
      </Box>

      {/* Run history */}
      {runs.length === 0 ? (
        <Alert severity="info">No feed runs yet. Trigger the first run above.</Alert>
      ) : (
        <Table>
          <TableHead>
            <TableRow sx={{ '& th': { bgcolor: 'primary.main', color: 'white' } }}>
              <TableCell>ID</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Window Start</TableCell>
              <TableCell>Window End</TableCell>
              <TableCell align="right">Records</TableCell>
              <TableCell align="right">Errors</TableCell>
              <TableCell>Triggered By</TableCell>
              <TableCell>Completed</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {runs.map((run) => (
              <TableRow key={run.id} hover>
                <TableCell>{run.id}</TableCell>
                <TableCell>
                  <Chip label={run.runType} size="small"
                    color={run.runType === 'Full' ? 'secondary' : 'default'} variant="outlined" />
                </TableCell>
                <TableCell><StatusChip status={run.status} /></TableCell>
                <TableCell>
                  {run.windowStart ? new Date(run.windowStart).toLocaleString() : '—'}
                </TableCell>
                <TableCell>
                  {run.windowEnd ? new Date(run.windowEnd).toLocaleString() : '—'}
                </TableCell>
                <TableCell align="right">{run.totalRecords.toLocaleString()}</TableCell>
                <TableCell align="right">
                  {run.errorCount > 0 ? (
                    <Typography color="error.main" variant="body2">{run.errorCount}</Typography>
                  ) : run.errorCount}
                </TableCell>
                <TableCell>{run.triggeredBy}</TableCell>
                <TableCell>
                  {run.completedAt ? new Date(run.completedAt).toLocaleString() : '—'}
                </TableCell>
                <TableCell>
                  {run.reconciliation && (
                    <Tooltip title="View reconciliation report">
                      <Button size="small" startIcon={<BarChartIcon />}
                        onClick={() => setSelectedReport(run.reconciliation!)}>
                        Report
                      </Button>
                    </Tooltip>
                  )}
                  {run.errorMessage && (
                    <Tooltip title={run.errorMessage}>
                      <Typography variant="caption" color="error.main">Error</Typography>
                    </Tooltip>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      {/* Trigger dialog */}
      <Dialog open={triggerOpen} onClose={() => setTriggerOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Trigger Data Feed Run</DialogTitle>
        <DialogContent>
          <FormControl component="fieldset" sx={{ mb: 2 }}>
            <FormLabel component="legend">Run Type</FormLabel>
            <RadioGroup value={runType} onChange={(e) => setRunType(e.target.value as 'Delta' | 'Full')}>
              <FormControlLabel value="Delta" control={<Radio />}
                label="Delta — records changed since last successful run" />
              <FormControlLabel value="Full" control={<Radio />}
                label="Full Resync — all current records + tombstones" />
            </RadioGroup>
          </FormControl>
          {runType === 'Full' && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              Full resync may take a significant amount of time and should only be triggered
              during off-peak hours to avoid performance impact.
            </Alert>
          )}
          <TextField
            label="Window Override Start (optional)"
            placeholder="e.g. 2024-01-01T00:00:00Z"
            fullWidth
            size="small"
            value={windowOverride}
            onChange={(e) => setWindowOverride(e.target.value)}
            helperText="Leave blank to use the last successful run's window end as the start."
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTriggerOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => triggerMutation.mutate()}
            disabled={triggerMutation.isPending}
          >
            {triggerMutation.isPending ? 'Triggering...' : 'Trigger'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reconciliation report modal */}
      {selectedReport && (
        <ReconciliationModal
          report={selectedReport}
          open
          onClose={() => setSelectedReport(null)}
        />
      )}
    </Box>
  );
}
