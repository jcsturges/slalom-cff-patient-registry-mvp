import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Grid,
  Typography,
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import { apiGet } from '../../services/api';

interface MonitoringMetrics {
  uptime: { serverStartedAt: string; uptimeHours: number };
  database: { healthy: boolean; latencyMs: number };
  counts: { patients: number; users: number };
  last24h: { auditEvents: number; reportFailures: number };
  applicationInsights: { note: string };
  generatedAt: string;
}

function MetricCard({ label, value, sub }: { label: string; value: string | number; sub?: string }) {
  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="caption" color="text.secondary" textTransform="uppercase" letterSpacing={1}>
          {label}
        </Typography>
        <Typography variant="h4" fontWeight={700} mt={0.5}>
          {value}
        </Typography>
        {sub && (
          <Typography variant="body2" color="text.secondary" mt={0.25}>
            {sub}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}

export function MonitoringPage() {
  const { data, isLoading, error, dataUpdatedAt } = useQuery<MonitoringMetrics>({
    queryKey: ['admin', 'monitoring'],
    queryFn: () => apiGet('/api/admin/monitoring'),
    refetchInterval: 30_000, // refresh every 30 s
  });

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" py={6}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !data) {
    return (
      <Alert severity="error">
        Unable to load monitoring metrics. Ensure you have Foundation Admin access.
      </Alert>
    );
  }

  const uptimeHours = data.uptime.uptimeHours.toFixed(1);
  const dbStatus = data.database.healthy;
  const lastUpdated = new Date(dataUpdatedAt).toLocaleTimeString();

  return (
    <Box>
      <Box display="flex" alignItems="baseline" gap={2} mb={3}>
        <Typography variant="h5" fontWeight={700}>
          System Monitoring
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Auto-refreshes every 30 s — last updated {lastUpdated}
        </Typography>
      </Box>

      {/* DB Status banner */}
      <Alert
        severity={dbStatus ? 'success' : 'error'}
        icon={dbStatus ? <CheckCircleIcon /> : <ErrorIcon />}
        sx={{ mb: 3 }}
      >
        {dbStatus
          ? `Database is healthy — round-trip ${data.database.latencyMs} ms`
          : 'Database is unreachable. Check connection string and SQL service.'}
      </Alert>

      <Grid container spacing={2} mb={4}>
        <Grid item xs={12} sm={6} md={3}>
          <MetricCard
            label="Uptime"
            value={`${uptimeHours} h`}
            sub={`Since ${new Date(data.uptime.serverStartedAt).toLocaleString()}`}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <MetricCard
            label="DB Latency"
            value={`${data.database.latencyMs} ms`}
            sub="Round-trip SELECT 1"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <MetricCard label="Total Patients" value={data.counts.patients.toLocaleString()} />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <MetricCard label="Total Users" value={data.counts.users.toLocaleString()} />
        </Grid>
      </Grid>

      <Typography variant="h6" fontWeight={600} mb={2}>
        Last 24 Hours
      </Typography>
      <Grid container spacing={2} mb={4}>
        <Grid item xs={12} sm={6} md={3}>
          <MetricCard
            label="Audit Events"
            value={data.last24h.auditEvents.toLocaleString()}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <MetricCard
            label="Report Failures"
            value={data.last24h.reportFailures}
            sub={data.last24h.reportFailures > 0 ? 'Review ReportExecutions table' : 'All reports succeeded'}
          />
        </Grid>
      </Grid>

      <Divider sx={{ mb: 3 }} />

      <Box>
        <Typography variant="h6" fontWeight={600} mb={1}>
          Application Insights
        </Typography>
        <Typography variant="body2" color="text.secondary" mb={2}>
          {data.applicationInsights.note}
        </Typography>
        <Box display="flex" gap={1} flexWrap="wrap">
          <Chip label="Error rates" variant="outlined" size="small" />
          <Chip label="Response times" variant="outlined" size="small" />
          <Chip label="Availability tests" variant="outlined" size="small" />
          <Chip label="Live Metrics Stream" variant="outlined" size="small" />
        </Box>
        <Typography variant="caption" color="text.secondary" display="block" mt={1}>
          Access via Azure Portal → Application Insights resource for the NGR deployment.
          Target uptime: 99.5%. Planned maintenance must be announced at least 7 days in advance
          via the Announcements system.
        </Typography>
      </Box>
    </Box>
  );
}
