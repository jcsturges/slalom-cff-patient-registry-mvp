import { Box, Link } from '@mui/material';

/**
 * Skip navigation link for screen readers and keyboard users.
 * Visually hidden until focused, then appears at top of page.
 */
export function SkipNavLink() {
  return (
    <Box
      component="div"
      sx={{
        position: 'fixed',
        top: 0,
        left: 0,
        zIndex: 9999,
      }}
    >
      <Link
        href="#main-content"
        sx={{
          position: 'absolute',
          left: '-9999px',
          top: 0,
          bgcolor: 'primary.main',
          color: 'white',
          p: 2,
          fontWeight: 600,
          textDecoration: 'none',
          zIndex: 9999,
          '&:focus': {
            left: 0,
            outline: '2px solid #F5A623',
            outlineOffset: 2,
          },
        }}
      >
        Skip to main content
      </Link>
    </Box>
  );
}
