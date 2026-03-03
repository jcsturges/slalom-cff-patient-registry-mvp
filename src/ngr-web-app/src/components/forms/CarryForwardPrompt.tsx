import {
  Alert,
  Button,
  Stack,
  Typography,
} from '@mui/material';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';

interface CarryForwardPromptProps {
  /** Type of sub-form (e.g., "Medications", "Complications") */
  subFormLabel: string;
  /** Whether carry-forward data is available */
  available: boolean;
  /** Whether the prompt has been dismissed */
  dismissed: boolean;
  onCarryForwardAll: () => void;
  onCancel: () => void;
}

/**
 * One-time carry-forward prompt displayed when first accessing
 * Medications or Complications sub-form on an Encounter.
 * Dismissed after selection and never shown again for that sub-form instance.
 */
export function CarryForwardPrompt({
  subFormLabel,
  available,
  dismissed,
  onCarryForwardAll,
  onCancel,
}: CarryForwardPromptProps) {
  if (dismissed || !available) return null;

  return (
    <Alert
      severity="info"
      sx={{ mb: 3 }}
      action={
        <Stack direction="row" spacing={1}>
          <Button
            size="small"
            variant="contained"
            startIcon={<ContentCopyIcon />}
            onClick={onCarryForwardAll}
          >
            Carry Forward All
          </Button>
          <Button size="small" variant="outlined" onClick={onCancel}>
            Cancel
          </Button>
        </Stack>
      }
    >
      <Typography variant="subtitle2" gutterBottom>
        Carry Forward Available
      </Typography>
      <Typography variant="body2">
        {subFormLabel} data from the most recent prior Encounter is available.
        Would you like to carry it forward into this form?
      </Typography>
    </Alert>
  );
}
