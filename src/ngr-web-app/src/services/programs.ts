import { apiGet, apiPost, apiPut } from './api';
import type { CareProgramDto, CreateCareProgramDto, UpdateCareProgramDto } from '../types';

export interface ProgramListParams {
  programId?: number;
  name?: string;
  city?: string;
  state?: string;
  includeInactive?: boolean;
  includeOrh?: boolean;
  page?: number;
  pageSize?: number;
}

export const programsService = {
  getAll(params: ProgramListParams = {}): Promise<CareProgramDto[]> {
    return apiGet<CareProgramDto[]>(
      '/api/programs',
      params as Record<string, string | number | boolean | undefined>,
    );
  },

  getById(id: number): Promise<CareProgramDto> {
    return apiGet<CareProgramDto>(`/api/programs/${id}`);
  },

  create(data: CreateCareProgramDto): Promise<CareProgramDto> {
    return apiPost<CareProgramDto>('/api/programs', data);
  },

  update(id: number, data: UpdateCareProgramDto): Promise<CareProgramDto> {
    return apiPut<CareProgramDto>(`/api/programs/${id}`, data);
  },
};
