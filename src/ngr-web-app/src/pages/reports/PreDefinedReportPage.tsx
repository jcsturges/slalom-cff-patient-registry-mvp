import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import { reportsService } from '../../services/reports';
import { useProgram } from '../../contexts/ProgramContext';
import { ReportResultsView } from '../../components/ReportResultsView';
import type { ReportResultDto } from '../../types';

type ReportKind =
  | 'incomplete_records'
  | 'patients_due_visit'
  | 'diabetes_testing'
  | 'program_list'
  | 'merge_report'
  | 'transfer_report'
  | 'file_upload_report'
  | 'user_management_audit'
  | 'download_audit';

const REPORT_CONFIG: Record<ReportKind, { title: string; description: string; needsProgram: boolean; needsYear: boolean; needsDateRange: boolean }> = {
  incomplete_records: { title: 'Incomplete Records', description: 'View incomplete unlocked forms for the selected reporting year.', needsProgram: true, needsYear: true, needsDateRange: false },
  patients_due_visit: { title: 'Patients Due Visit', description: 'Patients overdue for a visit based on last encounter date.', needsProgram: true, needsYear: false, needsDateRange: false },
  diabetes_testing: { title: 'Diabetes Testing', description: 'Patients ≥10 years who need diabetes testing.', needsProgram: true, needsYear: false, needsDateRange: false },
  program_list: { title: 'CF Care Program List', description: 'All programs with stats.', needsProgram: false, needsYear: false, needsDateRange: false },
  merge_report: { title: 'Duplicate Record Merge Report', description: 'Recent merge operations.', needsProgram: false, needsYear: false, needsDateRange: false },
  transfer_report: { title: 'Patient Transfer Report', description: 'Patient program association changes.', needsProgram: false, needsYear: false, needsDateRange: false },
  file_upload_report: { title: 'File Upload Report', description: 'All file uploads by program.', needsProgram: false, needsYear: false, needsDateRange: false },
  user_management_audit: { title: 'User Management Audit', description: 'User creation, deactivation, role changes within date range.', needsProgram: false, needsYear: false, needsDateRange: true },
  download_audit: { title: 'Download Details Audit', description: 'Report and file downloads within date range.', needsProgram: false, needsYear: false, needsDateRange: true },
};

export function PreDefinedReportPage({ reportKind }: { reportKind: ReportKind }) {
  const { selectedProgram } = useProgram();
  const config = REPORT_CONFIG[reportKind];
  const currentYear = new Date().getFullYear();

  const [reportingYear, setReportingYear] = useState(currentYear);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [result, setResult] = useState<ReportResultDto | null>(null);

  const executeMutation = useMutation({
    mutationFn: async () => {
      const programId = selectedProgram?.id ?? 0;
      switch (reportKind) {
        case 'incomplete_records': return reportsService.incompleteRecords(programId, reportingYear);
        case 'patients_due_visit': return reportsService.patientsDueVisit(programId);
        case 'diabetes_testing': return reportsService.diabetesTesting(programId);
        case 'program_list': return reportsService.programList();
        case 'merge_report': return reportsService.mergeReport();
        case 'transfer_report': return reportsService.transferReport();
        case 'file_upload_report': return reportsService.fileUploadReport();
        case 'user_management_audit': return reportsService.userManagementAudit(startDate, endDate);
        case 'download_audit': return reportsService.downloadAudit(startDate, endDate);
      }
    },
    onSuccess: (data) => setResult(data),
  });

  const handleDownload = (format: 'csv' | 'excel') => {
    if (result) {
      reportsService.downloadReport({ executionId: result.executionId, format });
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{config.title}</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>{config.description}</Typography>

      <Stack direction="row" spacing={2} sx={{ mb: 3 }} alignItems="flex-end">
        {config.needsYear && (
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Year</InputLabel>
            <Select value={reportingYear} label="Year" onChange={(e) => setReportingYear(Number(e.target.value))}>
              {[currentYear, currentYear - 1, currentYear - 2].map((y) => (
                <MenuItem key={y} value={y}>{y}</MenuItem>
              ))}
            </Select>
          </FormControl>
        )}
        {config.needsDateRange && (
          <>
            <TextField label="Start Date" type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} size="small" InputLabelProps={{ shrink: true }} />
            <TextField label="End Date" type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} size="small" InputLabelProps={{ shrink: true }} />
          </>
        )}
        <Button
          variant="contained"
          startIcon={executeMutation.isPending ? <CircularProgress size={16} /> : <PlayArrowIcon />}
          onClick={() => executeMutation.mutate()}
          disabled={executeMutation.isPending || (config.needsDateRange && (!startDate || !endDate))}
        >
          Run Report
        </Button>
      </Stack>

      {executeMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(executeMutation.error as Error)?.message || 'Failed to run report.'}
        </Alert>
      )}

      {result && <ReportResultsView result={result} onDownload={handleDownload} />}
    </Box>
  );
}
