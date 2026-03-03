import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { useProgram } from '../../contexts/ProgramContext';
import { RoleGatedButton } from '../../components/RoleGatedButton';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  });
}

function Field({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <Box mb={2}>
      <Typography variant="caption" color="text.secondary" display="block">
        {label}
      </Typography>
      <Typography variant="body1">{value ?? '—'}</Typography>
    </Box>
  );
}

const REMOVAL_REASONS = [
  'Patient no longer seen within the program',
  'Patient withdrew consent',
  'Consent issue/unable to verify consent',
];

export function PatientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { canEditPatient, canDeactivatePatient } = useRoles();
  const { selectedProgram } = useProgram();

  const [removeDialogOpen, setRemoveDialogOpen] = useState(false);
  const [removalReason, setRemovalReason] = useState('');

  const { data: patient, isLoading, error } = useQuery({
    queryKey: ['patient', id],
    queryFn: () => patientsService.getById(Number(id)),
    enabled: !!id,
  });

  const { data: associations = [] } = useQuery({
    queryKey: ['patient', id, 'programs'],
    queryFn: () => patientsService.getProgramAssociations(Number(id)),
    enabled: !!id,
  });

  const removeMutation = useMutation({
    mutationFn: () =>
      patientsService.removeFromProgram(Number(id), selectedProgram!.id, {
        removalReason,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['patients'] });
      navigate('/patients');
    },
  });

  const handleRemove = () => {
    if (!removalReason) return;
    removeMutation.mutate();
  };

  if (isLoading) {
    return <Box display="flex" justifyContent="center" pt={8}><CircularProgress /></Box>;
  }

  if (error || !patient) {
    return <Alert severity="error">{error ? (error as Error).message : 'Patient not found'}</Alert>;
  }

  const activeAssociations = associations.filter((a) => a.status === 'Active');

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="flex-start" mb={3}>
        <Box>
          <Typography variant="h4">
            {patient.firstName} {patient.middleName ? `${patient.middleName} ` : ''}{patient.lastName}
          </Typography>
          <Stack direction="row" spacing={1} sx={{ mt: 0.5 }}>
            <Chip label={`CFF ID: ${patient.cffId}`} size="small" color="primary" />
            <Chip
              label={patient.vitalStatus}
              size="small"
              color={patient.vitalStatus === 'Alive' ? 'success' : 'default'}
              variant="outlined"
            />
            <Chip
              label={patient.status}
              size="small"
              color={patient.status === 'Active' ? 'success' : 'default'}
              variant="outlined"
            />
          </Stack>
        </Box>
        <Stack direction="row" spacing={1}>
          <RoleGatedButton
            variant="outlined"
            allowed={canEditPatient}
            disabledReason="Requires ClinicalUser role or above to edit patients."
            onClick={() => navigate(`/patients/${id}/edit`)}
          >
            Edit
          </RoleGatedButton>
          <RoleGatedButton
            variant="outlined"
            color="error"
            allowed={canDeactivatePatient}
            disabledReason="Requires ProgramAdmin role or above to remove patients."
            onClick={() => setRemoveDialogOpen(true)}
          >
            Remove from Program
          </RoleGatedButton>
          <Button variant="text" onClick={() => navigate('/patients')}>Back</Button>
        </Stack>
      </Stack>

      <Grid container spacing={3}>
        {/* Patient Identity */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Patient Identity" titleTypographyProps={{ variant: 'h6' }} />
            <Divider />
            <CardContent>
              <Field label="Registry ID" value={patient.registryId} />
              <Field label="CFF ID" value={String(patient.cffId)} />
              <Field label="First Name" value={patient.firstName} />
              <Field label="Middle Name" value={patient.middleName} />
              <Field label="Last Name" value={patient.lastName} />
              <Field label="Last Name at Birth" value={patient.lastNameAtBirth} />
              <Field label="Date of Birth" value={formatDate(patient.dateOfBirth)} />
              <Field label="Biological Sex at Birth" value={patient.biologicalSexAtBirth} />
              <Field label="MRN" value={patient.medicalRecordNumber} />
            </CardContent>
          </Card>
        </Grid>

        {/* Clinical Info */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Clinical Information" titleTypographyProps={{ variant: 'h6' }} />
            <Divider />
            <CardContent>
              <Field label="Diagnosis" value={patient.diagnosis} />
              <Field label="Vital Status" value={patient.vitalStatus} />
              <Field label="Email" value={patient.email} />
              <Field label="Phone" value={patient.phone} />
            </CardContent>
          </Card>
        </Grid>

        {/* Program Associations */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Program Associations" titleTypographyProps={{ variant: 'h6' }} />
            <Divider />
            <CardContent>
              {activeAssociations.length === 0 ? (
                <Typography color="text.secondary">No active program associations.</Typography>
              ) : (
                activeAssociations.map((a) => (
                  <Box key={a.id} sx={{ mb: 1.5, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography variant="body1">{a.programName}</Typography>
                    {a.isPrimaryProgram && <Chip label="Primary" size="small" color="primary" />}
                    {a.localMRN && (
                      <Typography variant="caption" color="text.secondary">
                        MRN: {a.localMRN}
                      </Typography>
                    )}
                  </Box>
                ))
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Record Metadata */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader title="Record Metadata" titleTypographyProps={{ variant: 'h6' }} />
            <Divider />
            <CardContent>
              <Field label="Patient ID" value={String(patient.id)} />
              <Field label="Created" value={formatDate(patient.createdAt)} />
              <Field label="Last Modified" value={patient.lastModifiedDate ? formatDate(patient.lastModifiedDate) : '—'} />
              <Field label="Last Modified By" value={patient.lastModifiedBy} />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Remove from Program Dialog (04-007) ────────────────── */}
      <Dialog
        open={removeDialogOpen}
        onClose={() => setRemoveDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Remove Patient from Program</DialogTitle>
        <Divider />
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Remove <strong>{patient.firstName} {patient.lastName}</strong> from{' '}
            <strong>{selectedProgram?.name}</strong>? This action cannot be undone.
          </Typography>

          {removalReason === 'Patient withdrew consent' && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              Consent withdrawal will remove this patient from <strong>ALL</strong> clinical
              programs and send a notification to reghelp@cff.org.
            </Alert>
          )}

          <FormControl fullWidth size="small" required>
            <InputLabel>Removal Reason</InputLabel>
            <Select
              value={removalReason}
              label="Removal Reason"
              onChange={(e) => setRemovalReason(e.target.value)}
            >
              {REMOVAL_REASONS.map((r) => (
                <MenuItem key={r} value={r}>{r}</MenuItem>
              ))}
            </Select>
          </FormControl>

          {removeMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {(removeMutation.error as Error)?.message || 'Failed to remove patient.'}
            </Alert>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setRemoveDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleRemove}
            disabled={!removalReason || removeMutation.isPending}
          >
            {removeMutation.isPending ? <CircularProgress size={20} /> : 'Confirm Removal'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
