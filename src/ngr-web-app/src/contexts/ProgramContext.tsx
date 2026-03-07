import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import { useQuery } from '@tanstack/react-query';
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
  const isAuthenticated = authState?.isAuthenticated ?? false;

  const [selectedProgram, setSelectedProgram] = useState<CareProgramDto | null>(null);

  // Use React Query so invalidateQueries({ queryKey: ['programs'] }) from ProgramFormPage
  // triggers an immediate refetch here, updating the header dropdown without a page reload.
  const { data: programs = [], isLoading: loading } = useQuery({
    queryKey: ['programs', 'context'],
    queryFn: () => programsService.getAll({ includeInactive: false, includeOrh: false }),
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000,
  });

  // Sync selectedProgram whenever the programs list changes (initial load or after invalidation)
  useEffect(() => {
    if (programs.length === 0) return;

    setSelectedProgram((current) => {
      // If we already have a selection, refresh it with the latest data from the server
      if (current) {
        const refreshed = programs.find((p) => p.id === current.id);
        return refreshed ?? programs[0];
      }

      // Otherwise restore last-selected from localStorage, or default to first program
      const savedId = localStorage.getItem(STORAGE_KEY);
      const saved = savedId ? programs.find((p) => p.programId === Number(savedId)) : null;
      if (saved) return saved;

      localStorage.setItem(STORAGE_KEY, String(programs[0].programId));
      return programs[0];
    });
  }, [programs]);

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
