import { apiGet, apiPost, apiPut, apiDelete } from './api';
import type {
  SavedReportDto,
  CreateSavedReportDto,
  UpdateSavedReportDto,
  ExecuteReportDto,
  ReportResultDto,
  ReportDownloadRequestDto,
} from '../types';

export const reportsService = {
  // ── Saved Reports ─────────────────────────────────────────────
  getSavedReports(scope?: string, programId?: number): Promise<SavedReportDto[]> {
    return apiGet<SavedReportDto[]>('/api/reports/saved', { scope, programId });
  },

  getSavedReport(id: number): Promise<SavedReportDto> {
    return apiGet<SavedReportDto>(`/api/reports/saved/${id}`);
  },

  createSavedReport(data: CreateSavedReportDto): Promise<SavedReportDto> {
    return apiPost<SavedReportDto>('/api/reports/saved', data);
  },

  updateSavedReport(id: number, data: UpdateSavedReportDto): Promise<SavedReportDto> {
    return apiPut<SavedReportDto>(`/api/reports/saved/${id}`, data);
  },

  deleteSavedReport(id: number): Promise<void> {
    return apiDelete(`/api/reports/saved/${id}`);
  },

  // ── Execution ─────────────────────────────────────────────────
  executeReport(data: ExecuteReportDto): Promise<ReportResultDto> {
    return apiPost<ReportResultDto>('/api/reports/execute', data);
  },

  // ── Pre-defined Reports ───────────────────────────────────────
  incompleteRecords(programId: number, reportingYear: number): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/incomplete-records', { programId, reportingYear });
  },

  patientsDueVisit(programId: number): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/patients-due-visit', { programId });
  },

  diabetesTesting(programId: number): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/diabetes-testing', { programId });
  },

  // ── Admin Reports ─────────────────────────────────────────────
  programList(): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/admin/program-list');
  },

  mergeReport(): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/admin/merges');
  },

  transferReport(): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/admin/transfers');
  },

  fileUploadReport(): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/admin/file-uploads');
  },

  // ── Audit Reports ─────────────────────────────────────────────
  userManagementAudit(startDate: string, endDate: string): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/audit/user-management', { startDate, endDate });
  },

  downloadAudit(startDate: string, endDate: string): Promise<ReportResultDto> {
    return apiGet<ReportResultDto>('/api/reports/audit/downloads', { startDate, endDate });
  },

  // ── Download ──────────────────────────────────────────────────
  downloadReport(data: ReportDownloadRequestDto): Promise<Blob> {
    return apiPost<Blob>('/api/reports/download', data);
  },
};
