import { useEffect, useRef } from 'react';
import { useOktaAuth } from '@okta/okta-react';
import { apiPost } from '../services/api';

/**
 * Calls POST /api/auth/sync once after login to upsert the user record
 * in the local database from JWT claims.
 */
export function useUserSync() {
  const { authState } = useOktaAuth();
  const hasSynced = useRef(false);

  useEffect(() => {
    if (!authState?.isAuthenticated || hasSynced.current) return;

    hasSynced.current = true;
    apiPost('/api/auth/sync', {}).catch((err) => {
      // Non-critical — log and continue. The user can still use the app.
      console.warn('User sync failed:', err);
    });
  }, [authState?.isAuthenticated]);
}
