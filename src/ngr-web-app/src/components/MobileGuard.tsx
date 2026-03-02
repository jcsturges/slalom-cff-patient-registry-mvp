import { Box, Typography, useMediaQuery } from '@mui/material';
import DesktopWindowsIcon from '@mui/icons-material/DesktopWindows';

/**
 * Displays a full-screen message on mobile devices (<768px) directing users to desktop.
 * Returns null on non-mobile viewports so children render normally.
 */
export function MobileGuard({ children }: { children: React.ReactNode }) {
  // 768px matches the SRS spec for mobile cutoff (below tablet)
  const isMobile = useMediaQuery('(max-width:767px)');

  if (!isMobile) return <>{children}</>;

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        px: 4,
        textAlign: 'center',
        bgcolor: 'background.default',
      }}
    >
      <DesktopWindowsIcon sx={{ fontSize: 64, color: 'primary.main', mb: 3 }} />
      <Typography variant="h5" gutterBottom fontWeight={600}>
        Desktop Required
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 400 }}>
        The CFF Next Generation Patient Registry is optimized for desktop and tablet
        devices. Please access this application from a device with a screen width of
        at least 768 pixels.
      </Typography>
      <Typography variant="body2" color="text.disabled" sx={{ mt: 3 }}>
        Minimum resolution: 1024 × 768
      </Typography>
    </Box>
  );
}
