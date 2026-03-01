import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginCallback } from '@okta/okta-react';
import { Box, CircularProgress } from '@mui/material';
import { SecureRoute } from './components/SecureRoute';
import { Layout } from './components/Layout';
import { DashboardPage } from './pages/DashboardPage';
import { PlaceholderPage } from './pages/PlaceholderPage';
import { PatientListPage } from './pages/patients/PatientListPage';
import { PatientDetailPage } from './pages/patients/PatientDetailPage';
import { PatientFormPage } from './pages/patients/PatientFormPage';
import { ProgramListPage } from './pages/programs/ProgramListPage';
import { ProgramFormPage } from './pages/programs/ProgramFormPage';

function LoadingSpinner() {
  return (
    <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
      <CircularProgress />
    </Box>
  );
}

export function AppRoutes() {
  return (
    <Routes>
      <Route
        path="/login/callback"
        element={<LoginCallback loadingElement={<LoadingSpinner />} />}
      />
      <Route element={<SecureRoute />}>
        <Route element={<Layout />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/patients" element={<PatientListPage />} />
          <Route path="/patients/new" element={<PatientFormPage />} />
          <Route path="/patients/:id" element={<PatientDetailPage />} />
          <Route path="/patients/:id/edit" element={<PatientFormPage />} />
          <Route path="/programs" element={<ProgramListPage />} />
          <Route path="/programs/new" element={<ProgramFormPage />} />
          <Route path="/programs/:id/edit" element={<ProgramFormPage />} />
          <Route path="/forms" element={<PlaceholderPage title="Forms" />} />
          <Route path="/reports" element={<PlaceholderPage title="Reports" />} />
          <Route path="/import" element={<PlaceholderPage title="Data Import" />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Route>
    </Routes>
  );
}
