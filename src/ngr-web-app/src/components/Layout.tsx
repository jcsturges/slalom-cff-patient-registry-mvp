import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { useOktaAuth } from '@okta/okta-react';
import { Alert, Box, Button, CircularProgress, Snackbar, Typography } from '@mui/material';
import { GlobalHeader } from './GlobalHeader';
import { NavigationBar } from './NavigationBar';
import { AppBreadcrumbs } from './Breadcrumbs';
import { SkipNavLink } from './SkipNavLink';
import { HelpModal } from './HelpModal';
import { ContactUsDialog } from './ContactUsDialog';
import { ImpersonationBanner } from './ImpersonationBanner';
import { ErrorBoundary } from './ErrorBoundary';
import { ImpersonationProvider } from '../contexts/ImpersonationContext';
import { useSessionMonitor } from '../hooks/useSessionMonitor';
import { useUserSync } from '../hooks/useUserSync';
import { useAnalytics } from '../hooks/useAnalytics';

const HEADER_HEIGHT = 64;
const NAV_HEIGHT = 44;
const TOTAL_TOP_OFFSET = HEADER_HEIGHT + NAV_HEIGHT;

export function Layout() {
  const { oktaAuth, authState } = useOktaAuth();
  const { sessionExpired } = useSessionMonitor();
  useUserSync();
  useAnalytics('Layout');

  const [helpOpen, setHelpOpen] = useState(false);
  const [contactOpen, setContactOpen] = useState(false);

  // If not authenticated, show a simple login prompt — NO automatic redirect.
  // This eliminates any possibility of a redirect loop.
  if (authState && !authState.isAuthenticated) {
    return (
      <Box display="flex" flexDirection="column" justifyContent="center" alignItems="center" minHeight="100vh" gap={3}>
        <Typography variant="h4" fontWeight={700} color="primary.main">CFF Registry</Typography>
        <Typography color="text.secondary">Please log in to continue.</Typography>
        <Button
          variant="contained"
          size="large"
          onClick={() => {
            const uri = window.location.pathname + window.location.search;
            void oktaAuth.signInWithRedirect({ originalUri: uri });
          }}
        >
          Log In
        </Button>
      </Box>
    );
  }

  if (!authState) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  // Authenticated — render the full app
  return (
    <ImpersonationProvider>
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <SkipNavLink />
      <ImpersonationBanner />
      <GlobalHeader />
      <NavigationBar onHelpClick={() => setHelpOpen(true)} />

      <Box
        component="main"
        id="main-content"
        tabIndex={-1}
        role="main"
        sx={{ flexGrow: 1, pt: `${TOTAL_TOP_OFFSET + 24}px`, px: 3, pb: 3 }}
      >
        <AppBreadcrumbs />
        <ErrorBoundary>
          <Outlet context={{ openContact: () => setContactOpen(true) }} />
        </ErrorBoundary>
      </Box>

      <HelpModal open={helpOpen} onClose={() => setHelpOpen(false)} />
      <ContactUsDialog open={contactOpen} onClose={() => setContactOpen(false)} />

      <Snackbar open={sessionExpired} anchorOrigin={{ vertical: 'top', horizontal: 'center' }}>
        <Alert severity="warning" variant="filled">
          Your session has expired. Redirecting to login...
        </Alert>
      </Snackbar>
    </Box>
    </ImpersonationProvider>
  );
}
