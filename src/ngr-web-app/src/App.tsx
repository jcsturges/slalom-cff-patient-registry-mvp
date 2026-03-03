import { Security } from '@okta/okta-react';
import { OktaAuth, toRelativeUrl } from '@okta/okta-auth-js';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { theme } from './theme';
import { AppRoutes } from './routes';
import { ErrorBoundary } from './components/ErrorBoundary';
import { ProgramProvider } from './contexts/ProgramContext';
import { oktaAuth } from './lib/okta';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000,
    },
  },
});

function App() {
  const restoreOriginalUri = async (_oktaAuth: OktaAuth, originalUri: string) => {
    // Use window.location instead of React Router navigate().
    // A full page load ensures the Security provider initializes fresh
    // with the tokens already in storage — eliminates the race condition
    // where authState briefly shows "not authenticated" after callback.
    const url = toRelativeUrl(originalUri || '/', window.location.origin);
    window.location.replace(url);
  };

  return (
    <ErrorBoundary>
      <Security oktaAuth={oktaAuth} restoreOriginalUri={restoreOriginalUri}>
        <QueryClientProvider client={queryClient}>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <ProgramProvider>
              <AppRoutes />
            </ProgramProvider>
          </ThemeProvider>
        </QueryClientProvider>
      </Security>
    </ErrorBoundary>
  );
}

export default App;
