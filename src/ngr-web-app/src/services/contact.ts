import { apiPost } from './api';
import { oktaAuth } from '../lib/okta';
import type { ContactRequestDto } from '../types';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

function authHeaders(): Record<string, string> {
  const token = oktaAuth.getAccessToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export const contactService = {
  /** Submit a contact request (supports file attachment via FormData) */
  async submit(data: {
    subject: string;
    message: string;
    attachment?: File;
  }): Promise<ContactRequestDto> {
    if (data.attachment) {
      // Use FormData for file uploads
      const formData = new FormData();
      formData.append('subject', data.subject);
      formData.append('message', data.message);
      formData.append('attachment', data.attachment);

      const res = await fetch(`${BASE_URL}/api/contact`, {
        method: 'POST',
        headers: authHeaders(),
        body: formData,
      });
      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `HTTP ${res.status}`);
      }
      return res.json() as Promise<ContactRequestDto>;
    }

    // No attachment — use JSON
    return apiPost<ContactRequestDto>('/api/contact', {
      subject: data.subject,
      message: data.message,
    });
  },
};
