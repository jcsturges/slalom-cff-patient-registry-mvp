import { useOktaAuth } from '@okta/okta-react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Divider,
  Link,
  Typography,
} from '@mui/material';
import LoginIcon from '@mui/icons-material/Login';
import CampaignIcon from '@mui/icons-material/Campaign';
import { announcementsService } from '../services/announcements';

/**
 * Unauthenticated home page (03-002).
 * Shows CFF branding, NGR description, active announcements, and login button.
 */
export function HomePage() {
  const { oktaAuth } = useOktaAuth();

  // Fetch active announcements (public endpoint)
  const { data: announcements = [] } = useQuery({
    queryKey: ['announcements', 'active'],
    queryFn: () => announcementsService.getActive().catch(() => []),
  });

  const handleLogin = () => {
    void oktaAuth.signInWithRedirect({ originalUri: '/' });
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* ── Hero section ──────────────────────────────────────── */}
      <Box
        sx={{
          bgcolor: 'primary.main',
          color: 'white',
          py: 8,
          textAlign: 'center',
        }}
      >
        <Container maxWidth="md">
          <Typography
            variant="h3"
            fontWeight={700}
            gutterBottom
            sx={{ letterSpacing: '-0.02em' }}
          >
            CFF Registry
          </Typography>
          <Typography variant="h6" sx={{ opacity: 0.9, mb: 4, fontWeight: 400 }}>
            Next Generation Patient Registry
          </Typography>
          <Typography variant="body1" sx={{ opacity: 0.8, mb: 4, maxWidth: 600, mx: 'auto' }}>
            The Cystic Fibrosis Foundation&rsquo;s Next Generation Registry is a comprehensive
            patient data system designed to support CF care programs across the United States.
            It enables clinical data collection, quality improvement reporting, and care
            coordination for the CF community.
          </Typography>
          <Button
            variant="contained"
            size="large"
            startIcon={<LoginIcon />}
            onClick={handleLogin}
            sx={{
              bgcolor: 'secondary.main',
              color: 'primary.main',
              fontWeight: 600,
              px: 4,
              py: 1.5,
              '&:hover': { bgcolor: '#e09620' },
            }}
          >
            Log In
          </Button>
        </Container>
      </Box>

      {/* ── Announcements section ─────────────────────────────── */}
      {announcements.length > 0 && (
        <Container maxWidth="md" sx={{ py: 5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
            <CampaignIcon color="primary" />
            <Typography variant="h5" fontWeight={600}>
              Announcements
            </Typography>
          </Box>
          {announcements.map((ann) => (
            <Card key={ann.id} sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  {ann.title}
                </Typography>
                <Typography variant="caption" color="text.secondary" gutterBottom display="block">
                  {new Date(ann.effectiveDate).toLocaleDateString('en-US', {
                    month: 'long',
                    day: 'numeric',
                    year: 'numeric',
                    timeZone: 'America/New_York',
                  })}{' '}
                  ET
                </Typography>
                <Box
                  dangerouslySetInnerHTML={{ __html: ann.content }}
                  sx={{
                    mt: 1,
                    '& a': { color: 'primary.light' },
                    '& img': { maxWidth: '100%' },
                  }}
                />
              </CardContent>
            </Card>
          ))}
        </Container>
      )}

      {/* ── Spacer ────────────────────────────────────────────── */}
      <Box sx={{ flexGrow: 1 }} />

      {/* ── Footer ────────────────────────────────────────────── */}
      <Box
        component="footer"
        role="contentinfo"
        sx={{
          bgcolor: 'background.paper',
          borderTop: '1px solid',
          borderColor: 'divider',
          py: 3,
          textAlign: 'center',
        }}
      >
        <Container maxWidth="md">
          <Divider sx={{ mb: 2 }} />
          <Box sx={{ display: 'flex', justifyContent: 'center', gap: 4 }}>
            <Link
              href="https://www.cff.org/privacy-policy"
              target="_blank"
              rel="noopener noreferrer"
              variant="body2"
              color="text.secondary"
              underline="hover"
            >
              Privacy Policy
            </Link>
            <Link
              href="https://www.cff.org/terms-and-conditions"
              target="_blank"
              rel="noopener noreferrer"
              variant="body2"
              color="text.secondary"
              underline="hover"
            >
              Terms &amp; Conditions
            </Link>
          </Box>
          <Typography variant="caption" color="text.disabled" display="block" sx={{ mt: 2 }}>
            &copy; {new Date().getFullYear()} Cystic Fibrosis Foundation. All rights reserved.
          </Typography>
        </Container>
      </Box>
    </Box>
  );
}
