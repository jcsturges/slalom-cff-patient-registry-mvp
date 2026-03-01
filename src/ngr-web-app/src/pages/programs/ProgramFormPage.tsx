import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Breadcrumbs,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  FormControl,
  FormControlLabel,
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
import { programsService } from '../../services/programs';
import type { CreateCareProgramDto, UpdateCareProgramDto } from '../../types';
import { useRoles } from '../../hooks/useRoles';
import { US_STATES, PROGRAM_TYPES } from '../../constants';

interface FormValues {
  programId: string;
  code: string;
  name: string;
  programType: string;
  city: string;
  state: string;
  address1: string;
  address2: string;
  zipCode: string;
  phone: string;
  email: string;
  isActive: boolean;
  isTrainingProgram: boolean;
}

const EMPTY: FormValues = {
  programId: '',
  code: '',
  name: '',
  programType: 'Adult',
  city: '',
  state: '',
  address1: '',
  address2: '',
  zipCode: '',
  phone: '',
  email: '',
  isActive: true,
  isTrainingProgram: false,
};

export function ProgramFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { isFoundationAdmin } = useRoles();
  const [values, setValues] = useState<FormValues>(EMPTY);
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<keyof FormValues, string>>>({});

  const { data: program, isLoading: loadingProgram } = useQuery({
    queryKey: ['program', id],
    queryFn: () => programsService.getById(Number(id)),
    enabled: isEdit,
  });

  useEffect(() => {
    if (program) {
      setValues({
        programId: String(program.programId),
        code: program.code,
        name: program.name,
        programType: program.programType,
        city: program.city ?? '',
        state: program.state ?? '',
        address1: program.address1 ?? '',
        address2: program.address2 ?? '',
        zipCode: program.zipCode ?? '',
        phone: program.phone ?? '',
        email: program.email ?? '',
        isActive: program.isActive,
        isTrainingProgram: program.isTrainingProgram,
      });
    }
  }, [program]);

  const createMutation = useMutation({
    mutationFn: (data: CreateCareProgramDto) => programsService.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['programs'] });
      navigate('/programs');
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateCareProgramDto) => programsService.update(Number(id), data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['programs'] });
      void queryClient.invalidateQueries({ queryKey: ['program', id] });
      navigate('/programs');
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
    if (!isEdit && !values.programId.trim()) errs.programId = 'Required';
    if (!isEdit && values.programId && isNaN(Number(values.programId)))
      errs.programId = 'Must be a number';
    if (!isEdit && values.programId && Number(values.programId) <= 0)
      errs.programId = 'Must be positive';
    if (!isEdit && !values.code.trim()) errs.code = 'Required';
    if (!values.name.trim()) errs.name = 'Required';
    if (!values.programType) errs.programType = 'Required';
    setFieldErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    if (isEdit) {
      updateMutation.mutate({
        name: values.name,
        programType: values.programType,
        city: values.city || undefined,
        state: values.state || undefined,
        address1: values.address1 || undefined,
        address2: values.address2 || undefined,
        zipCode: values.zipCode || undefined,
        phone: values.phone || undefined,
        email: values.email || undefined,
        isActive: values.isActive,
      });
    } else {
      createMutation.mutate({
        programId: Number(values.programId),
        code: values.code,
        name: values.name,
        programType: values.isTrainingProgram ? 'Training' : values.programType,
        city: values.city || undefined,
        state: values.state || undefined,
        address1: values.address1 || undefined,
        address2: values.address2 || undefined,
        zipCode: values.zipCode || undefined,
        phone: values.phone || undefined,
        email: values.email || undefined,
        isTrainingProgram: values.isTrainingProgram,
      });
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;
  const mutationError = createMutation.error ?? updateMutation.error;

  if (!isFoundationAdmin) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        You do not have permission to manage care programs. Requires Foundation Analyst or System Admin role.
      </Alert>
    );
  }

  if (isEdit && loadingProgram) {
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
          onClick={() => navigate('/programs')}
        >
          Programs
        </Link>
        <Typography color="text.primary">
          {isEdit ? 'Edit Program' : 'Add Program'}
        </Typography>
      </Breadcrumbs>

      <Typography variant="h4" mb={3}>
        {isEdit ? 'Edit Program' : 'Add Program'}
      </Typography>

      {mutationError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {(mutationError as Error).message}
        </Alert>
      )}

      {program?.isTrainingProgram && (
        <Alert severity="info" sx={{ mb: 3 }}>
          This is a Training Program. Data entered here is excluded from analytics and reporting.
        </Alert>
      )}

      <Card sx={{ maxWidth: 720 }}>
        <CardContent sx={{ p: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4}>
              <TextField
                label="Program ID"
                fullWidth
                required
                type="number"
                disabled={isEdit}
                value={values.programId}
                onChange={setField('programId')}
                error={!!fieldErrors.programId}
                helperText={fieldErrors.programId ?? (isEdit ? 'Cannot be changed' : 'Unique identifier')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                label="Code"
                fullWidth
                required
                disabled={isEdit}
                value={values.code}
                onChange={setField('code')}
                error={!!fieldErrors.code}
                helperText={fieldErrors.code}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControl fullWidth required error={!!fieldErrors.programType}>
                <InputLabel>Program Type</InputLabel>
                <Select
                  value={values.isTrainingProgram ? 'Training' : values.programType}
                  label="Program Type"
                  onChange={setSelect('programType')}
                  disabled={values.isTrainingProgram}
                >
                  {PROGRAM_TYPES.map((t) => (
                    <MenuItem key={t} value={t}>{t}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Program Name"
                fullWidth
                required
                value={values.name}
                onChange={setField('name')}
                error={!!fieldErrors.name}
                helperText={fieldErrors.name}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Address Line 1"
                fullWidth
                value={values.address1}
                onChange={setField('address1')}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Address Line 2"
                fullWidth
                value={values.address2}
                onChange={setField('address2')}
              />
            </Grid>
            <Grid item xs={12} sm={5}>
              <TextField
                label="City"
                fullWidth
                value={values.city}
                onChange={setField('city')}
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <FormControl fullWidth>
                <InputLabel>State</InputLabel>
                <Select value={values.state} label="State" onChange={setSelect('state')}>
                  <MenuItem value="">—</MenuItem>
                  {US_STATES.map((s) => (
                    <MenuItem key={s} value={s}>{s}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                label="ZIP Code"
                fullWidth
                value={values.zipCode}
                onChange={setField('zipCode')}
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
            <Grid item xs={12} sm={6}>
              <TextField
                label="Email"
                type="email"
                fullWidth
                value={values.email}
                onChange={setField('email')}
              />
            </Grid>
            {!isEdit && (
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={values.isTrainingProgram}
                      onChange={(e) =>
                        setValues((v) => ({ ...v, isTrainingProgram: e.target.checked }))
                      }
                    />
                  }
                  label="Training Program (excluded from analytics and reporting)"
                />
              </Grid>
            )}
            {isEdit && (
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={values.isActive ? 'Active' : 'Inactive'}
                    label="Status"
                    onChange={(e) =>
                      setValues((v) => ({ ...v, isActive: e.target.value === 'Active' }))
                    }
                  >
                    <MenuItem value="Active">Active</MenuItem>
                    <MenuItem value="Inactive">Inactive</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            )}
          </Grid>

          <Stack direction="row" spacing={2} mt={3}>
            <Button type="submit" variant="contained" disabled={isPending}>
              {isPending ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Program'}
            </Button>
            <Button variant="text" disabled={isPending} onClick={() => navigate('/programs')}>
              Cancel
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}
