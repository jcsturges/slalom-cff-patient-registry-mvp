import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  type SelectChangeEvent,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { RoleGatedButton } from '../../components/RoleGatedButton';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

export function PatientListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const { canCreatePatient, canEditPatient, canDeactivatePatient } = useRoles();

  // Initialize from URL query params so state survives re-login redirects
  const [page, setPage] = useState(() => {
    const p = searchParams.get('page');
    return p ? Math.max(0, parseInt(p, 10) - 1) : 0;
  });
  const [pageSize] = useState(50);
  const [search, setSearch] = useState(() => searchParams.get('search') ?? '');
  const [status, setStatus] = useState(() => searchParams.get('status') ?? '');

  // Sync filters back to URL
  const syncSearchParams = useCallback(
    (s: string, st: string, p: number) => {
      const params: Record<string, string> = {};
      if (s) params.search = s;
      if (st) params.status = st;
      if (p > 0) params.page = String(p + 1); // URL uses 1-indexed
      setSearchParams(params, { replace: true });
    },
    [setSearchParams],
  );

  useEffect(() => {
    syncSearchParams(search, status, page);
  }, [search, status, page, syncSearchParams]);

  const { data: patients = [], isLoading, error } = useQuery({
    queryKey: ['patients', { page: page + 1, pageSize, search, status }],
    queryFn: () =>
      patientsService.getAll({
        page: page + 1,
        pageSize,
        searchTerm: search || undefined,
        status: status || undefined,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => patientsService.delete(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['patients'] }),
  });

  const handleStatusChange = (e: SelectChangeEvent) => {
    setStatus(e.target.value);
    setPage(0);
  };

  const handleDeactivate = (e: React.MouseEvent, id: number) => {
    e.stopPropagation();
    if (window.confirm('Deactivate this patient? They will be marked as Inactive.')) {
      deleteMutation.mutate(id);
    }
  };

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Patient Roster</Typography>
        <RoleGatedButton
          variant="contained"
          allowed={canCreatePatient}
          disabledReason="Requires ClinicalUser role or above to add patients."
          onClick={() => navigate('/patients/new')}
        >
          Add Patient
        </RoleGatedButton>
      </Stack>

      <Stack direction="row" spacing={2} mb={3}>
        <TextField
          label="Search"
          size="small"
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(0); }}
          placeholder="Name or MRN..."
          sx={{ width: 280 }}
        />
        <FormControl size="small" sx={{ width: 160 }}>
          <InputLabel>Status</InputLabel>
          <Select value={status} label="Status" onChange={handleStatusChange}>
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Active">Active</MenuItem>
            <MenuItem value="Inactive">Inactive</MenuItem>
          </Select>
        </FormControl>
      </Stack>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(error as Error).message}
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Date of Birth</TableCell>
              <TableCell>MRN</TableCell>
              <TableCell>Gender</TableCell>
              <TableCell>Care Program</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <CircularProgress size={28} />
                </TableCell>
              </TableRow>
            ) : patients.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">No patients found</Typography>
                </TableCell>
              </TableRow>
            ) : (
              patients.map((patient) => (
                <TableRow
                  key={patient.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/patients/${patient.id}`)}
                >
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {patient.lastName}, {patient.firstName}
                    </Typography>
                  </TableCell>
                  <TableCell>{formatDate(patient.dateOfBirth)}</TableCell>
                  <TableCell>{patient.medicalRecordNumber ?? '—'}</TableCell>
                  <TableCell>{patient.gender ?? '—'}</TableCell>
                  <TableCell>{patient.careProgramName}</TableCell>
                  <TableCell>
                    <Chip
                      label={patient.status}
                      size="small"
                      color={patient.status === 'Active' ? 'success' : 'default'}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <RoleGatedButton
                      size="small"
                      allowed={canEditPatient}
                      disabledReason="Requires ClinicalUser role or above to edit patients."
                      onClick={(e) => {
                        e.stopPropagation();
                        navigate(`/patients/${patient.id}/edit`);
                      }}
                    >
                      Edit
                    </RoleGatedButton>
                    <RoleGatedButton
                      size="small"
                      color="error"
                      allowed={canDeactivatePatient}
                      disabledReason="Requires ProgramAdmin role or above to deactivate patients."
                      disabled={deleteMutation.isPending}
                      onClick={(e) => handleDeactivate(e, patient.id)}
                    >
                      Deactivate
                    </RoleGatedButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={-1}
        page={page}
        rowsPerPage={pageSize}
        rowsPerPageOptions={[50]}
        onPageChange={(_, newPage) => setPage(newPage)}
        labelDisplayedRows={({ from, to }) => `${from}–${to}`}
      />
    </Box>
  );
}
