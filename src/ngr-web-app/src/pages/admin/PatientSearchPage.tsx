import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  InputLabel,
  Link,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { DataTable, type DataTableColumn } from '../../components/DataTable';
import type { PatientDto } from '../../types';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

const BULK_ACTIONS = [
  { value: 'add_to_program', label: 'Add to program' },
  { value: 'transfer', label: 'Transfer between programs' },
  { value: 'remove_from_program', label: 'Remove from specific program' },
  { value: 'remove_all_inactivity', label: 'Remove from all (inactivity) → ORH' },
  { value: 'remove_all_consent', label: 'Remove from all (consent issue) → ORH' },
  { value: 'remove_all_withdrawal', label: 'Remove from all (consent withdrawal) → ORH' },
];

const COLUMNS: DataTableColumn<PatientDto>[] = [
  {
    id: 'cffId',
    label: 'CFF ID',
    dataType: 'number',
    render: (row) => <Link component="span" sx={{ cursor: 'pointer', fontWeight: 600 }}>{row.cffId}</Link>,
  },
  { id: 'firstName', label: 'First Name' },
  { id: 'middleName', label: 'Middle', render: (row) => row.middleName ?? '—' },
  { id: 'lastName', label: 'Last Name' },
  { id: 'dateOfBirth', label: 'DOB', dataType: 'date', render: (row) => formatDate(row.dateOfBirth) },
  { id: 'biologicalSexAtBirth', label: 'Sex', render: (row) => row.biologicalSexAtBirth ?? row.gender ?? '—' },
  {
    id: 'otherPrograms',
    label: 'Programs',
    sortable: false,
    render: (row) => {
      const all = [row.careProgramName, ...(row.otherPrograms || [])].filter(Boolean);
      return all.join(', ') || '—';
    },
  },
  {
    id: 'vitalStatus',
    label: 'Vital Status',
    render: (row) => <Chip label={row.vitalStatus} size="small" color={row.vitalStatus === 'Alive' ? 'success' : 'default'} variant="outlined" />,
  },
  {
    id: 'lastModifiedDate',
    label: 'Last Modified',
    dataType: 'date',
    render: (row) => (
      <Typography variant="body2" fontSize="0.8rem">
        {row.lastModifiedDate ? formatDate(row.lastModifiedDate) : '—'}
        {row.lastModifiedBy && <Typography component="span" variant="caption" color="text.secondary" display="block">by {row.lastModifiedBy}</Typography>}
      </Typography>
    ),
  },
];

export function PatientSearchPage() {
  const navigate = useNavigate();
  const { isFoundationAdmin } = useRoles();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [selectedIds, setSelectedIds] = useState<Set<number>>(new Set());
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkAction, setBulkAction] = useState('');
  const [targetProgramId, setTargetProgramId] = useState('');
  const [sourceProgramId, setSourceProgramId] = useState('');
  const [bulkReason, setBulkReason] = useState('');

  const { data: patients = [], isLoading } = useQuery({
    queryKey: ['patients', 'admin-search', { search, page: page + 1, pageSize }],
    queryFn: () => patientsService.getAll({ searchTerm: search || undefined, page: page + 1, pageSize }),
    enabled: isFoundationAdmin,
  });

  const bulkMutation = useMutation({
    mutationFn: () =>
      patientsService.bulkModifyAssociations({
        patientIds: Array.from(selectedIds),
        action: bulkAction,
        targetProgramId: targetProgramId ? Number(targetProgramId) : undefined,
        sourceProgramId: sourceProgramId ? Number(sourceProgramId) : undefined,
        reason: bulkReason || undefined,
      }),
    onSuccess: () => {
      setBulkDialogOpen(false);
      setSelectedIds(new Set());
    },
  });

  if (!isFoundationAdmin) {
    return <Alert severity="error">Patient Search is available only to Foundation Administrators.</Alert>;
  }

  const toggleSelect = (id: number) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const needsTarget = ['add_to_program', 'transfer'].includes(bulkAction);
  const needsSource = ['transfer', 'remove_from_program'].includes(bulkAction);

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Patient Search</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Search across all patients in the Registry regardless of program membership.
      </Typography>

      <DataTable
        columns={COLUMNS}
        rows={patients}
        getRowId={(row) => row.id}
        loading={isLoading}
        emptyMessage="No patients found."
        searchPlaceholder="Search by name, CFF ID, Registry ID..."
        onRowClick={(row) => navigate(`/patients/${row.id}`)}
        serverSide
        page={page}
        pageSize={pageSize}
        search={search}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearchChange={setSearch}
        onSortChange={() => {}}
        renderActions={(row) => (
          <Checkbox
            checked={selectedIds.has(row.id)}
            onChange={() => toggleSelect(row.id)}
            onClick={(e) => e.stopPropagation()}
            size="small"
          />
        )}
      />

      {selectedIds.size > 0 && (
        <Box sx={{ mt: 2, p: 2, bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider', borderRadius: 1 }}>
          <Stack direction="row" alignItems="center" spacing={2}>
            <Typography variant="body2">{selectedIds.size} patient(s) selected</Typography>
            <Button variant="contained" size="small" onClick={() => setBulkDialogOpen(true)}>
              Modify Associations
            </Button>
            <Button size="small" onClick={() => setSelectedIds(new Set())}>Clear Selection</Button>
          </Stack>
        </Box>
      )}

      {/* ── Bulk Modification Dialog (05-004) ─────────────────── */}
      <Dialog open={bulkDialogOpen} onClose={() => setBulkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Modify Program Associations</DialogTitle>
        <Divider />
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            {selectedIds.size} patient(s) selected. Choose an action:
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Action</InputLabel>
              <Select value={bulkAction} label="Action" onChange={(e) => setBulkAction(e.target.value)}>
                {BULK_ACTIONS.map((a) => <MenuItem key={a.value} value={a.value}>{a.label}</MenuItem>)}
              </Select>
            </FormControl>

            {needsTarget && (
              <TextField label="Target Program ID" value={targetProgramId} onChange={(e) => setTargetProgramId(e.target.value)} size="small" type="number" />
            )}
            {needsSource && (
              <TextField label="Source Program ID" value={sourceProgramId} onChange={(e) => setSourceProgramId(e.target.value)} size="small" type="number" />
            )}

            <TextField
              label="Reason (free text)"
              value={bulkReason}
              onChange={(e) => setBulkReason(e.target.value)}
              multiline
              rows={2}
              size="small"
            />
          </Box>

          {bulkAction && (
            <Alert severity="info" sx={{ mt: 2 }}>
              <strong>Confirmation:</strong> {selectedIds.size} patients shall be affected by action "{BULK_ACTIONS.find((a) => a.value === bulkAction)?.label}". Are you sure?
            </Alert>
          )}

          {bulkMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {(bulkMutation.error as Error)?.message || 'Operation failed.'}
            </Alert>
          )}
          {bulkMutation.isSuccess && (
            <Alert severity="success" sx={{ mt: 2 }}>
              Operation completed successfully.
            </Alert>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setBulkDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => bulkMutation.mutate()}
            disabled={!bulkAction || bulkMutation.isPending}
          >
            Apply Changes
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
