import { useCallback, useState } from 'react';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Checkbox,
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
  IconButton,
  LinearProgress,
  MenuItem,
  Paper,
  Radio,
  RadioGroup,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import LockIcon from '@mui/icons-material/Lock';
import LockOpenIcon from '@mui/icons-material/LockOpen';
import CloseIcon from '@mui/icons-material/Close';
import { useRoles } from '../../hooks/useRoles';
import {
  databaseLockService,
  type CreateDatabaseLockDto,
  type DatabaseLockDto,
} from '../../services/databaseLock';

// ── Helpers ───────────────────────────────────────────────────────────────────

function currentYear() { return new Date().getFullYear(); }
function prevYear()    { return currentYear() - 1; }

function availableYears(): number[] {
  const years: number[] = [];
  for (let y = prevYear(); y >= 2015; y--) years.push(y);
  return years;
}

function StatusChip({ status }: { status: string }) {
  const map: Record<string, 'success' | 'error' | 'warning' | 'info' | 'default'> = {
    Completed: 'success', Failed: 'error', InProgress: 'info', Pending: 'warning',
  };
  return (
    <Chip
      label={status}
      color={map[status] ?? 'default'}
      size="small"
      variant="outlined"
    />
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function DatabaseLockPage() {
  const { isFoundationAdmin } = useRoles();
  const queryClient = useQueryClient();

  const [reportingYear, setReportingYear] = useState<number>(prevYear());
  const [lockDate,      setLockDate]      = useState('');
  const [execMode,      setExecMode]      = useState<'Synchronous' | 'Batch'>('Synchronous');
  const [scheduledDate, setScheduledDate] = useState('');
  const [confirmed,     setConfirmed]     = useState(false);
  const [confirmOpen,   setConfirmOpen]   = useState(false);

  // ── Data ──────────────────────────────────────────────────────────────────

  const { data: locks = [], isLoading: locksLoading } = useQuery<DatabaseLockDto[]>({
    queryKey: ['database-locks'],
    queryFn:  () => databaseLockService.getLocks(),
    refetchInterval: (query) => {
      const data = query.state.data as DatabaseLockDto[] | undefined;
      return data?.some(l => l.status === 'InProgress') ? 5000 : false;
    },
  });

  const { data: impact, isLoading: impactLoading } = useQuery({
    queryKey: ['database-lock-impact', reportingYear],
    queryFn:  () => databaseLockService.getImpact(reportingYear),
  });

  const activeLock = locks.find(l => l.status === 'InProgress');

  const createMutation = useMutation({
    mutationFn: (dto: CreateDatabaseLockDto) => databaseLockService.createLock(dto),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['database-locks'] });
      void queryClient.invalidateQueries({ queryKey: ['database-lock-impact', reportingYear] });
      setConfirmOpen(false);
      setConfirmed(false);
    },
  });

  // ── Validation ────────────────────────────────────────────────────────────

  const lockDateValid = lockDate && new Date(lockDate) > new Date();
  const batchDateValid = execMode !== 'Batch' || Boolean(scheduledDate);
  const canSubmit = lockDateValid && batchDateValid && !activeLock && !impact?.isAlreadyLocked;

  const handleOpenConfirm = () => {
    setConfirmed(false);
    setConfirmOpen(true);
  };

  const handleExecute = useCallback(() => {
    const dto: CreateDatabaseLockDto = {
      reportingYear,
      lockDate: new Date(lockDate).toISOString(),
      executionMode: execMode,
      scheduledDate: execMode === 'Batch' ? new Date(scheduledDate).toISOString() : undefined,
    };
    createMutation.mutate(dto);
  }, [reportingYear, lockDate, execMode, scheduledDate, createMutation]);

  // ── Guard ─────────────────────────────────────────────────────────────────

  if (!isFoundationAdmin) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        Access denied. This page requires the FoundationAnalyst role or higher.
      </Alert>
    );
  }

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Database Lock</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Lock all form submissions for a completed reporting year, making them read-only.
        This action is permanent and requires explicit confirmation.
      </Typography>

      {/* ── Configuration form ────────────────────────────────────────── */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>Configure Lock</Typography>

        <Stack spacing={3}>
          {/* Reporting Year */}
          <FormControl size="small" sx={{ maxWidth: 200 }}>
            <FormLabel sx={{ mb: 0.5 }}>Reporting Year</FormLabel>
            <Select
              value={reportingYear}
              onChange={(e) => setReportingYear(Number(e.target.value))}
            >
              {availableYears().map((y) => (
                <MenuItem key={y} value={y}>{y}</MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Impact summary */}
          {impactLoading ? (
            <CircularProgress size={20} />
          ) : impact ? (
            <Alert
              severity={impact.isAlreadyLocked ? 'success' : 'info'}
              icon={impact.isAlreadyLocked ? <LockIcon /> : <LockOpenIcon />}
            >
              {impact.isAlreadyLocked ? (
                `Reporting year ${reportingYear} is already locked (${impact.alreadyLocked.toLocaleString()} forms).`
              ) : (
                <>
                  <strong>{impact.wouldLock.toLocaleString()}</strong> forms would be locked
                  ({impact.alreadyLocked.toLocaleString()} already locked,{' '}
                  {impact.eligibleForms.toLocaleString()} total eligible).
                </>
              )}
            </Alert>
          ) : null}

          {/* Lock Date */}
          <FormControl sx={{ maxWidth: 300 }}>
            <FormLabel sx={{ mb: 0.5 }}>Lock Date</FormLabel>
            <TextField
              type="date"
              size="small"
              value={lockDate}
              onChange={(e) => setLockDate(e.target.value)}
              inputProps={{ min: new Date().toISOString().split('T')[0] }}
              helperText="Must be a future date"
            />
          </FormControl>

          {/* Execution Mode */}
          <FormControl>
            <FormLabel>Execution Mode</FormLabel>
            <RadioGroup
              value={execMode}
              onChange={(e) => setExecMode(e.target.value as 'Synchronous' | 'Batch')}
            >
              <FormControlLabel
                value="Synchronous"
                control={<Radio />}
                label={
                  <span>
                    <strong>Fast Synchronous</strong>
                    <Typography variant="caption" color="text.secondary" display="block">
                      Immediate execution (&lt;2 minutes). Suitable for normal registry sizes.
                    </Typography>
                  </span>
                }
              />
              <FormControlLabel
                value="Batch"
                control={<Radio />}
                label={
                  <span>
                    <strong>Overnight Batch</strong>
                    <Typography variant="caption" color="text.secondary" display="block">
                      Scheduled to run during the 2–6 AM ET window. For very large datasets.
                    </Typography>
                  </span>
                }
              />
            </RadioGroup>
          </FormControl>

          {/* Scheduled Date (Batch only) */}
          {execMode === 'Batch' && (
            <FormControl sx={{ maxWidth: 300 }}>
              <FormLabel sx={{ mb: 0.5 }}>Scheduled Execution Night</FormLabel>
              <TextField
                type="date"
                size="small"
                value={scheduledDate}
                onChange={(e) => setScheduledDate(e.target.value)}
                inputProps={{ min: new Date().toISOString().split('T')[0] }}
                helperText="Lock will execute between 2–6 AM ET on this night"
              />
            </FormControl>
          )}

          {/* Active lock warning */}
          {activeLock && (
            <Alert severity="warning">
              A lock operation is currently in progress for {activeLock.reportingYear}.
              Please wait for it to complete before initiating a new lock.
            </Alert>
          )}

          <Tooltip
            title={
              activeLock ? 'A lock is already in progress' :
              impact?.isAlreadyLocked ? 'This year is already locked' :
              !lockDateValid ? 'Lock date must be a future date' :
              ''
            }
          >
            <span>
              <Button
                variant="contained"
                color="error"
                startIcon={<LockIcon />}
                disabled={!canSubmit}
                onClick={handleOpenConfirm}
              >
                Initiate Lock
              </Button>
            </span>
          </Tooltip>
        </Stack>
      </Paper>

      {/* ── In-progress lock ──────────────────────────────────────────── */}
      {activeLock && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>Lock In Progress — {activeLock.reportingYear}</Typography>
          <Stack spacing={1}>
            <Typography variant="body2">
              {activeLock.progressFormsProcessed.toLocaleString()} /{' '}
              {activeLock.progressFormsTotal?.toLocaleString() ?? '?'} forms processed
            </Typography>
            <LinearProgress
              variant={activeLock.progressFormsTotal ? 'determinate' : 'indeterminate'}
              value={
                activeLock.progressFormsTotal
                  ? (activeLock.progressFormsProcessed / activeLock.progressFormsTotal) * 100
                  : undefined
              }
            />
            <Typography variant="caption" color="text.secondary">
              Initiated by {activeLock.initiatedBy} at {new Date(activeLock.initiatedAt).toLocaleString()}
            </Typography>
          </Stack>
        </Paper>
      )}

      {/* ── Lock history ──────────────────────────────────────────────── */}
      <Typography variant="h6" gutterBottom>Lock History</Typography>
      {locksLoading ? (
        <CircularProgress size={24} />
      ) : locks.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">No lock operations have been performed.</Typography>
        </Paper>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Year</TableCell>
                <TableCell>Lock Date</TableCell>
                <TableCell>Mode</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Locked</TableCell>
                <TableCell align="right">Skipped</TableCell>
                <TableCell>Initiated By</TableCell>
                <TableCell>Completed</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {locks.map((l) => (
                <TableRow key={l.id} hover>
                  <TableCell>{l.reportingYear}</TableCell>
                  <TableCell>{new Date(l.lockDate).toLocaleDateString()}</TableCell>
                  <TableCell>{l.executionMode}</TableCell>
                  <TableCell><StatusChip status={l.status} /></TableCell>
                  <TableCell align="right">{l.formsLocked.toLocaleString()}</TableCell>
                  <TableCell align="right">{l.formsSkipped.toLocaleString()}</TableCell>
                  <TableCell>{l.initiatedBy}</TableCell>
                  <TableCell>
                    {l.completedAt ? new Date(l.completedAt).toLocaleString() : '—'}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* ── Confirmation Dialog ────────────────────────────────────────── */}
      <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          Confirm Database Lock
          <IconButton onClick={() => setConfirmOpen(false)} size="small" aria-label="Close">
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          <Stack spacing={2}>
            <Alert severity="warning">
              This action will permanently lock all form submissions for the selected reporting year.
              Locked forms become read-only and cannot be edited without unlocking.
            </Alert>
            <Divider />
            <Stack spacing={0.5}>
              <Typography variant="body2"><strong>Reporting Year:</strong> {reportingYear}</Typography>
              <Typography variant="body2">
                <strong>Lock Date:</strong> {lockDate ? new Date(lockDate).toLocaleDateString() : '—'}
              </Typography>
              <Typography variant="body2"><strong>Execution Mode:</strong> {execMode}</Typography>
              {execMode === 'Batch' && scheduledDate && (
                <Typography variant="body2">
                  <strong>Scheduled:</strong> {new Date(scheduledDate).toLocaleDateString()} (2–6 AM ET)
                </Typography>
              )}
              <Typography variant="body2">
                <strong>Forms to be locked:</strong> {impact?.wouldLock.toLocaleString() ?? '—'}
              </Typography>
            </Stack>

            <FormControlLabel
              control={
                <Checkbox
                  checked={confirmed}
                  onChange={(e) => setConfirmed(e.target.checked)}
                  color="error"
                />
              }
              label={`I understand this action will lock ${impact?.wouldLock.toLocaleString() ?? 'N'} forms for reporting year ${reportingYear}.`}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            disabled={!confirmed || createMutation.isPending}
            onClick={handleExecute}
            startIcon={createMutation.isPending ? <CircularProgress size={16} color="inherit" /> : <LockIcon />}
          >
            {createMutation.isPending ? 'Executing…' : 'Confirm & Execute'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Error/success feedback */}
      {createMutation.isError && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {(createMutation.error as Error).message}
        </Alert>
      )}
      {createMutation.isSuccess && (
        <Alert severity="success" sx={{ mt: 2 }}>
          {createMutation.data.status === 'Completed'
            ? `Lock completed: ${createMutation.data.formsLocked.toLocaleString()} forms locked.`
            : `Lock scheduled for ${reportingYear} (${createMutation.data.status}).`}
        </Alert>
      )}
    </Box>
  );
}
