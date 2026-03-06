import { apiGet, apiPost } from './api';

export interface ValidationCheck {
  checkName: string;
  entityType: string;
  status: 'Pass' | 'Fail';
  detail: string | null;
  expectedCount: number | null;
  actualCount: number | null;
}

export interface ValidationReport {
  generatedAt: string;
  overallStatus: 'Pass' | 'Fail';
  totalChecks: number;
  passedChecks: number;
  failedChecks: number;
  checks: ValidationCheck[];
}

export interface MigrationRun {
  id: number;
  phase: string;
  status: 'Pending' | 'Running' | 'Completed' | 'Failed' | 'PartialSuccess';
  startedAt: string;
  completedAt: string | null;
  sourceCount: number;
  targetCount: number;
  errorCount: number;
  triggeredBy: string;
  errorMessage: string | null;
  validationReport: ValidationReport | null;
}

export interface MigrationPhase {
  id: string;
  description: string;
  srsRef: string;
}

interface PagedResult<T> {
  total: number;
  page: number;
  pageSize: number;
  runs: T[];
}

export const migrationService = {
  getRuns: (page = 1, pageSize = 25) =>
    apiGet<PagedResult<MigrationRun>>('/api/migration/runs', { page, pageSize }),

  getRun: (id: number) =>
    apiGet<MigrationRun>(`/api/migration/runs/${id}`),

  triggerPhase: (phase: string) =>
    apiPost<MigrationRun>('/api/migration/runs', { phase }),

  validateRun: (id: number) =>
    apiPost<ValidationReport>(`/api/migration/runs/${id}/validation`, {}),

  getPhases: () =>
    apiGet<{ phases: MigrationPhase[]; migrationUser: string; notes: string[] }>('/api/migration/phases'),
};
