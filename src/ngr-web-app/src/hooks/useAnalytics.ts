import { useCallback, useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import { analyticsService } from '../services/analytics';

/**
 * Tracks user interaction events for usability analytics (12-004).
 *
 * - Fires a "page_view" event on every route change.
 * - Tracks "time_on_page" when the user navigates away from a page.
 * - Returns a `trackEvent` helper for ad-hoc component events.
 *
 * No PHI is ever sent — only route paths, component names, and duration.
 */
export function useAnalytics(component?: string) {
  const location = useLocation();
  const pageEntryTime = useRef<number>(Date.now());
  const lastPage = useRef<string>(location.pathname);

  useEffect(() => {
    const currentPage = location.pathname;

    // If the page changed, log time spent on the previous page
    if (lastPage.current !== currentPage) {
      const durationMs = Date.now() - pageEntryTime.current;
      analyticsService.track({
        eventType: 'time_on_page',
        page: lastPage.current,
        properties: { durationMs },
      });
    }

    // Log the new page view
    analyticsService.track({
      eventType: 'page_view',
      page: currentPage,
    });

    lastPage.current = currentPage;
    pageEntryTime.current = Date.now();
  }, [location.pathname]);

  const trackEvent = useCallback(
    (eventType: string, properties?: Record<string, unknown>) => {
      analyticsService.track({ eventType, page: location.pathname, component, properties });
    },
    [location.pathname, component],
  );

  return { trackEvent };
}
