import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Chip,
  Link,
  Stack,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { useProgram } from '../../contexts/ProgramContext';
import { RoleGatedButton } from '../../components/RoleGatedButton';
import { DataTable, type DataTableColumn } from '../../components/DataTable';
import type { PatientDto } from '../../types';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

const COLUMNS: DataTableColumn<PatientDto>[] = [
  {
    id: 'cffId',
    label: 'CFF ID',
    dataType: 'number',
    minWidth: 90,
    render: (row) => (
      <Link component="span" sx={{ cursor: 'pointer', fontWeight: 600 }}>
        {row.cffId}
      </Link>
    ),
  },
  { id: 'firstName', label: 'First Name' },
  { id: 'lastName', label: 'Last Name' },
  {
    id: 'dateOfBirth',
    label: 'DOB',
    dataType: 'date',
    render: (row) => formatDate(row.dateOfBirth),
  },
  {
    id: 'biologicalSexAtBirth',
    label: 'Sex',
    render: (row) => row.biologicalSexAtBirth ?? row.gender ?? '—',
  },
  {
    id: 'diagnosis',
    label: 'Diagnosis',
    render: (row) => row.diagnosis ?? '—',
  },
  {
    id: 'otherPrograms',
    label: 'Other Programs',
    sortable: false,
    render: (row) =>
      row.otherPrograms && row.otherPrograms.length > 0
        ? row.otherPrograms.join(', ')
        : '—',
  },
  {
    id: 'vitalStatus',
    label: 'Vital Status',
    render: (row) => (
      <Chip
        label={row.vitalStatus}
        size="small"
        color={row.vitalStatus === 'Alive' ? 'success' : 'default'}
        variant="outlined"
      />
    ),
  },
  {
    id: 'lastModifiedDate',
    label: 'Last Modified',
    dataType: 'date',
    render: (row) => (
      <Typography variant="body2" fontSize="0.8rem">
        {row.lastModifiedDate ? formatDate(row.lastModifiedDate) : '—'}
        {row.lastModifiedBy && (
          <Typography component="span" variant="caption" color="text.secondary" display="block">
            by {row.lastModifiedBy}
          </Typography>
        )}
      </Typography>
    ),
  },
];

export function PatientListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { canCreatePatient } = useRoles();
  const { selectedProgram } = useProgram();

  // Server-side state
  const [page, setPage] = useState(() => {
    const p = searchParams.get('page');
    return p ? Math.max(0, parseInt(p, 10) - 1) : 0;
  });
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState(() => searchParams.get('search') ?? '');
  const [sortBy, setSortBy] = useState('lastName');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  // Sync to URL
  const syncParams = useCallback(
    (s: string, p: number) => {
      const params: Record<string, string> = {};
      if (s) params.search = s;
      if (p > 0) params.page = String(p + 1);
      setSearchParams(params, { replace: true });
    },
    [setSearchParams],
  );

  useEffect(() => {
    syncParams(search, page);
  }, [search, page, syncParams]);

  const programId = selectedProgram?.id;

  const { data: patients = [], isLoading } = useQuery({
    queryKey: ['patients', 'roster', { programId, page: page + 1, pageSize, search }],
    queryFn: () =>
      patientsService.getAll({
        careProgramId: programId,
        page: page + 1,
        pageSize,
        searchTerm: search || undefined,
      }),
  });

  const handleRowClick = (row: PatientDto) => {
    navigate(`/patients/${row.id}`);
  };

  const handleSortChange = (newSortBy: string, newDir: 'asc' | 'desc') => {
    setSortBy(newSortBy);
    setSortDirection(newDir);
  };

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Box>
          <Typography variant="h4">Program Roster</Typography>
          {selectedProgram && (
            <Typography variant="body2" color="text.secondary">
              {selectedProgram.name} ({selectedProgram.programId})
            </Typography>
          )}
        </Box>
        <RoleGatedButton
          variant="contained"
          allowed={canCreatePatient}
          disabledReason="Requires ClinicalUser role or above to add patients."
          onClick={() => navigate('/patients/new')}
        >
          Add Patient
        </RoleGatedButton>
      </Stack>

      <DataTable
        columns={COLUMNS}
        rows={patients}
        getRowId={(row) => row.id}
        loading={isLoading}
        emptyMessage="No patients found in this program."
        onRowClick={handleRowClick}
        searchPlaceholder="Search by name, MRN, or CFF ID..."
        serverSide
        page={page}
        pageSize={pageSize}
        search={search}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearchChange={setSearch}
        onSortChange={handleSortChange}
      />
    </Box>
  );
}
