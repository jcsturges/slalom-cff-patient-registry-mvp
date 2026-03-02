import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Alert, Box, Snackbar } from '@mui/material';
import { GlobalHeader } from './GlobalHeader';
import { NavigationBar } from './NavigationBar';
import { AppBreadcrumbs } from './Breadcrumbs';
import { SkipNavLink } from './SkipNavLink';
import { HelpModal } from './HelpModal';
import { ContactUsDialog } from './ContactUsDialog';
import { useSessionMonitor } from '../hooks/useSessionMonitor';
import { useUserSync } from '../hooks/useUserSync';

/** Height of the AppBar + NavigationBar combined */
const HEADER_HEIGHT = 64;
const NAV_HEIGHT = 44;
const TOTAL_TOP_OFFSET = HEADER_HEIGHT + NAV_HEIGHT;

export function Layout() {
  const { sessionExpired } = useSessionMonitor();
  useUserSync();

  const [helpOpen, setHelpOpen] = useState(false);
  const [contactOpen, setContactOpen] = useState(false);

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <SkipNavLink />

      {/* ── Global Header ──────────────────────────────────────── */}
      <GlobalHeader />

      {/* ── Navigation Bar ─────────────────────────────────────── */}
      <NavigationBar onHelpClick={() => setHelpOpen(true)} />

      {/* ── Main Content ───────────────────────────────────────── */}
      <Box
        component="main"
        id="main-content"
        tabIndex={-1}
        role="main"
        sx={{
          flexGrow: 1,
          pt: `${TOTAL_TOP_OFFSET + 24}px`,
          px: 3,
          pb: 3,
        }}
      >
        <AppBreadcrumbs />
        <Outlet context={{ openContact: () => setContactOpen(true) }} />
      </Box>

      {/* ── Help Modal ─────────────────────────────────────────── */}
      <HelpModal open={helpOpen} onClose={() => setHelpOpen(false)} />

      {/* ── Contact Us Dialog ──────────────────────────────────── */}
      <ContactUsDialog open={contactOpen} onClose={() => setContactOpen(false)} />

      {/* ── Session expiry notification ────────────────────────── */}
      <Snackbar open={sessionExpired} anchorOrigin={{ vertical: 'top', horizontal: 'center' }}>
        <Alert severity="warning" variant="filled">
          Your session has expired. Redirecting to login...
        </Alert>
      </Snackbar>
    </Box>
  );
}
