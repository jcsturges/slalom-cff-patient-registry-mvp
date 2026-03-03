import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginCallback } from '@okta/okta-react';
import { Box, CircularProgress } from '@mui/material';
import { SecureRoute } from './components/SecureRoute';
import { Layout } from './components/Layout';
import { MobileGuard } from './components/MobileGuard';
import { HomePage } from './pages/HomePage';
import { DashboardPage } from './pages/DashboardPage';
import { PlaceholderPage } from './pages/PlaceholderPage';
import { PatientListPage } from './pages/patients/PatientListPage';
import { PatientDetailPage } from './pages/patients/PatientDetailPage';
import { PatientFormPage } from './pages/patients/PatientFormPage';
import { MergePatientsPage } from './pages/patients/MergePatientsPage';
import { ProgramListPage } from './pages/programs/ProgramListPage';
import { ProgramFormPage } from './pages/programs/ProgramFormPage';
import { AnnouncementManagerPage } from './pages/admin/AnnouncementManagerPage';
import { HelpPageManagerPage } from './pages/admin/HelpPageManagerPage';
import { PatientSearchPage } from './pages/admin/PatientSearchPage';
import { FormEditPage } from './pages/forms/FormEditPage';
import { AuthGate } from './components/AuthGate';

function LoadingSpinner() {
  return (
    <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
      <CircularProgress />
    </Box>
  );
}

export function AppRoutes() {
  return (
    <MobileGuard>
      <Routes>
        {/* ── Public routes ────────────────────────────────────── */}
        <Route
          path="/login/callback"
          element={<LoginCallback loadingElement={<LoadingSpinner />} />}
        />

        {/* Unauthenticated home page / authenticated redirect */}
        <Route path="/home" element={<AuthGate unauthComponent={<HomePage />} />} />

        {/* ── Authenticated routes ─────────────────────────────── */}
        <Route element={<SecureRoute />}>
          <Route element={<Layout />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/patients" element={<PatientListPage />} />
            <Route path="/patients/new" element={<PatientFormPage />} />
            <Route path="/patients/:id" element={<PatientDetailPage />} />
            <Route path="/patients/:id/edit" element={<PatientFormPage />} />
            <Route path="/patients/merge" element={<MergePatientsPage />} />
            <Route path="/patients/:patientId/forms/:formId" element={<FormEditPage />} />
            <Route path="/programs" element={<ProgramListPage />} />
            <Route path="/programs/new" element={<ProgramFormPage />} />
            <Route path="/programs/:id/edit" element={<ProgramFormPage />} />
            <Route path="/forms" element={<PlaceholderPage title="Forms" />} />
            <Route path="/reports" element={<PlaceholderPage title="Reporting" />} />
            <Route path="/import" element={<PlaceholderPage title="EMR Upload" />} />
            <Route path="/help" element={<PlaceholderPage title="Help" />} />
            <Route path="/user-management" element={<PlaceholderPage title="User Management" />} />
            <Route path="/contact" element={<PlaceholderPage title="Contact Us" />} />

            {/* ── Foundation Admin routes ───────────────────────── */}
            <Route path="/admin/patient-search" element={<PatientSearchPage />} />
            <Route path="/admin/announcements" element={<AnnouncementManagerPage />} />
            <Route path="/admin/help-pages" element={<HelpPageManagerPage />} />
            <Route path="/admin/analytics" element={<PlaceholderPage title="User Analytics" />} />
            <Route path="/admin/database-lock" element={<PlaceholderPage title="Database Lock" />} />

            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </MobileGuard>
  );
}
