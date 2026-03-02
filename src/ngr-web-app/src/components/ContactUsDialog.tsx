import { useState, useRef } from 'react';
import { useOktaAuth } from '@okta/okta-react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  TextField,
  Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import AttachFileIcon from '@mui/icons-material/AttachFile';
import { useMutation } from '@tanstack/react-query';
import { contactService } from '../services/contact';
import { useProgram } from '../contexts/ProgramContext';

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

interface ContactUsDialogProps {
  open: boolean;
  onClose: () => void;
}

export function ContactUsDialog({ open, onClose }: ContactUsDialogProps) {
  const { authState } = useOktaAuth();
  const { selectedProgram } = useProgram();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const claims = authState?.idToken?.claims as Record<string, unknown> | undefined;
  const name = [claims?.given_name, claims?.family_name].filter(Boolean).join(' ') || '';
  const email = (claims?.email as string) ?? '';
  const programNumber = selectedProgram ? String(selectedProgram.programId) : '';

  const [subject, setSubject] = useState('');
  const [message, setMessage] = useState('');
  const [attachment, setAttachment] = useState<File | null>(null);
  const [fileError, setFileError] = useState('');
  const [referenceId, setReferenceId] = useState('');

  const submitMutation = useMutation({
    mutationFn: () =>
      contactService.submit({
        subject,
        message,
        attachment: attachment ?? undefined,
      }),
    onSuccess: (result) => {
      setReferenceId(result.referenceId);
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > MAX_FILE_SIZE) {
      setFileError('File must be 5 MB or smaller.');
      setAttachment(null);
      return;
    }
    setFileError('');
    setAttachment(file);
  };

  const handleSubmit = () => {
    submitMutation.mutate();
  };

  const handleClose = () => {
    setSubject('');
    setMessage('');
    setAttachment(null);
    setFileError('');
    setReferenceId('');
    submitMutation.reset();
    onClose();
  };

  const canSubmit = subject.trim() && message.trim() && !submitMutation.isPending;

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="contact-dialog-title"
    >
      <DialogTitle id="contact-dialog-title" sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography variant="h6" sx={{ flexGrow: 1 }}>
          Contact Us
        </Typography>
        <IconButton onClick={handleClose} aria-label="Close" size="small">
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <Divider />
      <DialogContent>
        {referenceId ? (
          <Alert severity="success" sx={{ mt: 1 }}>
            <Typography variant="subtitle2" gutterBottom>
              Request submitted successfully!
            </Typography>
            <Typography variant="body2">
              Your reference ID is <strong>{referenceId}</strong>. A confirmation email
              has been sent to {email}. Our team will respond shortly.
            </Typography>
          </Alert>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Name"
              value={name}
              disabled
              size="small"
              fullWidth
              aria-label="Your name"
            />
            <TextField
              label="Email"
              value={email}
              disabled
              size="small"
              fullWidth
              aria-label="Your email"
            />
            <TextField
              label="Program Number"
              value={programNumber}
              disabled
              size="small"
              fullWidth
              aria-label="Your program number"
            />
            <TextField
              label="Subject"
              value={subject}
              onChange={(e) => setSubject(e.target.value)}
              required
              size="small"
              fullWidth
              aria-label="Subject (required)"
            />
            <TextField
              label="Message"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              required
              multiline
              rows={4}
              fullWidth
              aria-label="Message (required)"
            />

            {/* File attachment */}
            <Box>
              <input
                ref={fileInputRef}
                type="file"
                hidden
                onChange={handleFileChange}
                accept="*/*"
              />
              <Button
                variant="outlined"
                size="small"
                startIcon={<AttachFileIcon />}
                onClick={() => fileInputRef.current?.click()}
              >
                {attachment ? attachment.name : 'Attach File (optional, max 5 MB)'}
              </Button>
              {fileError && (
                <Typography variant="caption" color="error" display="block" sx={{ mt: 0.5 }}>
                  {fileError}
                </Typography>
              )}
            </Box>

            {submitMutation.isError && (
              <Alert severity="error">
                {(submitMutation.error as Error)?.message || 'Failed to submit. Please try again.'}
              </Alert>
            )}
          </Box>
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        {referenceId ? (
          <Button onClick={handleClose} variant="contained">
            Close
          </Button>
        ) : (
          <>
            <Button onClick={handleClose}>Cancel</Button>
            <Button
              variant="contained"
              onClick={handleSubmit}
              disabled={!canSubmit}
              startIcon={submitMutation.isPending ? <CircularProgress size={16} /> : undefined}
            >
              Send
            </Button>
          </>
        )}
      </DialogActions>
    </Dialog>
  );
}
