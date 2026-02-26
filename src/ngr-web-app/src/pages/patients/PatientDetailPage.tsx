import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Breadcrumbs,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  CircularProgress,
  Divider,
  Grid,
  Link,
  Stack,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
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

export function PatientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { canEditPatient, canDeactivatePatient } = useRoles();

  const { data: patient, isLoading, error } = useQuery({
    queryKey: ['patient', id],
    queryFn: () => patientsService.getById(Number(id)),
    enabled: !!id,
  });

  const deleteMutation = useMutation({
    mutationFn: () => patientsService.delete(Number(id)),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['patients'] });
      navigate('/patients');
    },
  });

  const handleDeactivate = () => {
    if (!patient) return;
    if (window.confirm(`Deactivate ${patient.firstName} ${patient.lastName}?`)) {
      deleteMutation.mutate();
    }
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" pt={8}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !patient) {
    return (
      <Alert severity="error">
        {error ? (error as Error).message : 'Patient not found'}
      </Alert>
    );
  }

  return (
    <Box>
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link
          underline="hover"
          color="inherit"
          sx={{ cursor: 'pointer' }}
          onClick={() => navigate('/')}
        >
          Dashboard
        </Link>
        <Link
          underline="hover"
          color="inherit"
          sx={{ cursor: 'pointer' }}
          onClick={() => navigate('/patients')}
        >
          Patients
        </Link>
        <Typography color="text.primary">
          {patient.firstName} {patient.lastName}
        </Typography>
      </Breadcrumbs>

      <Stack direction="row" justifyContent="space-between" alignItems="flex-start" mb={3}>
        <Box>
          <Typography variant="h4">
            {patient.firstName} {patient.lastName}
          </Typography>
          <Chip
            label={patient.status}
            size="small"
            color={patient.status === 'Active' ? 'success' : 'default'}
            variant="outlined"
            sx={{ mt: 0.5 }}
          />
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
            disabledReason="Requires ProgramAdmin role or above to deactivate patients."
            disabled={deleteMutation.isPending || patient.status === 'Inactive'}
            onClick={handleDeactivate}
          >
            Deactivate
          </RoleGatedButton>
          <Button variant="text" onClick={() => navigate('/patients')}>
            Back
          </Button>
        </Stack>
      </Stack>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Patient Information"
              titleTypographyProps={{ variant: 'h6' }}
            />
            <Divider />
            <CardContent>
              <Field label="First Name" value={patient.firstName} />
              <Field label="Last Name" value={patient.lastName} />
              <Field label="Date of Birth" value={formatDate(patient.dateOfBirth)} />
              <Field label="Gender" value={patient.gender} />
              <Field label="Medical Record Number" value={patient.medicalRecordNumber} />
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Contact &amp; Program"
              titleTypographyProps={{ variant: 'h6' }}
            />
            <Divider />
            <CardContent>
              <Field label="Email" value={patient.email} />
              <Field label="Phone" value={patient.phone} />
              <Field label="Care Program" value={patient.careProgramName} />
              <Field label="Care Program ID" value={String(patient.careProgramId)} />
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Record Metadata"
              titleTypographyProps={{ variant: 'h6' }}
            />
            <Divider />
            <CardContent>
              <Field label="Patient ID" value={String(patient.id)} />
              <Field label="Created" value={formatDate(patient.createdAt)} />
              <Field label="Last Updated" value={formatDate(patient.updatedAt)} />
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
