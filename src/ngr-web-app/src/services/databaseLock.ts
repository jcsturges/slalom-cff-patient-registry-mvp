import { apiGet, apiPost } from './api';

export interface DatabaseLockDto {
  id: number;
  reportingYear: number;
  lockDate: string;
  executionMode: 'Synchronous' | 'Batch';
  scheduledDate: string | null;
  status: 'Pending' | 'InProgress' | 'Completed' | 'Failed';
  initiatedBy: string;
  initiatedAt: string;
  completedAt: string | null;
  formsLocked: number;
  formsSkipped: number;
  progressFormsTotal: number | null;
  progressFormsProcessed: number;
  retryCount: number;
  errorMessage: string | null;
}

export interface DatabaseLockImpactDto {
  reportingYear: number;
  eligibleForms: number;
  alreadyLocked: number;
  wouldLock: number;
  isAlreadyLocked: boolean;
}

export interface DatabaseLockProgressDto {
  id: number;
  status: string;
  formsLocked: number;
  formsSkipped: number;
  progressFormsTotal: number | null;
  progressFormsProcessed: number;
  completedAt: string | null;
  errorMessage: string | null;
  progressPercent: number | null;
}

export interface CreateDatabaseLockDto {
  reportingYear: number;
  lockDate: string;
  executionMode: 'Synchronous' | 'Batch';
  scheduledDate?: string;
}

export const databaseLockService = {
  getLocks(): Promise<DatabaseLockDto[]> {
    return apiGet<DatabaseLockDto[]>('/api/admin/database-locks');
  },

  getImpact(reportingYear: number): Promise<DatabaseLockImpactDto> {
    return apiGet<DatabaseLockImpactDto>('/api/admin/database-locks/impact', { reportingYear });
  },

  getProgress(id: number): Promise<DatabaseLockProgressDto> {
    return apiGet<DatabaseLockProgressDto>(`/api/admin/database-locks/${id}/progress`);
  },

  createLock(dto: CreateDatabaseLockDto): Promise<DatabaseLockDto> {
    return apiPost<DatabaseLockDto>('/api/admin/database-locks', dto);
  },
};
