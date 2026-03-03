import { apiGet, apiPost, apiPut, apiDelete } from './api';
import { oktaAuth } from '../lib/okta';
import type {
  DataExportRequestDto,
  SavedDownloadDefinitionDto,
  CreateSavedDownloadDto,
  UpdateSavedDownloadDto,
} from '../types';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export const dataExportService = {
  /** Execute export and download the ZIP file */
  async executeExport(data: DataExportRequestDto): Promise<void> {
    const token = oktaAuth.getAccessToken();
    const res = await fetch(`${BASE_URL}/api/data-export/execute`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: JSON.stringify(data),
    });

    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }

    // Download the ZIP
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `NGR_Export_${data.programId}_${new Date().toISOString().slice(0, 10)}.zip`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  },

  // ── Saved Definitions ─────────────────────────────────────────
  getDefinitions(programId?: number): Promise<SavedDownloadDefinitionDto[]> {
    return apiGet<SavedDownloadDefinitionDto[]>('/api/data-export/definitions', { programId });
  },

  getDefinition(id: number): Promise<SavedDownloadDefinitionDto> {
    return apiGet<SavedDownloadDefinitionDto>(`/api/data-export/definitions/${id}`);
  },

  createDefinition(data: CreateSavedDownloadDto): Promise<SavedDownloadDefinitionDto> {
    return apiPost<SavedDownloadDefinitionDto>('/api/data-export/definitions', data);
  },

  updateDefinition(id: number, data: UpdateSavedDownloadDto): Promise<SavedDownloadDefinitionDto> {
    return apiPut<SavedDownloadDefinitionDto>(`/api/data-export/definitions/${id}`, data);
  },

  deleteDefinition(id: number): Promise<void> {
    return apiDelete(`/api/data-export/definitions/${id}`);
  },
};
