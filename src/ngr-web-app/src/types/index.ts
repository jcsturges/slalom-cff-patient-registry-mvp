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
  careProgramId: number;
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
