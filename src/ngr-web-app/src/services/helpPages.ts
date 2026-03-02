import { apiGet, apiPost, apiPut, apiDelete } from './api';
import type { HelpPageDto, CreateHelpPageDto, UpdateHelpPageDto } from '../types';

export const helpPagesService = {
  getAll(includeUnpublished = false): Promise<HelpPageDto[]> {
    return apiGet<HelpPageDto[]>('/api/help-pages', { includeUnpublished });
  },

  getTree(includeUnpublished = false): Promise<HelpPageDto[]> {
    return apiGet<HelpPageDto[]>('/api/help-pages/tree', { includeUnpublished });
  },

  getById(id: number): Promise<HelpPageDto> {
    return apiGet<HelpPageDto>(`/api/help-pages/${id}`);
  },

  getBySlug(slug: string): Promise<HelpPageDto> {
    return apiGet<HelpPageDto>(`/api/help-pages/slug/${slug}`);
  },

  getByContextKey(contextKey: string): Promise<HelpPageDto> {
    return apiGet<HelpPageDto>(`/api/help-pages/context/${contextKey}`);
  },

  create(data: CreateHelpPageDto): Promise<HelpPageDto> {
    return apiPost<HelpPageDto>('/api/help-pages', data);
  },

  update(id: number, data: UpdateHelpPageDto): Promise<HelpPageDto> {
    return apiPut<HelpPageDto>(`/api/help-pages/${id}`, data);
  },

  delete(id: number): Promise<void> {
    return apiDelete(`/api/help-pages/${id}`);
  },
};
