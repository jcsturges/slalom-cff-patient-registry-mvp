import { useEffect, useState, useCallback } from 'react';
import { useOktaAuth } from '@okta/okta-react';

/**
 * Monitors Okta token lifecycle events. When a token expires and cannot be
 * silently renewed, sets `sessionExpired` so the UI can show a notification
 * before redirecting to login.
 */
export function useSessionMonitor() {
  const { oktaAuth } = useOktaAuth();
  const [sessionExpired, setSessionExpired] = useState(false);

  const redirectToLogin = useCallback(() => {
    // Just set the flag — the UI will show a notification.
    // Don't auto-redirect; let the user click login again.
    setSessionExpired(true);
  }, []);

  useEffect(() => {
    const tokenManager = oktaAuth.tokenManager;

    const onExpired = () => {
      // autoRenew should handle this, but if we still get the event
      // it means renewal failed — redirect to login
      redirectToLogin();
    };

    const onError = () => {
      redirectToLogin();
    };

    tokenManager.on('expired', onExpired);
    tokenManager.on('error', onError);

    return () => {
      tokenManager.off('expired', onExpired);
      tokenManager.off('error', onError);
    };
  }, [oktaAuth, redirectToLogin]);

  return { sessionExpired };
}
