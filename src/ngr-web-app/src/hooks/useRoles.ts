import { useOktaAuth } from '@okta/okta-react';

/**
 * Maps API authorization policies (from Program.cs) to the groups that satisfy them.
 * Mirrors the RequireRole() definitions on the server exactly.
 */
const POLICY_GROUPS = {
  ClinicalUser:      ['ClinicalUser', 'ProgramAdmin', 'SystemAdmin'],
  ProgramAdmin:      ['ProgramAdmin', 'SystemAdmin'],
  FoundationAnalyst: ['FoundationAnalyst', 'SystemAdmin'],
  SystemAdmin:       ['SystemAdmin'],
} as const;

export interface Roles {
  /** Raw groups array from the Okta access token */
  groups: string[];
  /** Create and edit patients (ClinicalUser policy) */
  canCreatePatient: boolean;
  canEditPatient: boolean;
  /** Deactivate patients (ProgramAdmin policy) */
  canDeactivatePatient: boolean;
  /** Foundation-level read access */
  canAccessReports: boolean;
  /** Foundation admin (FoundationAnalyst or SystemAdmin) — for nav gating */
  isFoundationAdmin: boolean;
  /** Full system access */
  isSystemAdmin: boolean;
}

export function useRoles(): Roles {
  const { authState } = useOktaAuth();

  // Groups come from the access token's "groups" claim (mapped by Okta's claim config)
  const groups =
    (authState?.accessToken?.claims?.groups as string[] | undefined) ?? [];

  const satisfies = (policy: keyof typeof POLICY_GROUPS) =>
    POLICY_GROUPS[policy].some((g) => groups.includes(g));

  return {
    groups,
    canCreatePatient:    satisfies('ClinicalUser'),
    canEditPatient:      satisfies('ClinicalUser'),
    canDeactivatePatient: satisfies('ProgramAdmin'),
    canAccessReports:    satisfies('FoundationAnalyst'),
    isFoundationAdmin:   satisfies('FoundationAnalyst'),
    isSystemAdmin:       satisfies('SystemAdmin'),
  };
}
