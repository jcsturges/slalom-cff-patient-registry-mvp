import { useOktaAuth } from '@okta/okta-react';
import { Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';

interface AuthGateProps {
  /** Component to render when user is NOT authenticated */
  unauthComponent: React.ReactNode;
  /** Where to redirect when authenticated (default: /) */
  authRedirect?: string;
}

/**
 * Renders different content based on authentication state.
 * - Not authenticated → shows unauthComponent
 * - Authenticated → redirects to authRedirect
 */
export function AuthGate({ unauthComponent, authRedirect = '/' }: AuthGateProps) {
  const { authState } = useOktaAuth();

  if (!authState) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  if (authState.isAuthenticated) {
    return <Navigate to={authRedirect} replace />;
  }

  return <>{unauthComponent}</>;
}
