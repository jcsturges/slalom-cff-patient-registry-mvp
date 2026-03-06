import { apiPost } from './api';

export interface TrackEventDto {
  eventType: string;
  page?: string;
  component?: string;
  properties?: Record<string, unknown>;
  sessionId?: string;
  occurredAt?: string;
}

/**
 * Batches analytics events and flushes them in a single POST to /api/analytics/events.
 * Events are queued for up to 2 seconds or until the batch reaches 20 items.
 * Fire-and-forget — errors are silently ignored so analytics never blocks the UI.
 */
class AnalyticsService {
  private queue: TrackEventDto[] = [];
  private flushTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly sessionId: string;

  constructor() {
    // Stable browser-session ID (resets on tab close, not page refresh)
    const stored = sessionStorage.getItem('ngr-analytics-session');
    this.sessionId = stored ?? `as-${Date.now().toString(36)}`;
    if (!stored) sessionStorage.setItem('ngr-analytics-session', this.sessionId);
  }

  track(event: Omit<TrackEventDto, 'sessionId' | 'occurredAt'>) {
    this.queue.push({
      ...event,
      sessionId: this.sessionId,
      occurredAt: new Date().toISOString(),
    });

    if (!this.flushTimer) {
      this.flushTimer = setTimeout(() => this.flush(), 2000);
    }
    if (this.queue.length >= 20) {
      this.flush();
    }
  }

  private flush() {
    if (this.flushTimer) {
      clearTimeout(this.flushTimer);
      this.flushTimer = null;
    }
    if (this.queue.length === 0) return;

    const batch = this.queue.splice(0);
    // Fire-and-forget; ignore errors
    apiPost('/api/analytics/events', batch).catch(() => undefined);
  }
}

export const analyticsService = new AnalyticsService();
