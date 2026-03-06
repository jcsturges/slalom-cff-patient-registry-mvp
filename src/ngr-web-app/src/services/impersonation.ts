import { apiGet, apiPost } from './api';

export interface TargetUserDto {
  id: string;
  name: string;
  email: string;
  groups: string[];
}

export interface ImpersonationSessionDto {
  sessionId: string;
  targetUser: TargetUserDto;
  startedAt: string;
  expiresAt: string;
  isActive: boolean;
}

export const impersonationService = {
  start(targetUserId: string): Promise<ImpersonationSessionDto> {
    return apiPost<ImpersonationSessionDto>('/api/admin/impersonation/start', { targetUserId });
  },

  end(sessionId: string, endReason = 'Manual'): Promise<{ success: boolean }> {
    return apiPost<{ success: boolean }>('/api/admin/impersonation/end', { sessionId, endReason });
  },

  getStatus(): Promise<ImpersonationSessionDto | null> {
    return apiGet<ImpersonationSessionDto | null>('/api/admin/impersonation/status');
  },
};
