import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginCallback } from '@okta/okta-react';
import { Box, CircularProgress } from '@mui/material';
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
import { ReportingPage } from './pages/reports/ReportingPage';
import { ReportBuilderPage } from './pages/reports/ReportBuilderPage';
import { PreDefinedReportPage } from './pages/reports/PreDefinedReportPage';
import { DataExportPage } from './pages/DataExportPage';
import { EmrUploadPage } from './pages/EmrUploadPage';
import { DatabaseLockPage } from './pages/admin/DatabaseLockPage';
import { UserManagementPage } from './pages/admin/UserManagementPage';
import { MonitoringPage } from './pages/admin/MonitoringPage';
import { AuthGate } from './components/AuthGate';

export function AppRoutes() {
  return (
    <MobileGuard>
      <Routes>
        {/* ── Public routes (no auth required) ──────────────────── */}
        <Route
          path="/login/callback"
          element={
            <LoginCallback
              loadingElement={
                <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
                  <CircularProgress />
                </Box>
              }
            />
          }
        />

        {/* Unauthenticated home page / authenticated redirect */}
        <Route path="/home" element={<AuthGate unauthComponent={<HomePage />} />} />

        {/* ── Authenticated routes (Layout handles auth check) ── */}
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
          <Route path="/reports" element={<ReportingPage />} />
          <Route path="/reports/builder" element={<ReportBuilderPage />} />
          <Route path="/reports/incomplete-records" element={<PreDefinedReportPage reportKind="incomplete_records" />} />
          <Route path="/reports/patients-due-visit" element={<PreDefinedReportPage reportKind="patients_due_visit" />} />
          <Route path="/reports/diabetes-testing" element={<PreDefinedReportPage reportKind="diabetes_testing" />} />
          <Route path="/reports/admin" element={<PreDefinedReportPage reportKind="program_list" />} />
          <Route path="/reports/audit" element={<PreDefinedReportPage reportKind="user_management_audit" />} />
          <Route path="/export" element={<DataExportPage />} />
          <Route path="/import" element={<EmrUploadPage />} />
          <Route path="/help" element={<PlaceholderPage title="Help" />} />
          <Route path="/user-management" element={<UserManagementPage />} />
          <Route path="/contact" element={<PlaceholderPage title="Contact Us" />} />

          {/* ── Foundation Admin routes ───────────────────────── */}
          <Route path="/admin/patient-search" element={<PatientSearchPage />} />
          <Route path="/admin/announcements" element={<AnnouncementManagerPage />} />
          <Route path="/admin/help-pages" element={<HelpPageManagerPage />} />
          <Route path="/admin/analytics" element={<PlaceholderPage title="User Analytics" />} />
          <Route path="/admin/monitoring" element={<MonitoringPage />} />
          <Route path="/admin/database-lock" element={<DatabaseLockPage />} />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </MobileGuard>
  );
}
