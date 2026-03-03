import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Grid,
  Typography,
} from '@mui/material';
import AssessmentIcon from '@mui/icons-material/Assessment';
import BuildIcon from '@mui/icons-material/Build';
import WarningIcon from '@mui/icons-material/Warning';
import EventIcon from '@mui/icons-material/Event';
import LocalHospitalIcon from '@mui/icons-material/LocalHospital';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import HistoryIcon from '@mui/icons-material/History';
import { useRoles } from '../../hooks/useRoles';

interface ReportCard {
  title: string;
  description: string;
  path: string;
  icon: React.ReactNode;
  adminOnly?: boolean;
}

const CP_REPORTS: ReportCard[] = [
  {
    title: 'Report Builder',
    description: 'Build custom reports with include/exclude patient selection, row/column customization, and saved queries.',
    path: '/reports/builder',
    icon: <BuildIcon fontSize="large" />,
  },
  {
    title: 'Incomplete Records',
    description: 'View incomplete unlocked forms for the current reporting year to track data entry progress.',
    path: '/reports/incomplete-records',
    icon: <WarningIcon fontSize="large" />,
  },
  {
    title: 'Patients Due Visit',
    description: 'Identify patients who are overdue for a visit based on last encounter date.',
    path: '/reports/patients-due-visit',
    icon: <EventIcon fontSize="large" />,
  },
  {
    title: 'Diabetes Testing',
    description: 'Identify patients ≥10 years who need diabetes testing based on OGTT results.',
    path: '/reports/diabetes-testing',
    icon: <LocalHospitalIcon fontSize="large" />,
  },
];

const ADMIN_REPORTS: ReportCard[] = [
  {
    title: 'Administrative Reports',
    description: 'Program list, merge report, EMR uploads, patient transfers, file uploads.',
    path: '/reports/admin',
    icon: <AdminPanelSettingsIcon fontSize="large" />,
    adminOnly: true,
  },
  {
    title: 'Audit Reports',
    description: 'User management events and download activity audit trails.',
    path: '/reports/audit',
    icon: <HistoryIcon fontSize="large" />,
    adminOnly: true,
  },
];

export function ReportingPage() {
  const navigate = useNavigate();
  const { isFoundationAdmin } = useRoles();

  const allReports = isFoundationAdmin ? [...CP_REPORTS, ...ADMIN_REPORTS] : CP_REPORTS;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
        <AssessmentIcon color="primary" />
        <Typography variant="h4">Reporting</Typography>
      </Box>

      <Grid container spacing={3}>
        {allReports.map((report) => (
          <Grid item xs={12} sm={6} md={4} key={report.path}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea onClick={() => navigate(report.path)} sx={{ height: '100%', p: 1 }}>
                <CardContent>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, color: 'primary.main' }}>
                    {report.icon}
                    <Typography variant="h6" fontWeight={600}>{report.title}</Typography>
                  </Box>
                  <Typography variant="body2" color="text.secondary">
                    {report.description}
                  </Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
