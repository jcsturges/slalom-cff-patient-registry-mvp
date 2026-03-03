import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Button,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import { useRoles } from '../hooks/useRoles';
import { getNavItems, type NavItem } from '../config/navigation';

interface NavigationBarProps {
  onHelpClick?: () => void;
}

export function NavigationBar({ onHelpClick }: NavigationBarProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const roles = useRoles();
  const theme = useTheme();
  const isTablet = useMediaQuery(theme.breakpoints.down('lg'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const [openItem, setOpenItem] = useState<string | null>(null);

  const navItems = getNavItems(roles);

  const isActive = (item: NavItem): boolean => {
    if (item.path === '/') return location.pathname === '/';
    if (item.children) {
      return item.children.some((child) => location.pathname.startsWith(child.path));
    }
    return location.pathname.startsWith(item.path);
  };

  const handleNavClick = (item: NavItem) => {
    if (item.children && item.children.length > 0) return;
    navigate(item.path);
    setMobileOpen(false);
  };

  const handleChildClick = (item: NavItem) => {
    navigate(item.path);
    setOpenItem(null);
    setMobileOpen(false);
  };

  // ── Desktop horizontal nav ─────────────────────────────────────
  const desktopNav = (
    <Box
      component="nav"
      role="navigation"
      aria-label="Primary navigation"
      sx={{
        display: 'flex',
        alignItems: 'stretch',
        bgcolor: 'primary.light',
        px: 2,
        minHeight: 44,
        position: 'fixed',
        top: 64,
        left: 0,
        right: 0,
        zIndex: (t) => t.zIndex.drawer + 1,
      }}
    >
      {navItems.map((item) => (
        <Box
          key={item.label}
          sx={{ position: 'relative', display: 'flex', alignItems: 'stretch' }}
          onMouseEnter={() => item.children && setOpenItem(item.label)}
          onMouseLeave={() => setOpenItem(null)}
        >
          <Button
            color="inherit"
            size="small"
            onClick={() => handleNavClick(item)}
            endIcon={item.children ? <ArrowDropDownIcon /> : undefined}
            aria-haspopup={item.children ? 'true' : undefined}
            aria-expanded={item.children ? openItem === item.label : undefined}
            sx={{
              color: 'white',
              px: 1.5,
              py: 0,
              height: '100%',
              fontSize: '0.8125rem',
              fontWeight: isActive(item) ? 700 : 400,
              borderBottom: isActive(item) ? '2px solid #F5A623' : '2px solid transparent',
              borderRadius: 0,
              '&:hover': { bgcolor: 'rgba(255,255,255,0.1)' },
            }}
          >
            {item.label}
          </Button>

          {item.children && openItem === item.label && (
            <Paper
              elevation={4}
              sx={{
                position: 'absolute',
                top: '100%',
                left: 0,
                minWidth: 200,
                zIndex: (t) => t.zIndex.drawer + 2,
                py: 0.5,
              }}
            >
              {item.children.map((child) => (
                <Box
                  key={child.path}
                  onClick={() => handleChildClick(child)}
                  sx={{
                    px: 2,
                    py: 1,
                    cursor: 'pointer',
                    fontSize: '0.875rem',
                    fontWeight: location.pathname === child.path ? 600 : 400,
                    color: location.pathname === child.path ? 'primary.main' : 'text.primary',
                    bgcolor: location.pathname === child.path ? 'action.selected' : 'transparent',
                    '&:hover': { bgcolor: 'action.hover' },
                  }}
                >
                  {child.label}
                </Box>
              ))}
            </Paper>
          )}
        </Box>
      ))}

      {/* ── Context help icon ─────────────────────────────────── */}
      <Box sx={{ flexGrow: 1 }} />
      <IconButton
        size="small"
        onClick={onHelpClick}
        aria-label="Open context help"
        sx={{ color: 'white', alignSelf: 'center' }}
      >
        <HelpOutlineIcon />
      </IconButton>
    </Box>
  );

  // ── Tablet hamburger nav ───────────────────────────────────────
  const tabletNav = (
    <>
      <Box
        sx={{
          bgcolor: 'primary.light',
          px: 2,
          display: 'flex',
          alignItems: 'center',
          minHeight: 44,
          position: 'fixed',
          top: 64,
          left: 0,
          right: 0,
          zIndex: (t) => t.zIndex.drawer + 1,
        }}
      >
        <IconButton
          color="inherit"
          onClick={() => setMobileOpen(true)}
          aria-label="Open navigation menu"
          sx={{ color: 'white' }}
        >
          <MenuIcon />
        </IconButton>

        <Box sx={{ flexGrow: 1 }} />
        <IconButton
          size="small"
          onClick={onHelpClick}
          aria-label="Open context help"
          sx={{ color: 'white' }}
        >
          <HelpOutlineIcon />
        </IconButton>
      </Box>

      <Drawer
        anchor="left"
        open={mobileOpen}
        onClose={() => setMobileOpen(false)}
        sx={{ '& .MuiDrawer-paper': { width: 280, pt: '108px' } }}
      >
        <List component="nav" aria-label="Primary navigation">
          {navItems.map((item) => (
            <Box key={item.label}>
              <ListItemButton
                selected={isActive(item)}
                onClick={() => handleNavClick(item)}
                sx={{ py: 1.5, px: 3 }}
              >
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{ fontWeight: isActive(item) ? 600 : 400 }}
                />
              </ListItemButton>
              {item.children?.map((child) => (
                <ListItemButton
                  key={child.path}
                  selected={location.pathname === child.path}
                  onClick={() => handleChildClick(child)}
                  sx={{ py: 1, pl: 5 }}
                >
                  <ListItemText
                    primary={child.label}
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItemButton>
              ))}
            </Box>
          ))}
        </List>
      </Drawer>
    </>
  );

  return isTablet ? tabletNav : desktopNav;
}
