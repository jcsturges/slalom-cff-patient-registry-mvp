import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginCallback } from '@okta/okta-react';
import { Box, CircularProgress, Typography } from '@mui/material';
import { SecureRoute } from './components/SecureRoute';

function LoadingSpinner() {
  return (
    <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
      <CircularProgress />
    </Box>
  );
}

function Dashboard() {
  return (
    <Box p={4}>
      <Typography variant="h4" gutterBottom>
        NGR — Next Generation Patient Registry
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Authenticated. Application routes will be built here.
      </Typography>
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
        <Route path="/" element={<Dashboard />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
