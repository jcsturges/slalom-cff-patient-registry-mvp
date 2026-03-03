import type { Roles } from '../hooks/useRoles';

export interface NavItem {
  label: string;
  path: string;
  /** If set, item is shown only when this check passes */
  visibleWhen?: (roles: Roles) => boolean;
  /** Sub-items for dropdown menus */
  children?: NavItem[];
}

/**
 * Care Program user navigation items.
 * Visible to ClinicalUser, ProgramAdmin roles.
 */
export const CP_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', path: '/' },
  { label: 'Help', path: '/help' },
  {
    label: 'Patients',
    path: '/patients',
    children: [
      { label: 'Program Roster', path: '/patients' },
      { label: 'Merge Duplicates', path: '/patients/merge' },
    ],
  },
  { label: 'User Management', path: '/user-management' },
  { label: 'Reporting', path: '/reports' },
  { label: 'Data Export', path: '/export' },
  { label: 'EMR Upload', path: '/import' },
];

/**
 * Foundation Administrator navigation items.
 * Visible to FoundationAnalyst, SystemAdmin roles.
 */
export const FOUNDATION_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', path: '/' },
  {
    label: 'Patients',
    path: '/admin/patient-search',
    children: [
      { label: 'Patient Search', path: '/admin/patient-search' },
      { label: 'Merge Duplicates', path: '/patients/merge' },
    ],
  },
  {
    label: 'Administration',
    path: '/programs',
    children: [
      { label: 'Care Programs', path: '/programs' },
      { label: 'User Management', path: '/user-management' },
      { label: 'Announcements', path: '/admin/announcements' },
      { label: 'Help Manager', path: '/admin/help-pages' },
      { label: 'Database Lock', path: '/admin/database-lock' },
    ],
  },
  { label: 'User Analytics', path: '/admin/analytics' },
  { label: 'Reporting', path: '/reports' },
  { label: 'Data Export', path: '/export' },
];

/**
 * Get the correct navigation items for the user's role.
 */
export function getNavItems(roles: Roles): NavItem[] {
  if (roles.isFoundationAdmin) return FOUNDATION_NAV_ITEMS;
  return CP_NAV_ITEMS;
}
