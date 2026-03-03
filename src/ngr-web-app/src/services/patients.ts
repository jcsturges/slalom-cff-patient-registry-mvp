import { apiGet, apiPost, apiPut, apiDelete } from './api';
import type {
  PatientDto,
  CreatePatientDto,
  UpdatePatientDto,
  PatientProgramAssociationDto,
  AddPatientToProgramDto,
  RemovePatientFromProgramDto,
  DuplicateCheckDto,
  DuplicateMatchDto,
  MergeRequestDto,
  MergeResultDto,
  PatientDashboardDto,
  FormSubmissionDto,
  CreateFormSubmissionDto,
  PatientFileDto,
  UpdatePatientFileDto,
  BulkAssociationModifyDto,
  BulkAssociationResultDto,
  HardDeleteConfirmDto,
  UpdateFormDataDto,
  FormValidationResultDto,
  DatabaseLockRequestDto,
  DatabaseLockResultDto,
} from '../types';
import { oktaAuth } from '../lib/okta';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

function authHeaders(): Record<string, string> {
  const token = oktaAuth.getAccessToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export interface PatientListParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  careProgramId?: number;
}

export const patientsService = {
  // ── CRUD ─────────────────────────────────────────────────────
  getAll(params: PatientListParams = {}): Promise<PatientDto[]> {
    return apiGet<PatientDto[]>(
      '/api/patients',
      params as Record<string, string | number | boolean | undefined>,
    );
  },

  getById(id: number): Promise<PatientDto> {
    return apiGet<PatientDto>(`/api/patients/${id}`);
  },

  getCount(careProgramId?: number): Promise<number> {
    return apiGet<number>(
      '/api/patients/count',
      careProgramId !== undefined ? { careProgramId } : undefined,
    );
  },

  create(data: CreatePatientDto): Promise<PatientDto> {
    return apiPost<PatientDto>('/api/patients', data);
  },

  update(id: number, data: UpdatePatientDto): Promise<PatientDto> {
    return apiPut<PatientDto>(`/api/patients/${id}`, data);
  },

  delete(id: number): Promise<void> {
    return apiDelete(`/api/patients/${id}`);
  },

  // ── Program Associations (04-002, 04-006, 04-007) ────────────
  getProgramAssociations(patientId: number): Promise<PatientProgramAssociationDto[]> {
    return apiGet<PatientProgramAssociationDto[]>(`/api/patients/${patientId}/programs`);
  },

  addToProgram(patientId: number, data: AddPatientToProgramDto): Promise<PatientProgramAssociationDto> {
    return apiPost<PatientProgramAssociationDto>(`/api/patients/${patientId}/programs`, data);
  },

  removeFromProgram(patientId: number, programId: number, data: RemovePatientFromProgramDto): Promise<void> {
    return apiPost(`/api/patients/${patientId}/programs/${programId}/remove`, data);
  },

  // ── Duplicate Detection (04-005) ──────────────────────────────
  checkDuplicates(data: DuplicateCheckDto): Promise<DuplicateMatchDto[]> {
    return apiPost<DuplicateMatchDto[]>('/api/patients/duplicates', data);
  },

  // ── Merge (04-008, 04-009, 04-010) ────────────────────────────
  merge(data: MergeRequestDto): Promise<MergeResultDto> {
    return apiPost<MergeResultDto>('/api/patients/merge', data);
  },

  // ── Dashboard (05-001, 05-002) ────────────────────────────────
  getDashboard(patientId: number): Promise<PatientDashboardDto> {
    return apiGet<PatientDashboardDto>(`/api/patients/${patientId}/dashboard`);
  },

  // ── Form Submissions (05-002) ─────────────────────────────────
  getFormSubmissions(patientId: number, formCode?: string, page = 1, pageSize = 5): Promise<FormSubmissionDto[]> {
    return apiGet<FormSubmissionDto[]>(`/api/patients/${patientId}/forms`, {
      formCode,
      page,
      pageSize,
    });
  },

  createFormSubmission(patientId: number, data: CreateFormSubmissionDto): Promise<FormSubmissionDto> {
    return apiPost<FormSubmissionDto>(`/api/patients/${patientId}/forms`, data);
  },

  getFormSubmission(patientId: number, formId: number): Promise<FormSubmissionDto> {
    return apiGet<FormSubmissionDto>(`/api/patients/${patientId}/forms/${formId}`);
  },

  updateFormData(patientId: number, formId: number, data: UpdateFormDataDto): Promise<FormSubmissionDto> {
    return apiPut<FormSubmissionDto>(`/api/patients/${patientId}/forms/${formId}`, data);
  },

  validateForm(patientId: number, formId: number): Promise<FormValidationResultDto> {
    return apiPost<FormValidationResultDto>(`/api/patients/${patientId}/forms/${formId}/validate`, {});
  },

  deleteFormSubmission(patientId: number, formId: number): Promise<void> {
    return apiDelete(`/api/patients/${patientId}/forms/${formId}`);
  },

  // ── Database Lock (06-005) ──────────────────────────────────
  executeDatabaseLock(data: DatabaseLockRequestDto): Promise<DatabaseLockResultDto> {
    return apiPost<DatabaseLockResultDto>('/api/patients/database-lock', data);
  },

  // ── Patient Files (05-006, 05-007) ────────────────────────────
  getFiles(patientId: number, page = 1, pageSize = 5): Promise<PatientFileDto[]> {
    return apiGet<PatientFileDto[]>(`/api/patients/${patientId}/files`, { page, pageSize });
  },

  async uploadFile(
    patientId: number,
    file: File,
    programId: number,
    fileType: string,
    description?: string,
    otherFileTypeDescription?: string,
  ): Promise<PatientFileDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('programId', String(programId));
    formData.append('fileType', fileType);
    if (description) formData.append('description', description);
    if (otherFileTypeDescription) formData.append('otherFileTypeDescription', otherFileTypeDescription);

    const res = await fetch(`${BASE_URL}/api/patients/${patientId}/files`, {
      method: 'POST',
      headers: authHeaders(),
      body: formData,
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json() as Promise<PatientFileDto>;
  },

  updateFileMetadata(patientId: number, fileId: number, data: UpdatePatientFileDto): Promise<PatientFileDto> {
    return apiPut<PatientFileDto>(`/api/patients/${patientId}/files/${fileId}`, data);
  },

  deleteFile(patientId: number, fileId: number): Promise<void> {
    return apiDelete(`/api/patients/${patientId}/files/${fileId}`);
  },

  // ── Admin Tools (05-003, 05-004, 05-005) ──────────────────────
  hardDelete(patientId: number, data: HardDeleteConfirmDto): Promise<void> {
    return apiPost(`/api/patients/${patientId}/hard-delete`, data);
  },

  bulkModifyAssociations(data: BulkAssociationModifyDto): Promise<BulkAssociationResultDto> {
    return apiPost<BulkAssociationResultDto>('/api/patients/bulk-association', data);
  },

  // ── Form Business Rules (07-*) ────────────────────────────────
  canCreateForm(patientId: number, formType: string, programId: number): Promise<{ allowed: boolean; reason: string | null }> {
    return apiGet(`/api/patients/${patientId}/can-create-form`, { formType, programId });
  },

  getDefaultAnnualReviewYear(patientId: number, programId: number): Promise<{ year: number }> {
    return apiGet(`/api/patients/${patientId}/annual-review-default-year`, { programId });
  },

  getCarryForward(patientId: number, programId: number, subFormType: string): Promise<{ data: string | null; available: boolean }> {
    return apiGet(`/api/patients/${patientId}/carry-forward`, { programId, subFormType });
  },
};
