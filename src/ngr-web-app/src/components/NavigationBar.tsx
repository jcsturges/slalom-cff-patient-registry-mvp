import { useState, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Button,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Menu,
  MenuItem,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import { useRoles } from '../hooks/useRoles';
import { getNavItems, type NavItem } from '../config/navigation';

interface DropdownState {
  anchorEl: HTMLElement | null;
  items: NavItem[];
}

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
  const [dropdown, setDropdown] = useState<DropdownState>({ anchorEl: null, items: [] });
  const dropdownTimeout = useRef<ReturnType<typeof setTimeout>>();

  const navItems = getNavItems(roles);

  const isActive = (item: NavItem): boolean => {
    if (item.path === '/') return location.pathname === '/';
    return location.pathname.startsWith(item.path);
  };

  const handleNavClick = (item: NavItem) => {
    if (item.children && item.children.length > 0) return; // Handled by dropdown
    navigate(item.path);
    setMobileOpen(false);
  };

  const handleDropdownOpen = (e: React.MouseEvent<HTMLElement>, item: NavItem) => {
    if (item.children && item.children.length > 0) {
      clearTimeout(dropdownTimeout.current);
      setDropdown({ anchorEl: e.currentTarget, items: item.children });
    }
  };

  const handleDropdownClose = () => {
    dropdownTimeout.current = setTimeout(() => {
      setDropdown({ anchorEl: null, items: [] });
    }, 150);
  };

  const handleDropdownEnter = () => {
    clearTimeout(dropdownTimeout.current);
  };

  const handleDropdownItemClick = (item: NavItem) => {
    navigate(item.path);
    setDropdown({ anchorEl: null, items: [] });
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
        alignItems: 'center',
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
        <Box key={item.label} sx={{ position: 'relative' }}>
          <Button
            color="inherit"
            size="small"
            onClick={() => handleNavClick(item)}
            onMouseEnter={(e) => handleDropdownOpen(e, item)}
            onMouseLeave={handleDropdownClose}
            endIcon={item.children ? <ArrowDropDownIcon /> : undefined}
            aria-haspopup={item.children ? 'true' : undefined}
            aria-expanded={item.children && dropdown.anchorEl ? 'true' : undefined}
            sx={{
              color: 'white',
              px: 2,
              py: 1,
              fontWeight: isActive(item) ? 700 : 400,
              borderBottom: isActive(item) ? '2px solid #F5A623' : '2px solid transparent',
              borderRadius: 0,
              '&:hover': {
                bgcolor: 'rgba(255,255,255,0.1)',
              },
            }}
          >
            {item.label}
          </Button>
        </Box>
      ))}

      {/* ── Context help icon ─────────────────────────────────── */}
      <Box sx={{ flexGrow: 1 }} />
      <IconButton
        size="small"
        onClick={onHelpClick}
        aria-label="Open context help"
        sx={{ color: 'white' }}
      >
        <HelpOutlineIcon />
      </IconButton>

      {/* ── Dropdown menu ─────────────────────────────────────── */}
      <Menu
        anchorEl={dropdown.anchorEl}
        open={Boolean(dropdown.anchorEl)}
        onClose={() => setDropdown({ anchorEl: null, items: [] })}
        MenuListProps={{
          onMouseEnter: handleDropdownEnter,
          onMouseLeave: handleDropdownClose,
        }}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        transformOrigin={{ vertical: 'top', horizontal: 'left' }}
      >
        {dropdown.items.map((child) => (
          <MenuItem
            key={child.path}
            onClick={() => handleDropdownItemClick(child)}
            selected={location.pathname === child.path}
          >
            {child.label}
          </MenuItem>
        ))}
      </Menu>
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
                  onClick={() => handleDropdownItemClick(child)}
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
