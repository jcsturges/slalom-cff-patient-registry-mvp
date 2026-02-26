import { OktaAuth } from '@okta/okta-auth-js';

export const oktaAuth = new OktaAuth({
  issuer: import.meta.env.VITE_OKTA_ISSUER,
  clientId: import.meta.env.VITE_OKTA_CLIENT_ID,
  redirectUri: import.meta.env.VITE_OKTA_REDIRECT_URI,
  scopes: (import.meta.env.VITE_OKTA_SCOPES || 'openid profile email groups').split(' '),
  pkce: true,
  tokenManager: {
    autoRenew: true,
    storage: 'localStorage',
  },
});
