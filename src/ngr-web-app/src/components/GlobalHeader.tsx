import { useOktaAuth } from '@okta/okta-react';
import {
  AppBar,
  Box,
  Button,
  FormControl,
  MenuItem,
  Select,
  type SelectChangeEvent,
  Toolbar,
  Typography,
} from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import LoginIcon from '@mui/icons-material/Login';
import { useProgram } from '../contexts/ProgramContext';

const CFF_LOGO_ALT = 'Cystic Fibrosis Foundation';

export function GlobalHeader() {
  const { oktaAuth, authState } = useOktaAuth();
  const { programs, selectedProgram, selectProgram } = useProgram();
  const isAuthenticated = authState?.isAuthenticated ?? false;

  const claims = authState?.idToken?.claims as Record<string, unknown> | undefined;
  const firstName = (claims?.given_name as string | undefined) ?? '';
  const lastName = (claims?.family_name as string | undefined) ?? '';
  const displayName = [firstName, lastName].filter(Boolean).join(' ');

  const handleLogin = () => {
    void oktaAuth.signInWithRedirect({ originalUri: '/' });
  };

  const handleLogout = () => {
    void oktaAuth.signOut();
  };

  const handleProgramChange = (e: SelectChangeEvent<number>) => {
    selectProgram(Number(e.target.value));
  };

  return (
    <AppBar
      position="fixed"
      sx={{ zIndex: (t) => t.zIndex.drawer + 2 }}
      role="banner"
    >
      <Toolbar sx={{ gap: 2 }}>
        {/* ── CFF Logo ─────────────────────────────────────────── */}
        <Box
          component={isAuthenticated ? 'a' : 'div'}
          {...(isAuthenticated ? { href: 'https://www.cff.org', target: '_blank', rel: 'noopener noreferrer' } : {})}
          sx={{
            display: 'flex',
            alignItems: 'center',
            textDecoration: 'none',
            color: 'inherit',
            mr: 2,
          }}
          aria-label={isAuthenticated ? 'Cystic Fibrosis Foundation website (opens in new tab)' : CFF_LOGO_ALT}
        >
          <Typography
            variant="h6"
            noWrap
            sx={{ fontWeight: 700, letterSpacing: '-0.02em' }}
          >
            CFF Registry
          </Typography>
        </Box>

        {/* ── Program selector (authenticated, multi-program users) ── */}
        {isAuthenticated && programs.length > 1 && selectedProgram && (
          <FormControl
            size="small"
            sx={{
              minWidth: 200,
              '& .MuiOutlinedInput-root': {
                color: 'white',
                '& fieldset': { borderColor: 'rgba(255,255,255,0.4)' },
                '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.7)' },
              },
              '& .MuiSelect-icon': { color: 'white' },
            }}
          >
            <Select
              value={selectedProgram.programId}
              onChange={handleProgramChange}
              aria-label="Select care program"
              renderValue={(val) => {
                const p = programs.find((pr) => pr.programId === val);
                return p ? `${p.name} (${p.programId})` : '';
              }}
            >
              {programs.map((p) => (
                <MenuItem key={p.programId} value={p.programId}>
                  {p.name} ({p.programId})
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}

        {/* ── Single-program display ──────────────────────────── */}
        {isAuthenticated && programs.length === 1 && selectedProgram && (
          <Typography variant="body2" sx={{ opacity: 0.9 }}>
            {selectedProgram.name} ({selectedProgram.programId})
          </Typography>
        )}

        {/* ── Spacer ──────────────────────────────────────────── */}
        <Box sx={{ flexGrow: 1 }} />

        {/* ── User info + auth button ─────────────────────────── */}
        {isAuthenticated ? (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="body2" sx={{ fontWeight: 500 }}>
              {displayName}
            </Typography>
            <Button
              color="inherit"
              variant="outlined"
              size="small"
              startIcon={<LogoutIcon />}
              onClick={handleLogout}
              sx={{
                borderColor: 'rgba(255,255,255,0.5)',
                '&:hover': { borderColor: 'white', bgcolor: 'rgba(255,255,255,0.1)' },
              }}
            >
              Log Out
            </Button>
          </Box>
        ) : (
          <Button
            color="inherit"
            variant="outlined"
            startIcon={<LoginIcon />}
            onClick={handleLogin}
            sx={{
              borderColor: 'rgba(255,255,255,0.5)',
              '&:hover': { borderColor: 'white', bgcolor: 'rgba(255,255,255,0.1)' },
            }}
          >
            Log In
          </Button>
        )}
      </Toolbar>
    </AppBar>
  );
}
