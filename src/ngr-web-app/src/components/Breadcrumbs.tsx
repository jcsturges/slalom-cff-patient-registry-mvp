import { useLocation, Link as RouterLink } from 'react-router-dom';
import { Breadcrumbs as MuiBreadcrumbs, Link, Typography } from '@mui/material';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';

/**
 * Map URL path segments to human-readable labels.
 * Add entries here when new routes are added.
 */
const SEGMENT_LABELS: Record<string, string> = {
  patients: 'Patients',
  programs: 'Care Programs',
  reports: 'Reporting',
  import: 'EMR Upload',
  forms: 'Forms',
  help: 'Help',
  admin: 'Administration',
  announcements: 'Announcements',
  'help-pages': 'Help Pages',
  analytics: 'User Analytics',
  'database-lock': 'Database Lock',
  'user-management': 'User Management',
  new: 'New',
  edit: 'Edit',
  contact: 'Contact Us',
};

/**
 * Auto-generates breadcrumbs from the current URL path.
 * Only renders when the path is 3+ levels deep (e.g., /patients/123/edit).
 */
export function AppBreadcrumbs() {
  const location = useLocation();
  const segments = location.pathname.split('/').filter(Boolean);

  // Only show breadcrumbs for pages 3+ levels deep
  if (segments.length < 3) return null;

  const crumbs = segments.map((segment, index) => {
    const path = '/' + segments.slice(0, index + 1).join('/');
    const label = SEGMENT_LABELS[segment] ?? (isNaN(Number(segment)) ? segment : `#${segment}`);
    const isLast = index === segments.length - 1;

    if (isLast) {
      return (
        <Typography key={path} color="text.primary" variant="body2" fontWeight={500}>
          {label}
        </Typography>
      );
    }

    return (
      <Link
        key={path}
        component={RouterLink}
        to={path}
        variant="body2"
        color="text.secondary"
        underline="hover"
      >
        {label}
      </Link>
    );
  });

  return (
    <MuiBreadcrumbs
      separator={<NavigateNextIcon fontSize="small" />}
      aria-label="Breadcrumb navigation"
      sx={{ mb: 2 }}
    >
      <Link
        component={RouterLink}
        to="/"
        variant="body2"
        color="text.secondary"
        underline="hover"
      >
        Home
      </Link>
      {crumbs}
    </MuiBreadcrumbs>
  );
}
