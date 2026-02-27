# Patient File Upload

**Story ID:** 05-006
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P1
**SRS Reference:** Section 6.7

## User Story
As a care program editor, I want to upload patient documents from the Patient Dashboard so that clinical documents are associated with the patient's record.

## Description
Upload files from Patient Dashboard. Approved file types: .pdf, .jpg, .jpeg, .png, .tif, .tiff (validated server-side by extension and MIME type). Max 10MB. Metadata collected during upload: Description (string, ≤1000 chars), File Type (enum: Informed Consent, Genotype Results, Sweat Test Results, Lab Results, Other). If "Other" selected, a free-text description is required.

File renamed using convention: `<Prefix>_<PatientRegistryID>_<SiteID>_<MMDDYYYY>_<HHMMSS>` where Prefix is determined by File Type (ICF, DX, Lab, Swt, GTP, Oth+first5chars of original filename).

## Dependencies
- 05-001 (Patient Dashboard Layout)

## Acceptance Criteria
- [ ] Upload restricted to approved file types (.pdf, .jpg, .jpeg, .png, .tif, .tiff)
- [ ] Server-side MIME type validation rejects executables and scripts
- [ ] Maximum file size: 10MB with clear error message for larger files
- [ ] Metadata collected: Description, File Type (enum), Other File Type description
- [ ] File renamed per naming convention with correct prefix
- [ ] Stored metadata: CFF ID, Care Program ID, file size, original name, extension, upload date, uploaded by
- [ ] Invalid filename characters replaced with underscores
- [ ] File list displayed on Patient Dashboard: type, date uploaded, care program

## Technical Notes
- Use Azure Blob Storage for HIPAA-compliant file storage
- Server-side MIME type checking is critical — don't trust client-side validation alone
- Consider virus scanning on upload
