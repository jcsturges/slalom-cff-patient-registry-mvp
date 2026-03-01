export interface PatientDto {
  id: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  medicalRecordNumber: string | null;
  gender: string | null;
  email: string | null;
  phone: string | null;
  status: string;
  careProgramId: number;
  careProgramName: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePatientDto {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  medicalRecordNumber?: string;
  gender?: string;
  email?: string;
  phone?: string;
  careProgramId?: number;
}

export interface UpdatePatientDto {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  medicalRecordNumber?: string;
  gender?: string;
  email?: string;
  phone?: string;
  status: string;
}

// ── Care Program types ──────────────────────────────────────────

export interface CareProgramDto {
  id: number;
  programId: number;
  code: string;
  name: string;
  programType: string;
  city: string | null;
  state: string | null;
  address1: string | null;
  address2: string | null;
  zipCode: string | null;
  phone: string | null;
  email: string | null;
  accreditationDate: string | null;
  isActive: boolean;
  isOrphanHoldingProgram: boolean;
  isTrainingProgram: boolean;
  displayTitle: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCareProgramDto {
  programId: number;
  code: string;
  name: string;
  programType: string;
  city?: string;
  state?: string;
  address1?: string;
  address2?: string;
  zipCode?: string;
  phone?: string;
  email?: string;
  isTrainingProgram?: boolean;
}

export interface UpdateCareProgramDto {
  name: string;
  programType: string;
  city?: string;
  state?: string;
  address1?: string;
  address2?: string;
  zipCode?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
}
