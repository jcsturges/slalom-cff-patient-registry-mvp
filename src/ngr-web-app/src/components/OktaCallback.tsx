import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useOktaAuth } from '@okta/okta-react';
import { Box, Button, CircularProgress, Typography } from '@mui/material';

/**
 * Custom Okta callback handler.
 * Uses low-level token.parseFromUrl() instead of handleLoginRedirect()
 * to avoid the internal restoreOriginalUri race condition.
 */
export function OktaCallback() {
  const { oktaAuth } = useOktaAuth();
  const navigate = useNavigate();
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');
  const [error, setError] = useState<string>('');

  useEffect(() => {
    let cancelled = false;

    const handleCallback = async () => {
      try {
        // Check for Okta error params first
        const params = new URLSearchParams(window.location.search);
        const errorParam = params.get('error');
        const errorDesc = params.get('error_description');

        if (errorParam) {
          setStatus('error');
          setError(`Okta error: ${errorParam} — ${errorDesc ?? ''}`);
          return;
        }

        // Use low-level parseFromUrl to exchange code for tokens
        // without triggering restoreOriginalUri
        const { tokens } = await oktaAuth.token.parseFromUrl();

        if (cancelled) return;

        // Store tokens in the token manager
        oktaAuth.tokenManager.setTokens(tokens);

        // Verify storage worked
        const accessToken = oktaAuth.getAccessToken();
        if (!accessToken) {
          setStatus('error');
          setError('Tokens were received but could not be stored.');
          return;
        }

        setStatus('success');

        // Navigate to dashboard after tokens are confirmed stored
        setTimeout(() => {
          if (!cancelled) navigate('/', { replace: true });
        }, 300);
      } catch (err) {
        if (!cancelled) {
          setStatus('error');
          setError(err instanceof Error ? err.message : String(err));
        }
      }
    };

    void handleCallback();
    return () => { cancelled = true; };
  }, [oktaAuth, navigate]);

  return (
    <Box display="flex" flexDirection="column" justifyContent="center" alignItems="center" minHeight="100vh" gap={2} px={4}>
      {status === 'processing' && (
        <>
          <CircularProgress />
          <Typography>Processing login...</Typography>
        </>
      )}
      {status === 'success' && (
        <>
          <Typography color="success.main">Login successful! Redirecting...</Typography>
          <Button variant="contained" onClick={() => navigate('/', { replace: true })}>
            Go to Dashboard
          </Button>
        </>
      )}
      {status === 'error' && (
        <>
          <Typography variant="h6" color="error.main">Login Failed</Typography>
          <Typography variant="body2" sx={{ maxWidth: 600, textAlign: 'center', wordBreak: 'break-word' }}>
            {error}
          </Typography>
          <Button variant="outlined" onClick={() => window.location.assign('/')}>
            Try Again
          </Button>
        </>
      )}
    </Box>
  );
}
