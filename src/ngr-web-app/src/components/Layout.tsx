import { useState } from 'react';
import { useNavigate, useLocation, Outlet } from 'react-router-dom';
import { useOktaAuth } from '@okta/okta-react';
import {
  AppBar,
  Avatar,
  Box,
  Chip,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Menu,
  MenuItem,
  Toolbar,
  Typography,
} from '@mui/material';

const DRAWER_WIDTH = 220;

const NAV_ITEMS = [
  { label: 'Dashboard', path: '/' },
  { label: 'Patients', path: '/patients' },
  { label: 'Forms', path: '/forms', soon: true },
  { label: 'Reports', path: '/reports', soon: true },
  { label: 'Import', path: '/import', soon: true },
] as const;

export function Layout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { oktaAuth, authState } = useOktaAuth();
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);

  const claims = authState?.idToken?.claims as Record<string, unknown> | undefined;
  const firstName = (claims?.given_name as string | undefined) ?? '';
  const lastName = (claims?.family_name as string | undefined) ?? '';
  const email = (claims?.email as string | undefined) ?? '';
  const displayName = [firstName, lastName].filter(Boolean).join(' ') || email;
  const initials = displayName
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2) || 'U';

  const handleSignOut = () => {
    setMenuAnchor(null);
    void oktaAuth.signOut();
  };

  return (
    <Box sx={{ display: 'flex' }}>
      {/* ── Top bar ──────────────────────────────────────────────────── */}
      <AppBar position="fixed" sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
        <Toolbar>
          <Typography variant="h6" noWrap sx={{ flexGrow: 1 }}>
            NGR — Patient Registry
          </Typography>
          <IconButton onClick={(e) => setMenuAnchor(e.currentTarget)} sx={{ p: 0 }}>
            <Avatar sx={{ bgcolor: 'secondary.main', width: 34, height: 34, fontSize: '0.8rem' }}>
              {initials}
            </Avatar>
          </IconButton>
          <Menu
            anchorEl={menuAnchor}
            open={Boolean(menuAnchor)}
            onClose={() => setMenuAnchor(null)}
          >
            <MenuItem disabled>
              <Typography variant="body2" color="text.secondary">
                {displayName}
              </Typography>
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleSignOut}>Sign out</MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      {/* ── Left nav ─────────────────────────────────────────────────── */}
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' },
        }}
      >
        <Toolbar /> {/* spacer under AppBar */}
        <List disablePadding>
          {NAV_ITEMS.map((item) => {
            const selected =
              item.path === '/'
                ? location.pathname === '/'
                : location.pathname.startsWith(item.path);
            return (
              <ListItemButton
                key={item.path}
                selected={selected}
                disabled={'soon' in item && item.soon}
                onClick={() => navigate(item.path)}
                sx={{ py: 1.5, px: 3 }}
              >
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{ fontWeight: selected ? 600 : 400 }}
                />
                {'soon' in item && item.soon && (
                  <Chip
                    label="Soon"
                    size="small"
                    variant="outlined"
                    sx={{ fontSize: '0.6rem', height: 16, px: 0.5 }}
                  />
                )}
              </ListItemButton>
            );
          })}
        </List>
      </Drawer>

      {/* ── Main content ─────────────────────────────────────────────── */}
      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        <Toolbar /> {/* spacer under AppBar */}
        <Outlet />
      </Box>
    </Box>
  );
}
