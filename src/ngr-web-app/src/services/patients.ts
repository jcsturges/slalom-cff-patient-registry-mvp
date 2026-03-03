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
} from '../types';

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

  removeFromProgram(patientId: number, programId: number, _data: RemovePatientFromProgramDto): Promise<void> {
    // Body sent via POST since DELETE with body is non-standard
    return apiPost(`/api/patients/${patientId}/programs/${programId}/remove`, _data);
  },

  // ── Duplicate Detection (04-005) ──────────────────────────────
  checkDuplicates(data: DuplicateCheckDto): Promise<DuplicateMatchDto[]> {
    return apiPost<DuplicateMatchDto[]>('/api/patients/duplicates', data);
  },

  // ── Merge (04-008, 04-009, 04-010) ────────────────────────────
  merge(data: MergeRequestDto): Promise<MergeResultDto> {
    return apiPost<MergeResultDto>('/api/patients/merge', data);
  },
};
