import { useQuery } from '@tanstack/react-query';
import { useOktaAuth } from '@okta/okta-react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  Stack,
  Typography,
} from '@mui/material';
import { patientsService } from '../services/patients';
import { useRoles } from '../hooks/useRoles';
import { RoleGatedButton } from '../components/RoleGatedButton';

const PLACEHOLDER_STATS = [
  { label: 'Active Encounters', note: 'Coming soon' },
  { label: 'Pending Forms', note: 'Coming soon' },
  { label: 'Care Programs', note: 'Coming soon' },
] as const;

export function DashboardPage() {
  const { authState } = useOktaAuth();
  const navigate = useNavigate();
  const { canCreatePatient } = useRoles();

  const claims = authState?.idToken?.claims as Record<string, unknown> | undefined;
  const firstName =
    (claims?.given_name as string | undefined) ??
    (claims?.name as string | undefined) ??
    'there';

  const { data: patientCount, isLoading } = useQuery({
    queryKey: ['patients', 'count'],
    queryFn: () => patientsService.getCount(),
  });

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Welcome back, {firstName}
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        CFF Next Generation Patient Registry
      </Typography>

      <Grid container spacing={3}>
        {/* Live patient count */}
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Total Patients
              </Typography>
              {isLoading ? (
                <CircularProgress size={28} />
              ) : (
                <Typography variant="h3" fontWeight={600}>
                  {patientCount ?? '—'}
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Placeholder stat cards */}
        {PLACEHOLDER_STATS.map((stat) => (
          <Grid item xs={12} sm={6} md={3} key={stat.label}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  {stat.label}
                </Typography>
                <Typography variant="h3" fontWeight={600} color="text.disabled">
                  —
                </Typography>
                <Typography variant="caption" color="text.disabled">
                  {stat.note}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Box sx={{ mt: 4 }}>
        <Typography variant="h6" gutterBottom>
          Quick Actions
        </Typography>
        <Stack direction="row" spacing={2}>
          <Button variant="contained" onClick={() => navigate('/patients')}>
            View Patient Roster
          </Button>
          <RoleGatedButton
            variant="outlined"
            allowed={canCreatePatient}
            disabledReason="Requires ClinicalUser role or above to add patients."
            onClick={() => navigate('/patients/new')}
          >
            Add New Patient
          </RoleGatedButton>
        </Stack>
      </Box>
    </Box>
  );
}
