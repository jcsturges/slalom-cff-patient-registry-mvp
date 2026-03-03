import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
  Stack,
  Typography,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import type { PatientDto, FormSubmissionDto, ValidationMessageDto } from '../../types';

interface FormPageLayoutProps {
  patient: PatientDto;
  form: FormSubmissionDto;
  /** Form-specific context label (e.g., "Encounter Date: Jan 15, 2026") */
  contextLabel?: string;
  /** Whether the user has unsaved changes */
  hasChanges: boolean;
  /** Whether save/mark-complete are in progress */
  saving: boolean;
  /** Validation messages to display */
  validationMessages?: ValidationMessageDto[];
  /** Whether the form can be marked complete */
  canComplete?: boolean;
  /** Whether the form can be saved */
  canSave?: boolean;
  onSave: () => void;
  onMarkComplete: () => void;
  /** Whether "Mark Complete" button is relevant (some forms auto-complete) */
  showMarkComplete?: boolean;
  children: React.ReactNode;
}

export function FormPageLayout({
  patient,
  form,
  contextLabel,
  hasChanges,
  saving,
  validationMessages = [],
  canComplete = true,
  canSave = true,
  onSave,
  onMarkComplete,
  showMarkComplete = true,
  children,
}: FormPageLayoutProps) {
  const navigate = useNavigate();
  const [exitConfirmOpen, setExitConfirmOpen] = useState(false);

  const saveBlockers = validationMessages.filter((m) => m.severity === 'SaveBlocking');
  const completionBlockers = validationMessages.filter((m) => m.severity === 'CompletionBlocking');
  const warnings = validationMessages.filter((m) => m.severity === 'Warning');
  const isLocked = form.lockStatus === 'Locked';

  const handleExit = () => {
    if (hasChanges) {
      setExitConfirmOpen(true);
    } else {
      navigate(`/patients/${patient.id}`);
    }
  };

  return (
    <Box>
      {/* ── Patient Header ────────────────────────────────────── */}
      <Card sx={{ mb: 2, bgcolor: 'primary.main', color: 'white' }}>
        <CardContent sx={{ py: 1.5 }}>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Box>
              <Typography variant="subtitle1" fontWeight={600}>
                {patient.firstName} {patient.lastName} — CFF ID: {patient.cffId}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.85 }}>
                DOB: {new Date(patient.dateOfBirth).toLocaleDateString()} | Diagnosis: {patient.diagnosis ?? '—'}
              </Typography>
            </Box>
            <Stack direction="row" spacing={1}>
              <Chip label={form.completionStatus} size="small"
                color={form.completionStatus === 'Complete' ? 'success' : 'warning'} />
              {isLocked && <Chip label="Locked" size="small" color="error" />}
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      {/* ── Form Context ──────────────────────────────────────── */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <Box>
          <Typography variant="h5" fontWeight={600}>{form.formName}</Typography>
          {contextLabel && (
            <Typography variant="body2" color="text.secondary">{contextLabel}</Typography>
          )}
        </Box>
        <Typography variant="caption" color="text.secondary">
          {form.programName} | Last saved: {new Date(form.updatedAt).toLocaleString()}
          {form.lastModifiedBy && ` by ${form.lastModifiedBy}`}
        </Typography>
      </Stack>

      {/* ── EMR Review Banner (06-012) ────────────────────────── */}
      {form.requiresReview && (
        <Alert severity="info" sx={{ mb: 2 }}>
          This form was updated from EMR and requires review. Please validate and Mark Complete.
        </Alert>
      )}

      {/* ── Locked Banner ─────────────────────────────────────── */}
      {isLocked && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          This form is locked (reporting year finalized). Read-only access for care program users.
        </Alert>
      )}

      {/* ── Validation Messages ───────────────────────────────── */}
      {saveBlockers.length > 0 && (
        <Alert severity="error" sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>Cannot save — fix these errors:</Typography>
          {saveBlockers.map((m, i) => (
            <Typography key={i} variant="body2">
              {m.fieldLabel}: {m.message}
              {m.correctiveAction && <em> ({m.correctiveAction})</em>}
            </Typography>
          ))}
        </Alert>
      )}

      {completionBlockers.length > 0 && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>Cannot mark complete — required fields missing:</Typography>
          {completionBlockers.map((m, i) => (
            <Typography key={i} variant="body2">
              {m.fieldLabel}: {m.message}
            </Typography>
          ))}
        </Alert>
      )}

      {warnings.length > 0 && (
        <Alert severity="info" sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>Warnings:</Typography>
          {warnings.map((m, i) => (
            <Typography key={i} variant="body2">
              {m.fieldLabel}: {m.message}
            </Typography>
          ))}
        </Alert>
      )}

      {/* ── Form Content ──────────────────────────────────────── */}
      <Card sx={{ mb: 3 }}>
        <CardContent>{children}</CardContent>
      </Card>

      {/* ── Action Buttons ────────────────────────────────────── */}
      <Stack direction="row" spacing={2}>
        <Button
          variant="contained"
          startIcon={saving ? <CircularProgress size={16} /> : <SaveIcon />}
          onClick={onSave}
          disabled={saving || !canSave || isLocked}
        >
          Save
        </Button>
        {showMarkComplete && (
          <Button
            variant="contained"
            color="success"
            startIcon={<CheckCircleIcon />}
            onClick={onMarkComplete}
            disabled={saving || !canComplete || isLocked}
          >
            Save &amp; Mark Complete
          </Button>
        )}
        <Button
          variant="outlined"
          startIcon={<ExitToAppIcon />}
          onClick={handleExit}
        >
          Exit
        </Button>
      </Stack>

      {/* ── Exit Confirmation ─────────────────────────────────── */}
      <Dialog open={exitConfirmOpen} onClose={() => setExitConfirmOpen(false)}>
        <DialogTitle>Unsaved Changes</DialogTitle>
        <Divider />
        <DialogContent>
          <Typography>You have unsaved changes. Are you sure you want to exit?</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setExitConfirmOpen(false)}>Stay</Button>
          <Button variant="contained" color="error" onClick={() => navigate(`/patients/${patient.id}`)}>
            Exit Without Saving
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
