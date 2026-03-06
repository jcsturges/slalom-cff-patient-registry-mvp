import { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from '@mui/material';
import { useImpersonation } from '../contexts/ImpersonationContext';

function formatCountdown(seconds: number): string {
  const m = Math.floor(seconds / 60).toString().padStart(2, '0');
  const s = (seconds % 60).toString().padStart(2, '0');
  return `${m}:${s}`;
}

export function ImpersonationBanner() {
  const { isImpersonating, targetUser, secondsRemaining, sessionExpired, endImpersonation } =
    useImpersonation();

  const [confirmOpen, setConfirmOpen] = useState(false);

  if (!isImpersonating) return null;

  const handleExitConfirm = async () => {
    setConfirmOpen(false);
    await endImpersonation('Manual');
  };

  return (
    <>
      {/* Sticky banner — sits above the GlobalHeader */}
      <Box
        role="status"
        aria-live="polite"
        sx={{
          position: 'sticky',
          top: 0,
          zIndex: 9999,
          bgcolor: sessionExpired ? 'error.main' : 'warning.main',
          color: 'warning.contrastText',
          px: 3,
          py: 0.75,
          display: 'flex',
          alignItems: 'center',
          gap: 2,
          minHeight: 44,
        }}
      >
        <Typography variant="body2" fontWeight={600} sx={{ flexGrow: 1 }}>
          {sessionExpired
            ? 'Impersonation session has expired. Returning to admin context...'
            : `You are impersonating ${targetUser?.name ?? ''} (${targetUser?.groups?.filter(g => g !== 'Everyone').join(', ') ?? ''})`}
        </Typography>

        {!sessionExpired && (
          <Typography
            variant="body2"
            aria-hidden="true"
            sx={{ fontVariantNumeric: 'tabular-nums', opacity: 0.85 }}
          >
            Session expires in {formatCountdown(secondsRemaining)}
          </Typography>
        )}

        <Button
          size="small"
          variant="outlined"
          aria-label="Exit impersonation"
          onClick={() => setConfirmOpen(true)}
          sx={{
            color: 'warning.contrastText',
            borderColor: 'warning.contrastText',
            '&:hover': { bgcolor: 'rgba(0,0,0,0.1)' },
            flexShrink: 0,
          }}
        >
          Exit Impersonation
        </Button>
      </Box>

      {/* Confirmation dialog */}
      <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Exit Impersonation?</DialogTitle>
        <DialogContent>
          <Typography>
            Return to your Foundation Admin session? You will regain full admin privileges.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmOpen(false)}>Stay</Button>
          <Button variant="contained" color="warning" onClick={handleExitConfirm}>
            Exit
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
