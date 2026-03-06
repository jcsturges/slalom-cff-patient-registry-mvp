import { oktaAuth } from '../lib/okta';
import { apiGet } from './api';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export interface ImportJobDto {
  id: number;
  fileName: string;
  status: string;
  programId: number;
  totalRows: number | null;
  processedRows: number | null;
  errorRows: number | null;
  warningCount: number | null;
  createdAt: string;
  createdBy: string;
  completedAt: string | null;
}

export interface EmrImportIssue {
  severity: 'Error' | 'Warning';
  rowNumber: number | null;
  csvColumn: string | null;
  fieldPath: string | null;
  message: string;
}

export interface ImportJobDetailDto extends ImportJobDto {
  errors: EmrImportIssue[];
  warnings: EmrImportIssue[];
}

export interface EmrValidationResult {
  isValid: boolean;
  rowCount: number;
  headers: string[];
  errors: EmrImportIssue[];
  warnings: EmrImportIssue[];
}

export const emrImportService = {
  /** Upload a CSV file and start an import job */
  async upload(
    file: File,
    programId: number,
    onProgress?: (pct: number) => void,
  ): Promise<ImportJobDto> {
    const token = oktaAuth.getAccessToken();
    const formData = new FormData();
    formData.append('file', file);
    formData.append('programId', String(programId));

    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${BASE_URL}/api/import/upload`);
      if (token) xhr.setRequestHeader('Authorization', `Bearer ${token}`);

      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable && onProgress) onProgress(Math.round((e.loaded / e.total) * 100));
      };

      xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          resolve(JSON.parse(xhr.responseText) as ImportJobDto);
        } else {
          reject(new Error(xhr.responseText || `HTTP ${xhr.status}`));
        }
      };
      xhr.onerror = () => reject(new Error('Network error during upload'));
      xhr.send(formData);
    });
  },

  /** Validate a CSV without persisting — returns full error/warning list */
  async validate(file: File, programId: number): Promise<EmrValidationResult> {
    const token = oktaAuth.getAccessToken();
    const formData = new FormData();
    formData.append('file', file);
    formData.append('programId', String(programId));

    const res = await fetch(`${BASE_URL}/api/import/validate`, {
      method: 'POST',
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      body: formData,
    });

    if (!res.ok) throw new Error((await res.text()) || `HTTP ${res.status}`);
    return res.json() as Promise<EmrValidationResult>;
  },

  getJobs(programId: number): Promise<ImportJobDto[]> {
    return apiGet<ImportJobDto[]>('/api/import/jobs', { programId });
  },

  getJob(id: number): Promise<ImportJobDto> {
    return apiGet<ImportJobDto>(`/api/import/jobs/${id}`);
  },

  getJobErrors(id: number): Promise<ImportJobDetailDto> {
    return apiGet<ImportJobDetailDto>(`/api/import/jobs/${id}/errors`);
  },
};
