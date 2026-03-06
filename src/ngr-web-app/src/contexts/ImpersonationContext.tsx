import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import { impersonationService, type ImpersonationSessionDto, type TargetUserDto } from '../services/impersonation';

interface ImpersonationState {
  isImpersonating: boolean;
  sessionId: string | null;
  targetUser: TargetUserDto | null;
  expiresAt: Date | null;
  initiatedFrom: string | null;
}

interface ImpersonationContextValue extends ImpersonationState {
  startImpersonation: (targetUserId: string) => Promise<void>;
  endImpersonation: (reason?: string) => Promise<void>;
  secondsRemaining: number;
  sessionExpired: boolean;
}

const ImpersonationContext = createContext<ImpersonationContextValue | null>(null);

const SESSION_KEY = 'ngr-impersonation-session';

function loadStoredSession(): ImpersonationSessionDto | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as ImpersonationSessionDto;
    if (new Date(parsed.expiresAt) <= new Date()) {
      sessionStorage.removeItem(SESSION_KEY);
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

export function ImpersonationProvider({ children }: { children: ReactNode }) {
  const stored = loadStoredSession();

  const [state, setState] = useState<ImpersonationState>({
    isImpersonating: stored != null,
    sessionId:       stored?.sessionId ?? null,
    targetUser:      stored?.targetUser ?? null,
    expiresAt:       stored ? new Date(stored.expiresAt) : null,
    initiatedFrom:   null,
  });

  const [secondsRemaining, setSecondsRemaining] = useState(0);
  const [sessionExpired, setSessionExpired]     = useState(false);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Use a ref for session ID so endImpersonation always sees the latest value
  const sessionIdRef = useRef<string | null>(state.sessionId);
  useEffect(() => { sessionIdRef.current = state.sessionId; }, [state.sessionId]);

  const endImpersonation = useCallback(async (reason = 'Manual') => {
    const sessionId = sessionIdRef.current;
    if (sessionId) {
      try {
        await impersonationService.end(sessionId, reason);
      } catch {
        // Best-effort — still clear local state
      }
    }

    sessionStorage.removeItem(SESSION_KEY);
    setState({
      isImpersonating: false,
      sessionId:       null,
      targetUser:      null,
      expiresAt:       null,
      initiatedFrom:   null,
    });
    setSessionExpired(false);
  }, []); // stable — uses ref for sessionId

  // Countdown timer
  useEffect(() => {
    if (!state.expiresAt) {
      setSecondsRemaining(0);
      return;
    }

    const tick = () => {
      const remaining = Math.max(0, Math.floor((state.expiresAt!.getTime() - Date.now()) / 1000));
      setSecondsRemaining(remaining);
      if (remaining === 0 && state.isImpersonating) {
        setSessionExpired(true);
      }
    };

    tick();
    timerRef.current = setInterval(tick, 1000);
    return () => { if (timerRef.current) clearInterval(timerRef.current); };
  }, [state.expiresAt, state.isImpersonating]);

  // Auto-end on expiration (3-second grace for visual message)
  useEffect(() => {
    if (!sessionExpired || !sessionIdRef.current) return;
    const t = setTimeout(() => { void endImpersonation('Expired'); }, 3000);
    return () => clearTimeout(t);
  }, [sessionExpired, endImpersonation]);

  const startImpersonation = useCallback(async (targetUserId: string) => {
    const initiatedFrom = window.location.pathname + window.location.search;
    const session = await impersonationService.start(targetUserId);

    sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));

    const newState: ImpersonationState = {
      isImpersonating: true,
      sessionId:       session.sessionId,
      targetUser:      session.targetUser,
      expiresAt:       new Date(session.expiresAt),
      initiatedFrom,
    };
    sessionIdRef.current = session.sessionId;
    setState(newState);
    setSessionExpired(false);
  }, []);

  return (
    <ImpersonationContext.Provider value={{
      ...state,
      startImpersonation,
      endImpersonation,
      secondsRemaining,
      sessionExpired,
    }}>
      {children}
    </ImpersonationContext.Provider>
  );
}

export function useImpersonation(): ImpersonationContextValue {
  const ctx = useContext(ImpersonationContext);
  if (!ctx) throw new Error('useImpersonation must be used inside <ImpersonationProvider>');
  return ctx;
}
