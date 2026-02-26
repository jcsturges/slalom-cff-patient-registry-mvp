import { apiGet, apiPost, apiPut, apiDelete } from './api';
import type { PatientDto, CreatePatientDto, UpdatePatientDto } from '../types';

export interface PatientListParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  careProgramId?: number;
}

export const patientsService = {
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
};
