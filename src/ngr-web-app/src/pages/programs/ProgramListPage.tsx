import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
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
import { programsService } from '../../services/programs';
import { useRoles } from '../../hooks/useRoles';
import { RoleGatedButton } from '../../components/RoleGatedButton';
import { US_STATES } from '../../constants';

export function ProgramListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isFoundationAdmin } = useRoles();

  if (!isFoundationAdmin) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        You do not have permission to manage care programs. Requires Foundation Analyst or System Admin role.
      </Alert>
    );
  }

  const [page, setPage] = useState(() => {
    const p = searchParams.get('page');
    return p ? Math.max(0, parseInt(p, 10) - 1) : 0;
  });
  const [pageSize] = useState(50);
  const [name, setName] = useState(() => searchParams.get('name') ?? '');
  const [state, setState] = useState(() => searchParams.get('state') ?? '');
  const [includeInactive, setIncludeInactive] = useState(
    () => searchParams.get('inactive') === 'true',
  );

  const syncSearchParams = useCallback(
    (n: string, st: string, inactive: boolean, p: number) => {
      const params: Record<string, string> = {};
      if (n) params.name = n;
      if (st) params.state = st;
      if (inactive) params.inactive = 'true';
      if (p > 0) params.page = String(p + 1);
      setSearchParams(params, { replace: true });
    },
    [setSearchParams],
  );

  useEffect(() => {
    syncSearchParams(name, state, includeInactive, page);
  }, [name, state, includeInactive, page, syncSearchParams]);

  const { data: programs = [], isLoading, error } = useQuery({
    queryKey: ['programs', { page: page + 1, pageSize, name, state, includeInactive }],
    queryFn: () =>
      programsService.getAll({
        page: page + 1,
        pageSize,
        name: name || undefined,
        state: state || undefined,
        includeInactive,
        includeOrh: true, // Foundation admins can see ORH
      }),
  });

  const handleStateChange = (e: SelectChangeEvent) => {
    setState(e.target.value);
    setPage(0);
  };

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Care Programs</Typography>
        <RoleGatedButton
          variant="contained"
          allowed={isFoundationAdmin}
          disabledReason="Requires Foundation Analyst or System Admin role."
          onClick={() => navigate('/programs/new')}
        >
          Add Program
        </RoleGatedButton>
      </Stack>

      <Stack direction="row" spacing={2} mb={3}>
        <TextField
          label="Search by name"
          size="small"
          value={name}
          onChange={(e) => { setName(e.target.value); setPage(0); }}
          placeholder="Program name..."
          sx={{ width: 280 }}
        />
        <FormControl size="small" sx={{ width: 120 }}>
          <InputLabel>State</InputLabel>
          <Select value={state} label="State" onChange={handleStateChange}>
            <MenuItem value="">All</MenuItem>
            {US_STATES.map((s) => (
              <MenuItem key={s} value={s}>{s}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <Button
          size="small"
          variant={includeInactive ? 'contained' : 'outlined'}
          onClick={() => { setIncludeInactive(!includeInactive); setPage(0); }}
        >
          {includeInactive ? 'Showing Inactive' : 'Show Inactive'}
        </Button>
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
              <TableCell>Program ID</TableCell>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>City</TableCell>
              <TableCell>State</TableCell>
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
            ) : programs.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">No programs found</Typography>
                </TableCell>
              </TableRow>
            ) : (
              programs.map((program) => (
                <TableRow key={program.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {program.programId}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="body2">{program.name}</Typography>
                      {program.isTrainingProgram && (
                        <Chip label="Training" size="small" color="info" variant="outlined" />
                      )}
                      {program.isOrphanHoldingProgram && (
                        <Chip label="ORH" size="small" color="warning" variant="outlined" />
                      )}
                    </Stack>
                  </TableCell>
                  <TableCell>{program.programType}</TableCell>
                  <TableCell>{program.city ?? '—'}</TableCell>
                  <TableCell>{program.state ?? '—'}</TableCell>
                  <TableCell>
                    <Chip
                      label={program.isActive ? 'Active' : 'Inactive'}
                      size="small"
                      color={program.isActive ? 'success' : 'default'}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <RoleGatedButton
                      size="small"
                      allowed={isFoundationAdmin && !program.isOrphanHoldingProgram}
                      disabledReason={
                        program.isOrphanHoldingProgram
                          ? 'The ORH program cannot be edited.'
                          : 'Requires Foundation Analyst or System Admin role.'
                      }
                      onClick={() => navigate(`/programs/${program.id}/edit`)}
                    >
                      Edit
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
