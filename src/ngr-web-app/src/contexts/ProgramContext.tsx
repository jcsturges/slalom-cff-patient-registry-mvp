import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import { useOktaAuth } from '@okta/okta-react';
import { programsService } from '../services/programs';
import type { CareProgramDto } from '../types';

const STORAGE_KEY = 'ngr_selected_program_id';

export interface ProgramContextValue {
  /** All programs the current user is associated with */
  programs: CareProgramDto[];
  /** The currently selected program (null if user has no programs or is Foundation Admin without programs) */
  selectedProgram: CareProgramDto | null;
  /** Select a different program */
  selectProgram: (programId: number) => void;
  /** Whether programs are loading */
  loading: boolean;
}

const ProgramContext = createContext<ProgramContextValue>({
  programs: [],
  selectedProgram: null,
  selectProgram: () => {},
  loading: true,
});

export function ProgramProvider({ children }: { children: ReactNode }) {
  const { authState } = useOktaAuth();
  const [programs, setPrograms] = useState<CareProgramDto[]>([]);
  const [selectedProgram, setSelectedProgram] = useState<CareProgramDto | null>(null);
  const [loading, setLoading] = useState(true);

  // Load programs when authenticated
  useEffect(() => {
    if (!authState?.isAuthenticated) {
      setPrograms([]);
      setSelectedProgram(null);
      setLoading(false);
      return;
    }

    let cancelled = false;
    setLoading(true);

    programsService
      .getAll({ includeInactive: false, includeOrh: false })
      .then((all) => {
        if (cancelled) return;
        setPrograms(all);

        // Try to restore last-selected program from localStorage
        const savedId = localStorage.getItem(STORAGE_KEY);
        const savedProgram = savedId ? all.find((p) => p.programId === Number(savedId)) : null;

        if (savedProgram) {
          setSelectedProgram(savedProgram);
        } else if (all.length > 0) {
          setSelectedProgram(all[0]);
          localStorage.setItem(STORAGE_KEY, String(all[0].programId));
        }
      })
      .catch(() => {
        // Programs may not be accessible for all users — non-critical
        if (!cancelled) setPrograms([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [authState?.isAuthenticated]);

  const selectProgram = useCallback(
    (programId: number) => {
      const program = programs.find((p) => p.programId === programId);
      if (program) {
        setSelectedProgram(program);
        localStorage.setItem(STORAGE_KEY, String(programId));
      }
    },
    [programs],
  );

  return (
    <ProgramContext.Provider value={{ programs, selectedProgram, selectProgram, loading }}>
      {children}
    </ProgramContext.Provider>
  );
}

export function useProgram(): ProgramContextValue {
  return useContext(ProgramContext);
}
