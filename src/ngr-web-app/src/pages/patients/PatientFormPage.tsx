import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Autocomplete,
  Box,
  Breadcrumbs,
  Button,
  Card,
  CardContent,
  CircularProgress,
  FormControl,
  Grid,
  InputLabel,
  Link,
  MenuItem,
  Select,
  type SelectChangeEvent,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { programsService } from '../../services/programs';
import type { CareProgramDto, CreatePatientDto, UpdatePatientDto } from '../../types';
import { useRoles } from '../../hooks/useRoles';

interface FormValues {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  medicalRecordNumber: string;
  gender: string;
  email: string;
  phone: string;
  careProgramId: string;
  status: string;
}

const EMPTY: FormValues = {
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  medicalRecordNumber: '',
  gender: '',
  email: '',
  phone: '',
  careProgramId: '',
  status: 'Active',
};

export function PatientFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { canEditPatient } = useRoles();

  const [values, setValues] = useState<FormValues>(EMPTY);
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<keyof FormValues, string>>>({});
  const [selectedProgram, setSelectedProgram] = useState<CareProgramDto | null>(null);
  const [programSearch, setProgramSearch] = useState('');

  // Load existing patient when editing
  const { data: patient, isLoading: loadingPatient } = useQuery({
    queryKey: ['patient', id],
    queryFn: () => patientsService.getById(Number(id)),
    enabled: isEdit,
  });

  // Search programs for typeahead (exclude ORH, training, inactive)
  const { data: programOptions = [] } = useQuery({
    queryKey: ['programs-lookup', programSearch],
    queryFn: () =>
      programsService.getAll({
        name: programSearch || undefined,
        pageSize: 20,
      }),
  });

  // Load selected program when editing (to show display title)
  const { data: editProgram } = useQuery({
    queryKey: ['program', patient?.careProgramId],
    queryFn: () => programsService.getById(patient!.careProgramId),
    enabled: isEdit && !!patient?.careProgramId,
  });

  useEffect(() => {
    if (editProgram && !selectedProgram) {
      setSelectedProgram(editProgram);
    }
  }, [editProgram, selectedProgram]);

  useEffect(() => {
    if (patient) {
      setValues({
        firstName: patient.firstName,
        lastName: patient.lastName,
        dateOfBirth: patient.dateOfBirth.slice(0, 10),
        medicalRecordNumber: patient.medicalRecordNumber ?? '',
        gender: patient.gender ?? '',
        email: patient.email ?? '',
        phone: patient.phone ?? '',
        careProgramId: String(patient.careProgramId),
        status: patient.status,
      });
    }
  }, [patient]);

  const createMutation = useMutation({
    mutationFn: (data: CreatePatientDto) => patientsService.create(data),
    onSuccess: (created) => {
      void queryClient.invalidateQueries({ queryKey: ['patients'] });
      navigate(`/patients/${created.id}`);
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdatePatientDto) => patientsService.update(Number(id), data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['patients'] });
      void queryClient.invalidateQueries({ queryKey: ['patient', id] });
      navigate(`/patients/${id}`);
    },
  });

  const setField =
    (field: keyof FormValues) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      setValues((v) => ({ ...v, [field]: e.target.value }));
      setFieldErrors((err) => ({ ...err, [field]: undefined }));
    };

  const setSelect = (field: keyof FormValues) => (e: SelectChangeEvent) => {
    setValues((v) => ({ ...v, [field]: e.target.value }));
  };

  const validate = (): boolean => {
    const errs: Partial<Record<keyof FormValues, string>> = {};
    if (!values.firstName.trim()) errs.firstName = 'Required';
    if (!values.lastName.trim()) errs.lastName = 'Required';
    if (!values.dateOfBirth) errs.dateOfBirth = 'Required';
    setFieldErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    if (isEdit) {
      updateMutation.mutate({
        firstName: values.firstName,
        lastName: values.lastName,
        dateOfBirth: values.dateOfBirth,
        medicalRecordNumber: values.medicalRecordNumber || undefined,
        gender: values.gender || undefined,
        email: values.email || undefined,
        phone: values.phone || undefined,
        status: values.status,
      });
    } else {
      createMutation.mutate({
        firstName: values.firstName,
        lastName: values.lastName,
        dateOfBirth: values.dateOfBirth,
        medicalRecordNumber: values.medicalRecordNumber || undefined,
        gender: values.gender || undefined,
        email: values.email || undefined,
        phone: values.phone || undefined,
        careProgramId: values.careProgramId ? Number(values.careProgramId) : undefined,
      });
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;
  const mutationError = createMutation.error ?? updateMutation.error;

  if (!canEditPatient) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        You do not have permission to {isEdit ? 'edit' : 'add'} patients. Requires ClinicalUser role or above.
      </Alert>
    );
  }

  if (isEdit && loadingPatient) {
    return (
      <Box display="flex" justifyContent="center" pt={8}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box component="form" onSubmit={handleSubmit}>
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
        <Typography color="text.primary">{isEdit ? 'Edit Patient' : 'Add Patient'}</Typography>
      </Breadcrumbs>

      <Typography variant="h4" mb={3}>
        {isEdit ? 'Edit Patient' : 'Add Patient'}
      </Typography>

      {mutationError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {(mutationError as Error).message}
        </Alert>
      )}

      <Card sx={{ maxWidth: 720 }}>
        <CardContent sx={{ p: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                label="First Name"
                fullWidth
                required
                value={values.firstName}
                onChange={setField('firstName')}
                error={!!fieldErrors.firstName}
                helperText={fieldErrors.firstName}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Last Name"
                fullWidth
                required
                value={values.lastName}
                onChange={setField('lastName')}
                error={!!fieldErrors.lastName}
                helperText={fieldErrors.lastName}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Date of Birth"
                type="date"
                fullWidth
                required
                InputLabelProps={{ shrink: true }}
                value={values.dateOfBirth}
                onChange={setField('dateOfBirth')}
                error={!!fieldErrors.dateOfBirth}
                helperText={fieldErrors.dateOfBirth}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>Gender</InputLabel>
                <Select value={values.gender} label="Gender" onChange={setSelect('gender')}>
                  <MenuItem value="">Not specified</MenuItem>
                  <MenuItem value="Male">Male</MenuItem>
                  <MenuItem value="Female">Female</MenuItem>
                  <MenuItem value="Other">Other</MenuItem>
                  <MenuItem value="Unknown">Unknown</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Medical Record Number"
                fullWidth
                value={values.medicalRecordNumber}
                onChange={setField('medicalRecordNumber')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Autocomplete
                options={programOptions}
                value={selectedProgram}
                disabled={isEdit}
                getOptionLabel={(opt) => opt.displayTitle}
                isOptionEqualToValue={(opt, val) => opt.id === val.id}
                onInputChange={(_, value) => setProgramSearch(value)}
                onChange={(_, program) => {
                  setSelectedProgram(program);
                  setValues((v) => ({
                    ...v,
                    careProgramId: program ? String(program.id) : '',
                  }));
                  setFieldErrors((err) => ({ ...err, careProgramId: undefined }));
                }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Care Program"
                    error={!!fieldErrors.careProgramId}
                    helperText={fieldErrors.careProgramId ?? (isEdit ? undefined : 'Type to search by name')}
                  />
                )}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Email"
                type="email"
                fullWidth
                value={values.email}
                onChange={setField('email')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Phone"
                fullWidth
                value={values.phone}
                onChange={setField('phone')}
              />
            </Grid>
            {isEdit && (
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select value={values.status} label="Status" onChange={setSelect('status')}>
                    <MenuItem value="Active">Active</MenuItem>
                    <MenuItem value="Inactive">Inactive</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            )}
          </Grid>

          <Stack direction="row" spacing={2} mt={3}>
            <Button type="submit" variant="contained" disabled={isPending}>
              {isPending ? 'Saving…' : isEdit ? 'Save Changes' : 'Add Patient'}
            </Button>
            <Button
              variant="text"
              disabled={isPending}
              onClick={() => navigate(isEdit ? `/patients/${id}` : '/patients')}
            >
              Cancel
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
