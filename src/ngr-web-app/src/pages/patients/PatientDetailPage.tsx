import { useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
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
import DeleteForeverIcon from '@mui/icons-material/DeleteForever';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { useProgram } from '../../contexts/ProgramContext';
import { RoleGatedButton } from '../../components/RoleGatedButton';
import type { FormSubmissionDto, PatientFileDto } from '../../types';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

const REMOVAL_REASONS = [
  'Patient no longer seen within the program',
  'Patient withdrew consent',
  'Consent issue/unable to verify consent',
];

const FILE_TYPES = ['Informed Consent', 'Genotype Results', 'Sweat Test Results', 'Lab Results', 'Other'];
const ALLOWED_EXTENSIONS = '.pdf,.jpg,.jpeg,.png,.tif,.tiff';
const MAX_FILE_SIZE = 10 * 1024 * 1024;

// ── Form Table Component ─────────────────────────────────────────

function FormTable({
  title,
  forms,
  canDelete,
  onDelete,
  extraColumns,
}: {
  title: string;
  forms: FormSubmissionDto[];
  canDelete: boolean;
  onDelete: (id: number) => void;
  extraColumns?: { label: string; render: (f: FormSubmissionDto) => React.ReactNode }[];
}) {
  const [page, setPage] = useState(0);
  const pageSize = 5;
  const paged = forms.slice(page * pageSize, (page + 1) * pageSize);

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent sx={{ pb: 1 }}>
        <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>{title}</Typography>
        {forms.length === 0 ? (
          <Typography variant="body2" color="text.secondary">No records</Typography>
        ) : (
          <>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Status</TableCell>
                    {extraColumns?.map((col) => (
                      <TableCell key={col.label}>{col.label}</TableCell>
                    ))}
                    <TableCell>Last Modified</TableCell>
                    <TableCell>Modified By</TableCell>
                    {canDelete && <TableCell align="right">Actions</TableCell>}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {paged.map((f) => (
                    <TableRow key={f.id} hover>
                      <TableCell>
                        <Chip
                          label={f.status}
                          size="small"
                          color={f.status === 'Complete' ? 'success' : f.status === 'Incomplete' ? 'error' : 'default'}
                          variant="outlined"
                          sx={{ fontSize: '0.7rem' }}
                        />
                      </TableCell>
                      {extraColumns?.map((col) => (
                        <TableCell key={col.label}>{col.render(f)}</TableCell>
                      ))}
                      <TableCell>{formatDate(f.updatedAt)}</TableCell>
                      <TableCell>{f.lastModifiedBy ?? '—'}</TableCell>
                      {canDelete && (
                        <TableCell align="right">
                          <Button
                            size="small"
                            color="error"
                            onClick={() => {
                              if (window.confirm('Delete this form submission?')) onDelete(f.id);
                            }}
                          >
                            Delete
                          </Button>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            {forms.length > pageSize && (
              <TablePagination
                component="div"
                count={forms.length}
                page={page}
                rowsPerPage={pageSize}
                rowsPerPageOptions={[5]}
                onPageChange={(_, p) => setPage(p)}
              />
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}

// ── Patient Dashboard ────────────────────────────────────────────

export function PatientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { canEditPatient, canDeactivatePatient, isFoundationAdmin } = useRoles();
  const { selectedProgram } = useProgram();

  const [removeDialogOpen, setRemoveDialogOpen] = useState(false);
  const [removalReason, setRemovalReason] = useState('');
  const [hardDeleteOpen, setHardDeleteOpen] = useState(false);
  const [hardDeleteConfirm, setHardDeleteConfirm] = useState('');
  const [uploadOpen, setUploadOpen] = useState(false);
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadFileType, setUploadFileType] = useState('');
  const [uploadDesc, setUploadDesc] = useState('');
  const [uploadOtherDesc, setUploadOtherDesc] = useState('');
  const [uploadError, setUploadError] = useState('');
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Fetch dashboard data
  const { data: dashboard, isLoading, error } = useQuery({
    queryKey: ['patient', id, 'dashboard'],
    queryFn: () => patientsService.getDashboard(Number(id)),
    enabled: !!id,
  });

  // Fetch files separately for pagination
  const [filePage] = useState(0);
  const { data: files = [] } = useQuery({
    queryKey: ['patient', id, 'files', filePage],
    queryFn: () => patientsService.getFiles(Number(id), filePage + 1, 5),
    enabled: !!id,
  });

  const removeMutation = useMutation({
    mutationFn: () =>
      patientsService.removeFromProgram(Number(id), selectedProgram!.id, { removalReason }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['patients'] });
      navigate('/patients');
    },
  });

  const hardDeleteMutation = useMutation({
    mutationFn: () =>
      patientsService.hardDelete(Number(id), { confirmCffId: Number(hardDeleteConfirm) }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['patients'] });
      navigate('/patients');
    },
  });

  const deleteFormMutation = useMutation({
    mutationFn: (formId: number) => patientsService.deleteFormSubmission(Number(id), formId),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['patient', id] }),
  });

  const uploadMutation = useMutation({
    mutationFn: () =>
      patientsService.uploadFile(
        Number(id), uploadFile!, selectedProgram!.id,
        uploadFileType, uploadDesc || undefined,
        uploadFileType === 'Other' ? uploadOtherDesc : undefined,
      ),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['patient', id] });
      setUploadOpen(false);
      setUploadFile(null);
      setUploadFileType('');
      setUploadDesc('');
      setUploadOtherDesc('');
    },
  });

  const deleteFileMutation = useMutation({
    mutationFn: (fileId: number) => patientsService.deleteFile(Number(id), fileId),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['patient', id] }),
  });

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > MAX_FILE_SIZE) {
      setUploadError('File must be 10 MB or smaller.');
      setUploadFile(null);
      return;
    }
    setUploadError('');
    setUploadFile(file);
  };

  if (isLoading) {
    return <Box display="flex" justifyContent="center" pt={8}><CircularProgress /></Box>;
  }

  if (error || !dashboard) {
    return <Alert severity="error">{error ? (error as Error).message : 'Patient not found'}</Alert>;
  }

  const patient = dashboard.patient;

  return (
    <Box>
      {/* ── Dashboard Header ─────────────────────────────────── */}
      <Card sx={{ mb: 3, bgcolor: 'primary.main', color: 'white' }}>
        <CardContent>
          <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
            <Box>
              <Typography variant="h5" fontWeight={700}>
                {patient.firstName} {patient.middleName ? `${patient.middleName} ` : ''}{patient.lastName}
              </Typography>
              <Stack direction="row" spacing={2} sx={{ mt: 1, opacity: 0.9 }}>
                <Typography variant="body2">CFF ID: {patient.cffId}</Typography>
                <Typography variant="body2">DOB: {formatDate(patient.dateOfBirth)}</Typography>
                <Typography variant="body2">Diagnosis: {patient.diagnosis ?? '—'}</Typography>
              </Stack>
              {patient.vitalStatus === 'Deceased' && (
                <Chip label="Vital status: Deceased" color="error" size="small" sx={{ mt: 1 }} />
              )}
            </Box>
            <Stack direction="row" spacing={1}>
              <RoleGatedButton
                variant="outlined"
                allowed={canEditPatient}
                disabledReason="Requires ClinicalUser role"
                onClick={() => navigate(`/patients/${id}/edit`)}
                sx={{ color: 'white', borderColor: 'rgba(255,255,255,0.5)' }}
              >
                Edit
              </RoleGatedButton>
              <RoleGatedButton
                variant="outlined"
                color="error"
                allowed={canDeactivatePatient}
                disabledReason="Requires ProgramAdmin role"
                onClick={() => setRemoveDialogOpen(true)}
                sx={{ borderColor: 'rgba(255,255,255,0.5)' }}
              >
                Remove
              </RoleGatedButton>
              {isFoundationAdmin && (
                <Button
                  variant="outlined"
                  startIcon={<DeleteForeverIcon />}
                  onClick={() => setHardDeleteOpen(true)}
                  sx={{ color: '#ff6b6b', borderColor: 'rgba(255,107,107,0.5)' }}
                >
                  Hard Delete
                </Button>
              )}
              <Button
                variant="outlined"
                startIcon={<UploadFileIcon />}
                onClick={() => setUploadOpen(true)}
                sx={{ color: 'white', borderColor: 'rgba(255,255,255,0.5)' }}
              >
                Upload File
              </Button>
              <Button variant="text" onClick={() => navigate('/patients')} sx={{ color: 'white' }}>
                Back
              </Button>
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      {/* ── Form Tables (05-002) ─────────────────────────────── */}
      <FormTable
        title="Shared Forms (Demographics, Diagnosis, Sweat Test)"
        forms={dashboard.sharedForms}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
      />

      <FormTable
        title="Transplants"
        forms={dashboard.transplants}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
        extraColumns={[
          { label: 'Organ', render: (f) => f.transplantOrgan ?? '—' },
        ]}
      />

      <FormTable
        title="Annual Review"
        forms={dashboard.annualReviews}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
        extraColumns={[
          { label: 'Year', render: (f) => f.annualReviewYear?.toString() ?? '—' },
        ]}
      />

      <FormTable
        title="Encounters"
        forms={dashboard.encounters}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
        extraColumns={[
          { label: 'Encounter Date', render: (f) => f.encounterDate ? formatDate(f.encounterDate) : '—' },
        ]}
      />

      <FormTable
        title="Labs and Tests"
        forms={dashboard.labs}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
        extraColumns={[
          { label: 'Lab Date', render: (f) => f.labDate ? formatDate(f.labDate) : '—' },
        ]}
      />

      <FormTable
        title="Care Episodes"
        forms={dashboard.careEpisodes}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
        extraColumns={[
          { label: 'Start Date', render: (f) => f.careEpisodeStartDate ? formatDate(f.careEpisodeStartDate) : '—' },
          { label: 'End Date', render: (f) => f.careEpisodeEndDate ? formatDate(f.careEpisodeEndDate) : '—' },
        ]}
      />

      <FormTable
        title="Phone Notes"
        forms={dashboard.phoneNotes}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
        extraColumns={[
          { label: 'Date', render: (f) => f.phoneNoteDate ? formatDate(f.phoneNoteDate) : '—' },
        ]}
      />

      <FormTable
        title="ALD Status"
        forms={dashboard.aldStatus}
        canDelete={canEditPatient}
        onDelete={(fid) => deleteFormMutation.mutate(fid)}
      />

      {/* ── Files Table (05-007) ─────────────────────────────── */}
      <Card sx={{ mb: 2 }}>
        <CardContent sx={{ pb: 1 }}>
          <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>Files</Typography>
          {files.length === 0 ? (
            <Typography variant="body2" color="text.secondary">No files uploaded</Typography>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>File Type</TableCell>
                    <TableCell>File Name</TableCell>
                    <TableCell>Date Uploaded</TableCell>
                    <TableCell>Program</TableCell>
                    <TableCell>Size</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {files.map((f: PatientFileDto) => (
                    <TableRow key={f.id} hover>
                      <TableCell>{f.fileType}</TableCell>
                      <TableCell>{f.originalFileName}</TableCell>
                      <TableCell>{formatDate(f.uploadedAt)}</TableCell>
                      <TableCell>{f.programName}</TableCell>
                      <TableCell>{(f.fileSize / 1024).toFixed(0)} KB</TableCell>
                      <TableCell align="right">
                        <Button size="small" onClick={() => window.open(`${import.meta.env.VITE_API_URL ?? 'http://localhost:5000'}/api/patients/${id}/files/${f.id}/download`)}>
                          Download
                        </Button>
                        {(isFoundationAdmin || f.programId === selectedProgram?.id) && (
                          <Button
                            size="small"
                            color="error"
                            onClick={() => {
                              if (window.confirm('Delete this file?')) deleteFileMutation.mutate(f.id);
                            }}
                          >
                            Delete
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* ── Remove from Program Dialog ────────────────────────── */}
      <Dialog open={removeDialogOpen} onClose={() => setRemoveDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Remove Patient from Program</DialogTitle>
        <Divider />
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Remove <strong>{patient.firstName} {patient.lastName}</strong> from <strong>{selectedProgram?.name}</strong>?
          </Typography>
          {removalReason === 'Patient withdrew consent' && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              Consent withdrawal will remove this patient from ALL clinical programs.
            </Alert>
          )}
          <FormControl fullWidth size="small" required>
            <InputLabel>Removal Reason</InputLabel>
            <Select value={removalReason} label="Removal Reason" onChange={(e) => setRemovalReason(e.target.value)}>
              {REMOVAL_REASONS.map((r) => <MenuItem key={r} value={r}>{r}</MenuItem>)}
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setRemoveDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" color="error" onClick={() => removeMutation.mutate()} disabled={!removalReason || removeMutation.isPending}>
            Confirm Removal
          </Button>
        </DialogActions>
      </Dialog>

      {/* ── Hard-Delete Dialog (05-005) ───────────────────────── */}
      <Dialog open={hardDeleteOpen} onClose={() => setHardDeleteOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Permanently Delete Patient Record</DialogTitle>
        <Divider />
        <DialogContent>
          <Alert severity="error" sx={{ mb: 2 }}>
            This action is irreversible. ALL data including forms, files, and program associations will be permanently erased.
          </Alert>
          <Typography variant="body2" sx={{ mb: 2 }}>
            To confirm, type the patient's CFF ID: <strong>{patient.cffId}</strong>
          </Typography>
          <TextField
            label="CFF ID Confirmation"
            value={hardDeleteConfirm}
            onChange={(e) => setHardDeleteConfirm(e.target.value)}
            fullWidth
            size="small"
          />
          {hardDeleteMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {(hardDeleteMutation.error as Error)?.message || 'Delete failed.'}
            </Alert>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setHardDeleteOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={() => hardDeleteMutation.mutate()}
            disabled={hardDeleteConfirm !== String(patient.cffId) || hardDeleteMutation.isPending}
          >
            Permanently Delete
          </Button>
        </DialogActions>
      </Dialog>

      {/* ── File Upload Dialog (05-006) ───────────────────────── */}
      <Dialog open={uploadOpen} onClose={() => setUploadOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Upload Patient File</DialogTitle>
        <Divider />
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <input ref={fileInputRef} type="file" hidden accept={ALLOWED_EXTENSIONS} onChange={handleFileSelect} />
            <Button variant="outlined" onClick={() => fileInputRef.current?.click()}>
              {uploadFile ? uploadFile.name : 'Select File (.pdf, .jpg, .png, .tif)'}
            </Button>
            {uploadError && <Typography variant="caption" color="error">{uploadError}</Typography>}

            <FormControl fullWidth size="small" required>
              <InputLabel>File Type</InputLabel>
              <Select value={uploadFileType} label="File Type" onChange={(e) => setUploadFileType(e.target.value)}>
                {FILE_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
              </Select>
            </FormControl>

            {uploadFileType === 'Other' && (
              <TextField
                label="Other File Type Description"
                value={uploadOtherDesc}
                onChange={(e) => setUploadOtherDesc(e.target.value)}
                required
                fullWidth
                size="small"
              />
            )}

            <TextField
              label="Description"
              value={uploadDesc}
              onChange={(e) => setUploadDesc(e.target.value)}
              multiline
              rows={2}
              fullWidth
              size="small"
              inputProps={{ maxLength: 1000 }}
              helperText={`${uploadDesc.length}/1000`}
            />

            {uploadMutation.isError && (
              <Alert severity="error">
                {(uploadMutation.error as Error)?.message || 'Upload failed.'}
              </Alert>
            )}
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setUploadOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => uploadMutation.mutate()}
            disabled={!uploadFile || !uploadFileType || (uploadFileType === 'Other' && !uploadOtherDesc) || uploadMutation.isPending}
          >
            {uploadMutation.isPending ? <CircularProgress size={20} /> : 'Upload'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
