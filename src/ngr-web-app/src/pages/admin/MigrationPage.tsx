import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import VerifiedIcon from '@mui/icons-material/Verified';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import { migrationService, type MigrationRun, type ValidationReport } from '../../services/migration';

function StatusChip({ status }: { status: MigrationRun['status'] }) {
  const color = {
    Pending:       'default',
    Running:       'info',
    Completed:     'success',
    Failed:        'error',
    PartialSuccess: 'warning',
  }[status] as 'default' | 'info' | 'success' | 'error' | 'warning';
  return <Chip label={status} color={color} size="small" variant="outlined" />;
}

function ValidationModal({
  report,
  open,
  onClose,
}: {
  report: ValidationReport;
  open: boolean;
  onClose: () => void;
}) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box display="flex" alignItems="center" gap={1}>
          {report.overallStatus === 'Pass' ? (
            <CheckCircleIcon color="success" />
          ) : (
            <CancelIcon color="error" />
          )}
          Validation Report — {report.overallStatus}
        </Box>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" gap={4} mb={2} flexWrap="wrap">
          <Box>
            <Typography variant="caption" color="text.secondary">Generated</Typography>
            <Typography variant="body2">{new Date(report.generatedAt).toLocaleString()}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Total Checks</Typography>
            <Typography variant="body2">{report.totalChecks}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Passed</Typography>
            <Typography variant="body2" color="success.main">{report.passedChecks}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">Failed</Typography>
            <Typography variant="body2" color={report.failedChecks > 0 ? 'error.main' : 'text.primary'}>
              {report.failedChecks}
            </Typography>
          </Box>
        </Box>
        <Divider sx={{ mb: 2 }} />
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Check</TableCell>
              <TableCell>Entity</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Expected</TableCell>
              <TableCell align="right">Actual</TableCell>
              <TableCell>Detail</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {report.checks.map((c, i) => (
              <TableRow key={i}>
                <TableCell>{c.checkName}</TableCell>
                <TableCell>{c.entityType}</TableCell>
                <TableCell>
                  <Chip
                    label={c.status}
                    color={c.status === 'Pass' ? 'success' : 'error'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">{c.expectedCount ?? '—'}</TableCell>
                <TableCell align="right">{c.actualCount ?? '—'}</TableCell>
                <TableCell>
                  <Typography variant="caption" color="text.secondary">{c.detail ?? '—'}</Typography>
                </TableCell>
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

export function MigrationPage() {
  const queryClient = useQueryClient();
  const [selectedValidation, setSelectedValidation] = useState<ValidationReport | null>(null);

  const { data: phasesData, isLoading: phasesLoading } = useQuery({
    queryKey: ['migrationPhases'],
    queryFn: () => migrationService.getPhases(),
  });

  const { data: runsData, isLoading: runsLoading } = useQuery({
    queryKey: ['migrationRuns'],
    queryFn: () => migrationService.getRuns(1, 50),
    refetchInterval: 15_000,
  });

  const triggerMutation = useMutation({
    mutationFn: (phase: string) => migrationService.triggerPhase(phase),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['migrationRuns'] });
    },
  });

  const validateMutation = useMutation({
    mutationFn: (id: number) => migrationService.validateRun(id),
    onSuccess: (report) => {
      setSelectedValidation(report);
      void queryClient.invalidateQueries({ queryKey: ['migrationRuns'] });
    },
  });

  if (phasesLoading || runsLoading) {
    return <Box display="flex" justifyContent="center" py={6}><CircularProgress /></Box>;
  }

  const phases = phasesData?.phases ?? [];
  const runs = runsData?.runs ?? [];
  const hasRunning = runs.some((r) => r.status === 'Running' || r.status === 'Pending');

  // Map latest run per phase for phase card status
  const latestRunByPhase = runs.reduce<Record<string, MigrationRun>>((acc, run) => {
    if (!acc[run.phase] || new Date(run.startedAt) > new Date(acc[run.phase].startedAt)) {
      acc[run.phase] = run;
    }
    return acc;
  }, {});

  const completedPhases = Object.values(latestRunByPhase).filter((r) => r.status === 'Completed').length;
  const progress = phases.length > 0 ? (completedPhases / phases.length) * 100 : 0;

  return (
    <Box>
      <Box display="flex" alignItems="flex-start" justifyContent="space-between" mb={3}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Historical Data Migration</Typography>
          <Typography variant="body2" color="text.secondary" mt={0.5}>
            Phase-based migration from the CFF Data Warehouse (portCF) into NGR. Each phase is
            independently re-runnable. Source connection must be configured in Key Vault.
          </Typography>
          {phasesData?.notes && phasesData.notes.length > 0 && (
            <Box mt={1}>
              {phasesData.notes.map((note, i) => (
                <Typography key={i} variant="caption" color="text.secondary" display="block">• {note}</Typography>
              ))}
            </Box>
          )}
        </Box>
      </Box>

      {/* Progress summary */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
            <Typography variant="subtitle2">Overall Progress</Typography>
            <Typography variant="body2" color="text.secondary">
              {completedPhases} / {phases.length} phases completed
            </Typography>
          </Box>
          <LinearProgress variant="determinate" value={progress} sx={{ height: 8, borderRadius: 4 }} />
          {phasesData?.migrationUser && (
            <Typography variant="caption" color="text.secondary" mt={1} display="block">
              Migration user: {phasesData.migrationUser}
            </Typography>
          )}
        </CardContent>
      </Card>

      {hasRunning && (
        <Alert severity="info" sx={{ mb: 3 }}>
          A migration phase is currently running. Triggering another phase is disabled until it completes.
        </Alert>
      )}

      {/* Phase cards */}
      <Grid container spacing={2} mb={4}>
        {phases.map((phase) => {
          const latestRun = latestRunByPhase[phase.id];
          const isRunning = latestRun?.status === 'Running' || latestRun?.status === 'Pending';

          return (
            <Grid item xs={12} sm={6} md={4} key={phase.id}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
                    <Typography variant="subtitle1" fontWeight={600}>{phase.id}</Typography>
                    {latestRun ? (
                      <StatusChip status={latestRun.status} />
                    ) : (
                      <Chip label="Not Started" size="small" variant="outlined" />
                    )}
                  </Box>
                  <Typography variant="body2" color="text.secondary" mb={1}>
                    {phase.description}
                  </Typography>
                  {phase.srsRef && (
                    <Typography variant="caption" color="text.secondary" display="block" mb={1}>
                      SRS: {phase.srsRef}
                    </Typography>
                  )}
                  {latestRun && (
                    <Box mb={1}>
                      <Typography variant="caption" color="text.secondary">
                        Source: {latestRun.sourceCount.toLocaleString()} →
                        Target: {latestRun.targetCount.toLocaleString()}
                        {latestRun.errorCount > 0 && (
                          <Typography component="span" variant="caption" color="error.main">
                            {' '}({latestRun.errorCount} errors)
                          </Typography>
                        )}
                      </Typography>
                    </Box>
                  )}
                  <Box display="flex" gap={1} mt={1}>
                    <Tooltip title={hasRunning && !isRunning ? 'Another phase is running' : ''}>
                      <span>
                        <Button
                          size="small"
                          variant="outlined"
                          startIcon={isRunning ? <CircularProgress size={14} /> : <PlayArrowIcon />}
                          onClick={() => triggerMutation.mutate(phase.id)}
                          disabled={hasRunning || triggerMutation.isPending}
                        >
                          {isRunning ? 'Running…' : latestRun ? 'Re-run' : 'Run'}
                        </Button>
                      </span>
                    </Tooltip>
                    {latestRun?.status === 'Completed' && (
                      <Button
                        size="small"
                        variant="outlined"
                        color="secondary"
                        startIcon={<VerifiedIcon />}
                        onClick={() => validateMutation.mutate(latestRun.id)}
                        disabled={validateMutation.isPending}
                      >
                        Validate
                      </Button>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          );
        })}
      </Grid>

      {/* Run history */}
      <Typography variant="h6" fontWeight={600} mb={2}>Run History</Typography>
      {runs.length === 0 ? (
        <Alert severity="info">No migration runs yet. Trigger a phase above to begin.</Alert>
      ) : (
        <Table>
          <TableHead>
            <TableRow sx={{ '& th': { bgcolor: 'primary.main', color: 'white' } }}>
              <TableCell>ID</TableCell>
              <TableCell>Phase</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Source</TableCell>
              <TableCell align="right">Target</TableCell>
              <TableCell align="right">Errors</TableCell>
              <TableCell>Triggered By</TableCell>
              <TableCell>Started</TableCell>
              <TableCell>Completed</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {runs.map((run) => (
              <TableRow key={run.id} hover>
                <TableCell>{run.id}</TableCell>
                <TableCell>
                  <Chip label={run.phase} size="small" variant="outlined" />
                </TableCell>
                <TableCell><StatusChip status={run.status} /></TableCell>
                <TableCell align="right">{run.sourceCount.toLocaleString()}</TableCell>
                <TableCell align="right">{run.targetCount.toLocaleString()}</TableCell>
                <TableCell align="right">
                  {run.errorCount > 0 ? (
                    <Typography color="error.main" variant="body2">{run.errorCount}</Typography>
                  ) : run.errorCount}
                </TableCell>
                <TableCell>{run.triggeredBy}</TableCell>
                <TableCell>{new Date(run.startedAt).toLocaleString()}</TableCell>
                <TableCell>{run.completedAt ? new Date(run.completedAt).toLocaleString() : '—'}</TableCell>
                <TableCell>
                  <Box display="flex" gap={1}>
                    {run.validationReport && (
                      <Tooltip title="View validation report">
                        <Button
                          size="small"
                          startIcon={<VerifiedIcon />}
                          onClick={() => setSelectedValidation(run.validationReport!)}
                        >
                          Report
                        </Button>
                      </Tooltip>
                    )}
                    {run.status === 'Completed' && !run.validationReport && (
                      <Button
                        size="small"
                        variant="outlined"
                        color="secondary"
                        onClick={() => validateMutation.mutate(run.id)}
                        disabled={validateMutation.isPending}
                      >
                        Validate
                      </Button>
                    )}
                    {run.errorMessage && (
                      <Tooltip title={run.errorMessage}>
                        <Typography variant="caption" color="error.main">Error</Typography>
                      </Tooltip>
                    )}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      {/* Validation report modal */}
      {selectedValidation && (
        <ValidationModal
          report={selectedValidation}
          open
          onClose={() => setSelectedValidation(null)}
        />
      )}
    </Box>
  );
}
