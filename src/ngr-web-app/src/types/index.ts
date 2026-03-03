export interface PatientDto {
  id: number;
  registryId: string;
  cffId: number;
  firstName: string;
  middleName: string | null;
  lastName: string;
  lastNameAtBirth: string | null;
  dateOfBirth: string;
  biologicalSexAtBirth: string | null;
  gender: string | null;
  medicalRecordNumber: string | null;
  email: string | null;
  phone: string | null;
  status: string;
  diagnosis: string | null;
  vitalStatus: string;
  careProgramId: number;
  careProgramName: string;
  lastModifiedBy: string | null;
  lastModifiedDate: string | null;
  createdAt: string;
  updatedAt: string;
  otherPrograms: string[];
}

export interface CreatePatientDto {
  firstName: string;
  middleName?: string;
  lastName: string;
  lastNameAtBirth?: string;
  dateOfBirth: string;
  biologicalSexAtBirth?: string;
  ssnLast4?: string;
  medicalRecordNumber?: string;
  gender?: string;
  email?: string;
  phone?: string;
  careProgramId?: number;
  knownRegistryId?: string;
}

export interface UpdatePatientDto {
  firstName: string;
  middleName?: string;
  lastName: string;
  lastNameAtBirth?: string;
  dateOfBirth: string;
  biologicalSexAtBirth?: string;
  medicalRecordNumber?: string;
  gender?: string;
  email?: string;
  phone?: string;
  status: string;
}

// ── Patient-Program Association types ───────────────────────────

export interface PatientProgramAssociationDto {
  id: number;
  patientId: number;
  programId: number;
  programName: string;
  localMRN: string | null;
  status: string;
  isPrimaryProgram: boolean;
  enrollmentDate: string;
  disenrollmentDate: string | null;
  removalReason: string | null;
}

export interface AddPatientToProgramDto {
  programId: number;
  localMRN?: string;
  isPrimaryProgram: boolean;
}

export interface RemovePatientFromProgramDto {
  removalReason: string;
}

// ── Duplicate Detection types ───────────────────────────────────

export interface DuplicateCheckDto {
  firstName?: string;
  lastName?: string;
  dateOfBirth?: string;
  biologicalSexAtBirth?: string;
  registryId?: string;
}

export interface DuplicateMatchDto {
  patientId: number;
  registryId: string;
  cffId: number;
  firstName: string;
  lastName: string;
  lastNameAtBirth: string | null;
  dateOfBirth: string;
  biologicalSexAtBirth: string | null;
  programAssociations: string[];
  isOrh: boolean;
  confidenceScore: number;
  matchReason: string;
}

// ── Merge types ─────────────────────────────────────────────────

export interface MergeRequestDto {
  primaryPatientId: number;
  secondaryPatientId: number;
}

export interface MergeResultDto {
  primaryPatientId: number;
  secondaryPatientId: number;
  aliasesCreated: number;
  associationsMerged: number;
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

// ── Announcement types ──────────────────────────────────────────

export interface AnnouncementDto {
  id: number;
  title: string;
  content: string;
  isActive: boolean;
  effectiveDate: string;
  expirationDate: string | null;
  createdBy: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateAnnouncementDto {
  title: string;
  content: string;
  effectiveDate: string;
  expirationDate?: string;
}

export interface UpdateAnnouncementDto {
  title: string;
  content: string;
  effectiveDate: string;
  expirationDate?: string;
  isActive: boolean;
}

// ── Help Page types ─────────────────────────────────────────────

export interface HelpPageDto {
  id: number;
  title: string;
  slug: string;
  content: string;
  parentId: number | null;
  sortOrder: number;
  isPublished: boolean;
  contextKey: string | null;
  createdBy: string;
  createdAt: string;
  updatedAt: string | null;
  children?: HelpPageDto[];
  attachments?: HelpAttachmentDto[];
}

export interface CreateHelpPageDto {
  title: string;
  slug: string;
  content: string;
  parentId?: number;
  sortOrder?: number;
  isPublished: boolean;
  contextKey?: string;
}

export interface UpdateHelpPageDto {
  title: string;
  slug: string;
  content: string;
  parentId?: number;
  sortOrder?: number;
  isPublished: boolean;
  contextKey?: string;
}

export interface HelpAttachmentDto {
  id: number;
  helpPageId: number;
  fileName: string;
  fileSize: number;
  contentType: string;
  downloadUrl: string;
  createdAt: string;
}

// ── Contact / Support Request types ─────────────────────────────

export interface ContactRequestDto {
  id: number;
  referenceId: string;
  name: string;
  email: string;
  programNumber: string | null;
  subject: string;
  message: string;
  attachmentFileName: string | null;
  attachmentUrl: string | null;
  status: string;
  createdAt: string;
}

export interface CreateContactRequestDto {
  subject: string;
  message: string;
}

// ── Paginated response wrapper ──────────────────────────────────

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
