import { apiGet, apiPost, apiPut } from './api';

export interface ReconciliationEntity {
  entityType: string;
  creates: number;
  updates: number;
  deletes: number;
  total: number;
}

export interface ReconciliationReport {
  extractionWindowStart: string;
  extractionWindowEnd: string;
  runType: string;
  generatedAt: string;
  totalRecords: number;
  errorCount: number;
  qualityRate: number;
  entities: ReconciliationEntity[];
}

export interface FeedRun {
  id: number;
  runType: 'Delta' | 'Full';
  status: 'Pending' | 'Running' | 'Completed' | 'Failed';
  windowStart: string | null;
  windowEnd: string | null;
  startedAt: string;
  completedAt: string | null;
  totalRecords: number;
  errorCount: number;
  blobPath: string | null;
  triggeredBy: string;
  errorMessage: string | null;
  reconciliation: ReconciliationReport | null;
}

export interface FeedFieldMapping {
  id: number;
  ngrEntity: string;
  ngrProperty: string;
  cffColumnName: string;
  dataType: string;
  transformHint: string | null;
  version: number;
  isActive: boolean;
}

export interface TriggerFeedRunDto {
  runType: 'Delta' | 'Full';
  windowOverrideStart?: string;
  windowOverrideEnd?: string;
}

export interface UpdateFieldMappingDto {
  cffColumnName: string;
  transformHint?: string;
  isActive: boolean;
}

interface PagedResult<T> {
  total: number;
  page: number;
  pageSize: number;
  runs: T[];
}

export const dataFeedService = {
  getRuns: (page = 1, pageSize = 25) =>
    apiGet<PagedResult<FeedRun>>('/api/data-feed/runs', { page, pageSize }),

  getRun: (id: number) =>
    apiGet<FeedRun>(`/api/data-feed/runs/${id}`),

  triggerRun: (dto: TriggerFeedRunDto) =>
    apiPost<FeedRun>('/api/data-feed/runs', dto),

  getFieldMappings: () =>
    apiGet<FeedFieldMapping[]>('/api/data-feed/field-mappings'),

  updateFieldMapping: (id: number, dto: UpdateFieldMappingDto) =>
    apiPut<FeedFieldMapping>(`/api/data-feed/field-mappings/${id}`, dto),
};
