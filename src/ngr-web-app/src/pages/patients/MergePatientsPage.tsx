import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Checkbox,
  Chip,
  CircularProgress,
  Divider,
  FormControlLabel,
  Grid,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import MergeIcon from '@mui/icons-material/MergeType';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { useProgram } from '../../contexts/ProgramContext';
import type { PatientDto, MergeResultDto } from '../../types';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function PatientCard({
  patient,
  label,
  selected,
  onSelect,
}: {
  patient: PatientDto;
  label: string;
  selected: boolean;
  onSelect: () => void;
}) {
  return (
    <Card
      sx={{
        border: selected ? '2px solid' : '1px solid',
        borderColor: selected ? 'primary.main' : 'divider',
        cursor: 'pointer',
      }}
      onClick={onSelect}
    >
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="subtitle2" color="primary">{label}</Typography>
          {selected && <Chip label="Selected" size="small" color="primary" />}
        </Stack>
        <Typography variant="h6" sx={{ mt: 1 }}>
          {patient.firstName} {patient.lastName}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          CFF ID: {patient.cffId} | DOB: {formatDate(patient.dateOfBirth)}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Sex: {patient.biologicalSexAtBirth ?? patient.gender ?? '—'} | Status: {patient.vitalStatus}
        </Typography>
        {patient.otherPrograms.length > 0 && (
          <Typography variant="caption" color="text.secondary">
            Programs: {patient.otherPrograms.join(', ')}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}

export function MergePatientsPage() {
  const navigate = useNavigate();
  const { canDeactivatePatient, isFoundationAdmin } = useRoles();
  const { selectedProgram } = useProgram();

  const [searchTerm, setSearchTerm] = useState('');
  const [primaryId, setPrimaryId] = useState<number | null>(null);
  const [secondaryId, setSecondaryId] = useState<number | null>(null);
  const [confirmed, setConfirmed] = useState(false);
  const [mergeResult, setMergeResult] = useState<MergeResultDto | null>(null);

  // Search for patients to merge
  const { data: searchResults = [], isLoading: searching } = useQuery({
    queryKey: ['patients', 'merge-search', searchTerm, selectedProgram?.id],
    queryFn: () =>
      patientsService.getAll({
        searchTerm,
        careProgramId: isFoundationAdmin ? undefined : selectedProgram?.id,
        pageSize: 20,
      }),
    enabled: searchTerm.length >= 2,
  });

  // Load selected patients for side-by-side view
  const { data: primaryPatient } = useQuery({
    queryKey: ['patient', primaryId],
    queryFn: () => patientsService.getById(primaryId!),
    enabled: primaryId !== null,
  });

  const { data: secondaryPatient } = useQuery({
    queryKey: ['patient', secondaryId],
    queryFn: () => patientsService.getById(secondaryId!),
    enabled: secondaryId !== null,
  });

  const mergeMutation = useMutation({
    mutationFn: () =>
      patientsService.merge({
        primaryPatientId: primaryId!,
        secondaryPatientId: secondaryId!,
      }),
    onSuccess: (result) => setMergeResult(result),
  });

  // Multi-program patients can only be merged by Foundation Admins
  const multiProgramBlocked = !isFoundationAdmin && secondaryPatient &&
    secondaryPatient.otherPrograms.length > 0;

  if (!canDeactivatePatient) {
    return (
      <Alert severity="error">
        Requires ProgramAdmin role or above to merge patient records.
      </Alert>
    );
  }

  if (mergeResult) {
    return (
      <Box>
        <Alert severity="success" sx={{ mb: 3 }}>
          <Typography variant="subtitle2" gutterBottom>Merge completed successfully!</Typography>
          <Typography variant="body2">
            Primary patient #{mergeResult.primaryPatientId} retained. Secondary patient #{mergeResult.secondaryPatientId} merged.
            {mergeResult.aliasesCreated > 0 && ` ${mergeResult.aliasesCreated} aliases created.`}
            {mergeResult.associationsMerged > 0 && ` ${mergeResult.associationsMerged} program associations merged.`}
          </Typography>
        </Alert>
        <Button variant="contained" onClick={() => navigate(`/patients/${mergeResult.primaryPatientId}`)}>
          View Primary Patient
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Merge Duplicate Records</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        {isFoundationAdmin
          ? 'Search across all patients in the Registry to find and merge duplicates.'
          : `Search within ${selectedProgram?.name ?? 'your program'} and ORH to find duplicates.`}
      </Typography>

      {/* ── Search ────────────────────────────────────────────── */}
      <TextField
        label="Search by CFF ID, first name, or last name"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        fullWidth
        size="small"
        sx={{ mb: 3, maxWidth: 500 }}
      />

      {searching && <CircularProgress size={24} sx={{ mb: 2 }} />}

      {/* ── Search Results ────────────────────────────────────── */}
      {searchResults.length > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" gutterBottom>
            Select two patients to merge:
          </Typography>
          <Grid container spacing={2}>
            {searchResults
              .filter((p) => p.vitalStatus !== 'Deceased' && p.status !== 'Merged')
              .map((patient) => (
                <Grid item xs={12} sm={6} md={4} key={patient.id}>
                  <PatientCard
                    patient={patient}
                    label={
                      primaryId === patient.id
                        ? 'Primary Record'
                        : secondaryId === patient.id
                          ? 'Secondary Record'
                          : 'Click to select'
                    }
                    selected={primaryId === patient.id || secondaryId === patient.id}
                    onSelect={() => {
                      if (primaryId === patient.id) {
                        setPrimaryId(null);
                      } else if (secondaryId === patient.id) {
                        setSecondaryId(null);
                      } else if (!primaryId) {
                        setPrimaryId(patient.id);
                      } else if (!secondaryId) {
                        setSecondaryId(patient.id);
                      }
                    }}
                  />
                </Grid>
              ))}
          </Grid>
        </Box>
      )}

      {/* ── Side-by-Side Comparison ───────────────────────────── */}
      {primaryPatient && secondaryPatient && (
        <Box>
          <Typography variant="h6" gutterBottom>
            Side-by-Side Comparison
          </Typography>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card sx={{ border: '2px solid', borderColor: 'primary.main' }}>
                <CardHeader title="Primary Record (Retained)" titleTypographyProps={{ variant: 'subtitle1', color: 'primary' }} />
                <Divider />
                <CardContent>
                  <Typography variant="body2"><strong>CFF ID:</strong> {primaryPatient.cffId}</Typography>
                  <Typography variant="body2"><strong>Name:</strong> {primaryPatient.firstName} {primaryPatient.lastName}</Typography>
                  <Typography variant="body2"><strong>DOB:</strong> {formatDate(primaryPatient.dateOfBirth)}</Typography>
                  <Typography variant="body2"><strong>Sex:</strong> {primaryPatient.biologicalSexAtBirth ?? '—'}</Typography>
                  <Typography variant="body2"><strong>Diagnosis:</strong> {primaryPatient.diagnosis ?? '—'}</Typography>
                  <Typography variant="body2"><strong>Vital Status:</strong> {primaryPatient.vitalStatus}</Typography>
                  <Typography variant="body2"><strong>Programs:</strong> {primaryPatient.careProgramName}{primaryPatient.otherPrograms.length > 0 ? `, ${primaryPatient.otherPrograms.join(', ')}` : ''}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} md={6}>
              <Card sx={{ border: '2px solid', borderColor: 'error.main' }}>
                <CardHeader title="Secondary Record (Merged In)" titleTypographyProps={{ variant: 'subtitle1', color: 'error' }} />
                <Divider />
                <CardContent>
                  <Typography variant="body2"><strong>CFF ID:</strong> {secondaryPatient.cffId}</Typography>
                  <Typography variant="body2"><strong>Name:</strong> {secondaryPatient.firstName} {secondaryPatient.lastName}</Typography>
                  <Typography variant="body2"><strong>DOB:</strong> {formatDate(secondaryPatient.dateOfBirth)}</Typography>
                  <Typography variant="body2"><strong>Sex:</strong> {secondaryPatient.biologicalSexAtBirth ?? '—'}</Typography>
                  <Typography variant="body2"><strong>Diagnosis:</strong> {secondaryPatient.diagnosis ?? '—'}</Typography>
                  <Typography variant="body2"><strong>Vital Status:</strong> {secondaryPatient.vitalStatus}</Typography>
                  <Typography variant="body2"><strong>Programs:</strong> {secondaryPatient.careProgramName}{secondaryPatient.otherPrograms.length > 0 ? `, ${secondaryPatient.otherPrograms.join(', ')}` : ''}</Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {multiProgramBlocked && (
            <Alert severity="error" sx={{ mt: 2 }}>
              The secondary patient is associated with multiple care programs. Only Foundation Administrators can merge multi-program patients.
            </Alert>
          )}

          {!multiProgramBlocked && (
            <Box sx={{ mt: 3 }}>
              <FormControlLabel
                control={<Checkbox checked={confirmed} onChange={(e) => setConfirmed(e.target.checked)} />}
                label="I have reviewed both records and confirm they are duplicates that should be merged."
              />

              {mergeMutation.isError && (
                <Alert severity="error" sx={{ mt: 1 }}>
                  {(mergeMutation.error as Error)?.message || 'Merge failed.'}
                </Alert>
              )}

              <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
                <Button
                  variant="contained"
                  startIcon={mergeMutation.isPending ? <CircularProgress size={16} /> : <MergeIcon />}
                  onClick={() => mergeMutation.mutate()}
                  disabled={!confirmed || mergeMutation.isPending}
                >
                  Merge Records
                </Button>
                <Button
                  variant="outlined"
                  onClick={() => {
                    setPrimaryId(null);
                    setSecondaryId(null);
                    setConfirmed(false);
                  }}
                >
                  Cancel
                </Button>
              </Stack>
            </Box>
          )}
        </Box>
      )}
    </Box>
  );
}
