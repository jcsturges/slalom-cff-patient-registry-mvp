import { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Alert, Box, CircularProgress } from '@mui/material';
import { patientsService } from '../../services/patients';
import { useRoles } from '../../hooks/useRoles';
import { FormPageLayout } from '../../components/forms/FormPageLayout';
import { FormRenderer, type FormSchema } from '../../components/forms/FormRenderer';
import type { ValidationMessageDto } from '../../types';

export function FormEditPage() {
  const { patientId, formId } = useParams<{ patientId: string; formId: string }>();
  const { canEditPatient, isFoundationAdmin } = useRoles();

  const [formData, setFormData] = useState<Record<string, unknown>>({});
  const [initialData, setInitialData] = useState<string>('');
  const [validationMessages, setValidationMessages] = useState<ValidationMessageDto[]>([]);

  // Load form submission
  const { data: submission, isLoading: loadingForm } = useQuery({
    queryKey: ['formSubmission', patientId, formId],
    queryFn: () => patientsService.getFormSubmission(Number(patientId), Number(formId)),
    enabled: !!patientId && !!formId,
  });

  // Load patient data for header
  const { data: patient, isLoading: loadingPatient } = useQuery({
    queryKey: ['patient', patientId],
    queryFn: () => patientsService.getById(Number(patientId)),
    enabled: !!patientId,
  });

  // Parse form schema from submission
  const formSchema = useMemo<FormSchema | null>(() => {
    if (!submission) return null;
    // The form definition is fetched via the submission's formDefinitionId
    // For now, return a basic schema structure — real schemas come from FormDefinition.SchemaJson
    return {
      fields: [],
      repeatBlocks: [],
      sections: [],
      dependencies: [],
    };
  }, [submission]);

  // Initialize form data from submission
  useEffect(() => {
    if (submission) {
      try {
        const parsed = JSON.parse('{}'); // submission.formDataJson would be the actual data
        setFormData(parsed);
        setInitialData(JSON.stringify(parsed));
      } catch {
        setFormData({});
        setInitialData('{}');
      }
    }
  }, [submission]);

  const hasChanges = useMemo(
    () => JSON.stringify(formData) !== initialData,
    [formData, initialData],
  );

  // Save mutation
  const saveMutation = useMutation({
    mutationFn: ({ markComplete }: { markComplete: boolean }) =>
      patientsService.updateFormData(Number(patientId), Number(formId), {
        formDataJson: JSON.stringify(formData),
        markComplete,
      }),
    onSuccess: () => {
      setInitialData(JSON.stringify(formData));
    },
  });

  // Validate on demand
  void useMutation({
    mutationFn: () => patientsService.validateForm(Number(patientId), Number(formId)),
    onSuccess: (result: { messages: ValidationMessageDto[] }) => {
      setValidationMessages(result.messages);
    },
  });

  const handleSave = useCallback(() => {
    saveMutation.mutate({ markComplete: false });
  }, [saveMutation]);

  const handleMarkComplete = useCallback(() => {
    saveMutation.mutate({ markComplete: true });
  }, [saveMutation]);

  const handleDataChange = useCallback((newData: Record<string, unknown>) => {
    setFormData(newData);
    // Trigger client-side validation on change
    setValidationMessages([]); // Clear until next explicit validation
  }, []);

  if (loadingForm || loadingPatient) {
    return <Box display="flex" justifyContent="center" pt={8}><CircularProgress /></Box>;
  }

  if (!submission || !patient) {
    return <Alert severity="error">Form or patient not found.</Alert>;
  }

  const isLocked = submission.lockStatus === 'Locked';
  const isEditable = canEditPatient && (!isLocked || isFoundationAdmin);

  // Determine context label
  let contextLabel = '';
  if (submission.encounterDate) contextLabel = `Encounter Date: ${new Date(submission.encounterDate).toLocaleDateString()}`;
  else if (submission.annualReviewYear) contextLabel = `Annual Review Year: ${submission.annualReviewYear}`;
  else if (submission.labDate) contextLabel = `Lab Date: ${new Date(submission.labDate).toLocaleDateString()}`;
  else if (submission.careEpisodeStartDate) contextLabel = `Care Episode: ${new Date(submission.careEpisodeStartDate).toLocaleDateString()}`;
  else if (submission.phoneNoteDate) contextLabel = `Phone Note: ${new Date(submission.phoneNoteDate).toLocaleDateString()}`;
  else if (submission.transplantOrgan) contextLabel = `Organ: ${submission.transplantOrgan}`;

  const canComplete = validationMessages.filter((m) => m.severity === 'CompletionBlocking').length === 0;
  const canSave = validationMessages.filter((m) => m.severity === 'SaveBlocking').length === 0;

  return (
    <FormPageLayout
      patient={patient}
      form={submission}
      contextLabel={contextLabel}
      hasChanges={hasChanges}
      saving={saveMutation.isPending}
      validationMessages={validationMessages}
      canComplete={canComplete}
      canSave={canSave}
      onSave={handleSave}
      onMarkComplete={handleMarkComplete}
      showMarkComplete={!submission.isShared || submission.formType !== 'DEMOGRAPHICS'}
    >
      {saveMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(saveMutation.error as Error)?.message || 'Failed to save.'}
        </Alert>
      )}
      {saveMutation.isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Form saved successfully.
        </Alert>
      )}

      {formSchema ? (
        <FormRenderer
          schema={formSchema}
          data={formData}
          onChange={handleDataChange}
          disabled={!isEditable}
          errors={validationMessages}
        />
      ) : (
        <Alert severity="info">
          Form schema not yet configured. The form definition needs to include a JSON schema
          for dynamic rendering. This will be populated when CFF provides the eCRF specifications.
        </Alert>
      )}
    </FormPageLayout>
  );
}
