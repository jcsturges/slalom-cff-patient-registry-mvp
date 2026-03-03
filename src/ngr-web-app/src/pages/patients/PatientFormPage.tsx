import { useState, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  CircularProgress,
  FormControl,
  FormControlLabel,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { patientsService } from '../../services/patients';
import { useProgram } from '../../contexts/ProgramContext';
import { useRoles } from '../../hooks/useRoles';
import type { CreatePatientDto, UpdatePatientDto, DuplicateMatchDto } from '../../types';

const SEX_OPTIONS = ['Male', 'Female', 'Unknown'];
const STATUS_OPTIONS = ['Active', 'Inactive', 'Transferred', 'Deceased'];

export function PatientFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { selectedProgram } = useProgram();
  const { canCreatePatient, canEditPatient } = useRoles();
  const isEditing = Boolean(id);

  // Form state
  const [firstName, setFirstName] = useState('');
  const [middleName, setMiddleName] = useState('');
  const [lastName, setLastName] = useState('');
  const [lastNameAtBirth, setLastNameAtBirth] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [biologicalSex, setBiologicalSex] = useState('');
  const [ssnLast4, setSsnLast4] = useState('');
  const [knownRegistryId, setKnownRegistryId] = useState('');
  const [mrn, setMrn] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [gender, setGender] = useState('');
  const [status, setStatus] = useState('Active');
  const [consentChecked, setConsentChecked] = useState(false);

  // Duplicate detection state
  const [duplicateMatches, setDuplicateMatches] = useState<DuplicateMatchDto[]>([]);
  const [duplicateChecked, setDuplicateChecked] = useState(false);
  const [overrideConfirmed, setOverrideConfirmed] = useState(false);

  // Load existing patient for edit mode
  const { isLoading: loadingPatient } = useQuery({
    queryKey: ['patient', id],
    queryFn: async () => {
      const data = await patientsService.getById(Number(id));
      setFirstName(data.firstName);
      setMiddleName(data.middleName ?? '');
      setLastName(data.lastName);
      setLastNameAtBirth(data.lastNameAtBirth ?? '');
      setDateOfBirth(data.dateOfBirth?.slice(0, 10) ?? '');
      setBiologicalSex(data.biologicalSexAtBirth ?? '');
      setGender(data.gender ?? '');
      setMrn(data.medicalRecordNumber ?? '');
      setEmail(data.email ?? '');
      setPhone(data.phone ?? '');
      setStatus(data.status);
      setConsentChecked(true);
      setDuplicateChecked(true);
      return data;
    },
    enabled: isEditing,
  });

  const dupCheckMutation = useMutation({
    mutationFn: () =>
      patientsService.checkDuplicates({
        firstName: firstName || undefined,
        lastName: lastName || undefined,
        dateOfBirth: dateOfBirth || undefined,
        biologicalSexAtBirth: biologicalSex || undefined,
        registryId: knownRegistryId || undefined,
      }),
    onSuccess: (matches) => {
      setDuplicateMatches(matches);
      setDuplicateChecked(true);
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePatientDto) => patientsService.create(data),
    onSuccess: (result) => navigate(`/patients/${result.id}`),
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdatePatientDto) => patientsService.update(Number(id), data),
    onSuccess: (result) => navigate(`/patients/${result.id}`),
  });

  const reacquireMutation = useMutation({
    mutationFn: (patientId: number) =>
      patientsService.addToProgram(patientId, {
        programId: selectedProgram!.id,
        isPrimaryProgram: false,
      }),
    onSuccess: (_, patientId) => navigate(`/patients/${patientId}`),
  });

  // ── Validation ──────────────────────────────────────────────────
  const hasIdentityComboA = firstName && lastName && dateOfBirth && biologicalSex;
  const hasIdentityComboB = knownRegistryId && (firstName || lastName || dateOfBirth);
  const canContinue = consentChecked && (hasIdentityComboA || hasIdentityComboB);

  const highConfidenceMatches = duplicateMatches.filter((m) => m.confidenceScore >= 0.8);
  const needsOverride = highConfidenceMatches.length > 0 && !overrideConfirmed;
  const tooManyMatches = duplicateMatches.length > 3;

  const canSubmitNew = useMemo(() => {
    if (!duplicateChecked) return false;
    if (tooManyMatches) return false;
    if (needsOverride) return false;
    return true;
  }, [duplicateChecked, tooManyMatches, needsOverride]);

  const handleContinue = () => {
    dupCheckMutation.mutate();
  };

  const handleSubmitNew = () => {
    createMutation.mutate({
      firstName,
      middleName: middleName || undefined,
      lastName,
      lastNameAtBirth: lastNameAtBirth || undefined,
      dateOfBirth,
      biologicalSexAtBirth: biologicalSex || undefined,
      ssnLast4: ssnLast4 || undefined,
      medicalRecordNumber: mrn || undefined,
      gender: gender || biologicalSex || undefined,
      email: email || undefined,
      phone: phone || undefined,
      careProgramId: selectedProgram?.id,
      knownRegistryId: knownRegistryId || undefined,
    });
  };

  const handleUpdate = () => {
    updateMutation.mutate({
      firstName,
      middleName: middleName || undefined,
      lastName,
      lastNameAtBirth: lastNameAtBirth || undefined,
      dateOfBirth,
      biologicalSexAtBirth: biologicalSex || undefined,
      medicalRecordNumber: mrn || undefined,
      gender: gender || biologicalSex || undefined,
      email: email || undefined,
      phone: phone || undefined,
      status,
    });
  };

  const handleSelectExisting = (patientId: number) => {
    if (!selectedProgram) return;
    reacquireMutation.mutate(patientId);
  };

  if (isEditing && !canEditPatient) {
    return <Alert severity="error">Requires ClinicalUser role or above to edit patients.</Alert>;
  }
  if (!isEditing && !canCreatePatient) {
    return <Alert severity="error">Requires ClinicalUser role or above to add patients.</Alert>;
  }
  if (isEditing && loadingPatient) {
    return <Box display="flex" justifyContent="center" py={4}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {isEditing ? 'Edit Patient' : 'Add Patient to Program'}
      </Typography>
      {selectedProgram && !isEditing && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Adding to: {selectedProgram.name} ({selectedProgram.programId})
        </Typography>
      )}

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>Patient Identity</Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4}>
              <TextField label="First Name" value={firstName} onChange={(e) => setFirstName(e.target.value)} required fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Middle Name" value={middleName} onChange={(e) => setMiddleName(e.target.value)} fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Last Name" value={lastName} onChange={(e) => setLastName(e.target.value)} required fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Last Name at Birth" value={lastNameAtBirth} onChange={(e) => setLastNameAtBirth(e.target.value)} fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Date of Birth" type="date" value={dateOfBirth} onChange={(e) => setDateOfBirth(e.target.value)} required fullWidth size="small" InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControl fullWidth size="small" required>
                <InputLabel>Biological Sex at Birth</InputLabel>
                <Select value={biologicalSex} label="Biological Sex at Birth" onChange={(e) => setBiologicalSex(e.target.value)}>
                  {SEX_OPTIONS.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Last 4 of SSN" value={ssnLast4} onChange={(e) => setSsnLast4(e.target.value.replace(/\D/g, '').slice(0, 4))} fullWidth size="small" inputProps={{ maxLength: 4 }} helperText="Optional" />
            </Grid>
            {!isEditing && (
              <Grid item xs={12} sm={4}>
                <TextField label="Registry ID (if known)" value={knownRegistryId} onChange={(e) => setKnownRegistryId(e.target.value)} fullWidth size="small" helperText="CFF ID or Registry ID" />
              </Grid>
            )}
            <Grid item xs={12} sm={4}>
              <TextField label="MRN" value={mrn} onChange={(e) => setMrn(e.target.value)} fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth size="small" />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField label="Phone" value={phone} onChange={(e) => setPhone(e.target.value)} fullWidth size="small" />
            </Grid>
            {isEditing && (
              <Grid item xs={12} sm={4}>
                <FormControl fullWidth size="small">
                  <InputLabel>Status</InputLabel>
                  <Select value={status} label="Status" onChange={(e) => setStatus(e.target.value)}>
                    {STATUS_OPTIONS.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                  </Select>
                </FormControl>
              </Grid>
            )}
          </Grid>

          {!isEditing && (
            <Box sx={{ mt: 3, p: 2, bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider', borderRadius: 1 }}>
              <FormControlLabel
                control={<Checkbox checked={consentChecked} onChange={(e) => setConsentChecked(e.target.checked)} />}
                label={
                  <Typography variant="body2">
                    I acknowledge that the patient has consented to participate in the CF Foundation Patient Registry Study.
                  </Typography>
                }
              />
            </Box>
          )}

          <Stack direction="row" spacing={2} sx={{ mt: 3 }}>
            <Button variant="outlined" onClick={() => navigate(-1)}>Cancel</Button>
            {isEditing ? (
              <Button variant="contained" onClick={handleUpdate} disabled={!firstName || !lastName || !dateOfBirth || updateMutation.isPending}>
                {updateMutation.isPending ? <CircularProgress size={20} /> : 'Save Changes'}
              </Button>
            ) : !duplicateChecked ? (
              <Button variant="contained" onClick={handleContinue} disabled={!canContinue || dupCheckMutation.isPending}>
                {dupCheckMutation.isPending ? <CircularProgress size={20} /> : 'Continue'}
              </Button>
            ) : null}
          </Stack>

          {(createMutation.isError || updateMutation.isError || reacquireMutation.isError) && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {((createMutation.error || updateMutation.error || reacquireMutation.error) as Error)?.message || 'An error occurred.'}
            </Alert>
          )}
        </CardContent>
      </Card>

      {/* ── Duplicate Detection Results (04-005) ──────────────── */}
      {duplicateChecked && !isEditing && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            {duplicateMatches.length === 0 ? (
              <Box>
                <Alert severity="success" sx={{ mb: 2 }}>No duplicate records found. You may proceed with creating a new patient.</Alert>
                <Button variant="contained" onClick={handleSubmitNew} disabled={createMutation.isPending}>
                  {createMutation.isPending ? <CircularProgress size={20} /> : 'Create New Patient'}
                </Button>
              </Box>
            ) : tooManyMatches ? (
              <Alert severity="warning">
                More than 3 potential matches found. Please provide additional identifying information or contact Registry Support for assistance.
              </Alert>
            ) : (
              <Box>
                <Alert severity="info" sx={{ mb: 2 }}>
                  {duplicateMatches.length} potential match{duplicateMatches.length > 1 ? 'es' : ''} found. Select an existing patient or create a new record.
                </Alert>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>CFF ID</TableCell>
                        <TableCell>Name</TableCell>
                        <TableCell>Last Name at Birth</TableCell>
                        <TableCell>DOB</TableCell>
                        <TableCell>Programs</TableCell>
                        <TableCell>Confidence</TableCell>
                        <TableCell>Action</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {duplicateMatches.map((match) => (
                        <TableRow key={match.patientId}>
                          <TableCell><Typography variant="body2" fontWeight={600}>{match.cffId}</Typography></TableCell>
                          <TableCell>{match.firstName} {match.lastName}</TableCell>
                          <TableCell>{match.lastNameAtBirth ?? '—'}</TableCell>
                          <TableCell>{new Date(match.dateOfBirth).toLocaleDateString()}</TableCell>
                          <TableCell>
                            {match.isOrh ? <Chip label="ORH" size="small" color="warning" variant="outlined" /> : (match.programAssociations.join(', ') || '—')}
                          </TableCell>
                          <TableCell>
                            <Chip label={`${(match.confidenceScore * 100).toFixed(0)}%`} size="small" color={match.confidenceScore >= 0.8 ? 'error' : match.confidenceScore >= 0.6 ? 'warning' : 'default'} />
                          </TableCell>
                          <TableCell>
                            <Button size="small" variant="outlined" onClick={() => handleSelectExisting(match.patientId)} disabled={reacquireMutation.isPending}>Select</Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>

                {highConfidenceMatches.length > 0 && !overrideConfirmed && (
                  <Alert severity="warning" sx={{ mt: 2 }}>
                    <Typography variant="body2" gutterBottom>High-confidence matches found. Are you sure this is a new patient?</Typography>
                    <Button size="small" variant="outlined" color="warning" onClick={() => setOverrideConfirmed(true)}>
                      Yes, create new patient (not a duplicate)
                    </Button>
                  </Alert>
                )}

                {canSubmitNew && (
                  <Box sx={{ mt: 2 }}>
                    <Button variant="contained" onClick={handleSubmitNew} disabled={createMutation.isPending}>
                      {createMutation.isPending ? <CircularProgress size={20} /> : 'Create New Patient'}
                    </Button>
                  </Box>
                )}
              </Box>
            )}
          </CardContent>
        </Card>
      )}
    </Box>
  );
}
