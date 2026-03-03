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

// ── Form Submission types ───────────────────────────────────────

export interface FormSubmissionDto {
  id: number;
  formDefinitionId: number;
  formName: string;
  formCode: string;
  formType: string;
  isShared: boolean;
  patientId: number;
  encounterId: number | null;
  programId: number;
  programName: string;
  completionStatus: string;
  lockStatus: string;
  status: string;
  lastUpdateSource: string;
  requiresReview: boolean;
  createdAt: string;
  updatedAt: string;
  lastModifiedBy: string | null;
  encounterDate: string | null;
  annualReviewYear: number | null;
  transplantOrgan: string | null;
  careEpisodeStartDate: string | null;
  careEpisodeEndDate: string | null;
  phoneNoteDate: string | null;
  labDate: string | null;
}

export interface CreateFormSubmissionDto {
  formDefinitionId: number;
  patientId: number;
  encounterId?: number;
  programId: number;
  formDataJson?: string;
}

export interface UpdateFormDataDto {
  formDataJson: string;
  markComplete: boolean;
}

export interface FormValidationResultDto {
  isValid: boolean;
  canSave: boolean;
  canComplete: boolean;
  messages: ValidationMessageDto[];
}

export interface ValidationMessageDto {
  severity: 'Warning' | 'CompletionBlocking' | 'SaveBlocking' | 'DependencyChange';
  fieldId: string;
  fieldLabel: string;
  message: string;
  correctiveAction: string | null;
}

export interface FormDefinitionDto {
  id: number;
  name: string;
  code: string;
  description: string | null;
  formType: string;
  isShared: boolean;
  autoComplete: boolean;
  schemaJson: string;
  validationRulesJson: string | null;
  uiSchemaJson: string | null;
}

export interface DatabaseLockRequestDto {
  reportingYear: number;
  mode: 'synchronous' | 'scheduled';
  scheduledAt?: string;
}

export interface DatabaseLockResultDto {
  reportingYear: number;
  formsLocked: number;
  formsSkipped: number;
  status: string;
}

// ── Patient Dashboard types ─────────────────────────────────────

export interface PatientDashboardDto {
  patient: PatientDto;
  sharedForms: FormSubmissionDto[];
  transplants: FormSubmissionDto[];
  annualReviews: FormSubmissionDto[];
  encounters: FormSubmissionDto[];
  labs: FormSubmissionDto[];
  careEpisodes: FormSubmissionDto[];
  phoneNotes: FormSubmissionDto[];
  aldStatus: FormSubmissionDto[];
  files: PatientFileDto[];
}

// ── Patient File types ──────────────────────────────────────────

export interface PatientFileDto {
  id: number;
  patientId: number;
  programId: number;
  programName: string;
  originalFileName: string;
  storedFileName: string;
  contentType: string;
  fileExtension: string;
  fileSize: number;
  description: string | null;
  fileType: string;
  otherFileTypeDescription: string | null;
  uploadedAt: string;
  uploadedBy: string;
  downloadUrl: string | null;
}

export interface UpdatePatientFileDto {
  description?: string;
  fileType: string;
  otherFileTypeDescription?: string;
}

// ── Admin bulk modification types ───────────────────────────────

export interface BulkAssociationModifyDto {
  patientIds: number[];
  action: string;
  targetProgramId?: number;
  sourceProgramId?: number;
  reason?: string;
}

export interface BulkAssociationResultDto {
  patientsAffected: number;
  action: string;
  status: string;
}

export interface HardDeleteConfirmDto {
  confirmCffId: number;
}

// ── Reporting types ─────────────────────────────────────────────

export interface SavedReportDto {
  id: number;
  title: string;
  description: string | null;
  scope: string;
  reportType: string;
  ownerEmail: string;
  programId: number | null;
  version: number;
  createdAt: string;
  updatedAt: string;
  lastExecutedAt: string | null;
}

export interface CreateSavedReportDto {
  title: string;
  description?: string;
  scope?: string;
  queryDefinitionJson: string;
  programId?: number;
}

export interface UpdateSavedReportDto {
  title: string;
  description?: string;
  queryDefinitionJson?: string;
}

export interface ExecuteReportDto {
  savedReportId?: number;
  reportType?: string;
  programId?: number;
  queryDefinitionJson?: string;
  reportingYear?: number;
}

export interface ReportResultDto {
  executionId: number;
  reportTitle: string;
  reportType: string;
  executedBy: string;
  executedAt: string;
  recordCount: number;
  executionTimeMs: number;
  columns: string[];
  rows: Record<string, unknown>[];
  querySummary: string | null;
}

export interface ReportDownloadRequestDto {
  executionId: number;
  format: 'csv' | 'excel';
}

// ── Data Export types ───────────────────────────────────────────

export interface DataExportRequestDto {
  formTypes: string[];
  dateFrom?: string;
  dateTo?: string;
  completenessFilter: 'complete_only' | 'all';
  diagnosisFilter?: string;
  outputFormat: 'coded' | 'descriptive';
  programId: number;
}

export interface SavedDownloadDefinitionDto {
  id: number;
  name: string;
  description: string | null;
  ownerEmail: string;
  programId: number;
  parametersJson: string;
  createdAt: string;
  updatedAt: string;
  lastExecutedAt: string | null;
}

export interface CreateSavedDownloadDto {
  name: string;
  description?: string;
  programId: number;
  parametersJson: string;
}

export interface UpdateSavedDownloadDto {
  name: string;
  description?: string;
  parametersJson?: string;
}
