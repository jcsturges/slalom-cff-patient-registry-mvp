import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Box, Button, Paper, Typography } from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  errorId: string | null;
}

/**
 * Application-level error boundary (12-007).
 * Renders a friendly, non-technical message with a reference ID and recovery actions.
 * No PHI or stack traces are displayed to the user.
 */
export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, errorId: null };
  }

  static getDerivedStateFromError(): State {
    return {
      hasError: true,
      errorId: `ERR-${Date.now().toString(36).toUpperCase()}`,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    // Log type only — no PHI or user data in logs (12-007 AC)
    console.error('[ErrorBoundary]', error.name, errorInfo.componentStack?.split('\n')[0]);
  }

  render() {
    if (this.state.hasError) {
      return (
        <Box display="flex" alignItems="center" justifyContent="center" minHeight="60vh" px={3}>
          <Paper
            elevation={0}
            sx={{ maxWidth: 480, p: 4, textAlign: 'center', border: '1px solid', borderColor: 'divider', borderRadius: 2 }}
          >
            <ErrorOutlineIcon sx={{ fontSize: 56, color: 'error.main', mb: 2 }} />

            <Typography variant="h5" fontWeight={700} gutterBottom>
              Something went wrong
            </Typography>

            <Typography variant="body1" color="text.secondary" mb={1}>
              An unexpected error occurred. Your data has not been affected.
            </Typography>
            <Typography variant="body2" color="text.secondary" mb={3}>
              Please return to the dashboard or contact support if the problem continues.
            </Typography>

            {this.state.errorId && (
              <Typography variant="caption" color="text.disabled" display="block" mb={3}>
                Reference: {this.state.errorId}
              </Typography>
            )}

            <Box display="flex" gap={2} justifyContent="center">
              <Button variant="outlined" onClick={() => this.setState({ hasError: false, errorId: null })}>
                Try Again
              </Button>
              <Button variant="contained" onClick={() => { window.location.href = '/'; }}>
                Go to Dashboard
              </Button>
            </Box>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}
