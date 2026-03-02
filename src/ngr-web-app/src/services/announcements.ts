import { apiGet, apiPost, apiPut, apiDelete } from './api';
import type { AnnouncementDto, CreateAnnouncementDto, UpdateAnnouncementDto } from '../types';

export const announcementsService = {
  getAll(includeInactive = false): Promise<AnnouncementDto[]> {
    return apiGet<AnnouncementDto[]>('/api/announcements', { includeInactive });
  },

  getActive(): Promise<AnnouncementDto[]> {
    return apiGet<AnnouncementDto[]>('/api/announcements/active');
  },

  getById(id: number): Promise<AnnouncementDto> {
    return apiGet<AnnouncementDto>(`/api/announcements/${id}`);
  },

  create(data: CreateAnnouncementDto): Promise<AnnouncementDto> {
    return apiPost<AnnouncementDto>('/api/announcements', data);
  },

  update(id: number, data: UpdateAnnouncementDto): Promise<AnnouncementDto> {
    return apiPut<AnnouncementDto>(`/api/announcements/${id}`, data);
  },

  delete(id: number): Promise<void> {
    return apiDelete(`/api/announcements/${id}`);
  },
};
