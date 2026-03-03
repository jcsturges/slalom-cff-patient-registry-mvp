import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Radio,
  RadioGroup,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import SaveIcon from '@mui/icons-material/Save';
import DeleteIcon from '@mui/icons-material/Delete';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import { dataExportService } from '../services/dataExport';
import { useProgram } from '../contexts/ProgramContext';

const FORM_TYPES = [
  { value: 'ALL', label: 'All Forms' },
  { value: 'DEMOGRAPHICS', label: 'Demographics' },
  { value: 'DIAGNOSIS', label: 'Diagnosis' },
  { value: 'SWEAT_TEST', label: 'Sweat Test & Fecal Elastase' },
  { value: 'TRANSPLANT', label: 'Transplant' },
  { value: 'ALD_INITIATION', label: 'ALD Initiation' },
  { value: 'ANNUAL_REVIEW', label: 'Annual Review' },
  { value: 'ENCOUNTER', label: 'Encounter' },
  { value: 'LABS_TESTS', label: 'Labs and Tests' },
  { value: 'CARE_EPISODE', label: 'Care Episode' },
  { value: 'PHONE_NOTE', label: 'Phone Note' },
];

export function DataExportPage() {
  const { selectedProgram } = useProgram();
  const queryClient = useQueryClient();

  const [selectedForms, setSelectedForms] = useState<string[]>(['ALL']);
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [completeness, setCompleteness] = useState<'all' | 'complete_only'>('all');
  const [diagnosisFilter, setDiagnosisFilter] = useState('');
  const [outputFormat, setOutputFormat] = useState<'coded' | 'descriptive'>('coded');
  const [saveDialogOpen, setSaveDialogOpen] = useState(false);
  const [saveName, setSaveName] = useState('');
  const [saveDesc, setSaveDesc] = useState('');

  const { data: savedDefs = [] } = useQuery({
    queryKey: ['export-definitions', selectedProgram?.id],
    queryFn: () => dataExportService.getDefinitions(selectedProgram?.id),
  });

  const programId = selectedProgram?.id ?? 0;

  const exportMutation = useMutation({
    mutationFn: () =>
      dataExportService.executeExport({
        formTypes: selectedForms,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
        completenessFilter: completeness,
        diagnosisFilter: diagnosisFilter || undefined,
        outputFormat,
        programId,
      }),
  });

  const saveMutation = useMutation({
    mutationFn: () =>
      dataExportService.createDefinition({
        name: saveName,
        description: saveDesc || undefined,
        programId,
        parametersJson: JSON.stringify({
          formTypes: selectedForms,
          dateFrom,
          dateTo,
          completenessFilter: completeness,
          diagnosisFilter,
          outputFormat,
        }),
      }),
    onSuccess: () => {
      setSaveDialogOpen(false);
      setSaveName('');
      setSaveDesc('');
      void queryClient.invalidateQueries({ queryKey: ['export-definitions'] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dataExportService.deleteDefinition(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['export-definitions'] }),
  });

  const handleFormToggle = (value: string) => {
    if (value === 'ALL') {
      setSelectedForms(['ALL']);
    } else {
      setSelectedForms((prev) => {
        const without = prev.filter((f) => f !== 'ALL');
        return without.includes(value) ? without.filter((f) => f !== value) : [...without, value];
      });
    }
  };

  const loadDefinition = (def: { parametersJson: string }) => {
    try {
      const params = JSON.parse(def.parametersJson);
      if (params.formTypes) setSelectedForms(params.formTypes);
      if (params.dateFrom) setDateFrom(params.dateFrom);
      if (params.dateTo) setDateTo(params.dateTo);
      if (params.completenessFilter) setCompleteness(params.completenessFilter);
      if (params.diagnosisFilter) setDiagnosisFilter(params.diagnosisFilter);
      if (params.outputFormat) setOutputFormat(params.outputFormat);
    } catch { /* ignore */ }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
        <DownloadIcon color="primary" />
        <Typography variant="h4">Raw Data Download</Typography>
      </Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Export registry data for {selectedProgram?.name ?? 'your program'} as CSV files packaged in a ZIP archive.
      </Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          {/* Form selection */}
          <FormControl component="fieldset" sx={{ mb: 3 }}>
            <FormLabel>Select eCRFs to export</FormLabel>
            <FormGroup row>
              {FORM_TYPES.map((ft) => (
                <FormControlLabel
                  key={ft.value}
                  control={
                    <Checkbox
                      checked={selectedForms.includes(ft.value)}
                      onChange={() => handleFormToggle(ft.value)}
                      size="small"
                    />
                  }
                  label={ft.label}
                />
              ))}
            </FormGroup>
          </FormControl>

          {/* Date range */}
          <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
            <TextField label="Date From" type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} size="small" InputLabelProps={{ shrink: true }} helperText="Filter by last modified date" />
            <TextField label="Date To" type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} size="small" InputLabelProps={{ shrink: true }} />
          </Stack>

          {/* Completeness */}
          <FormControl sx={{ mb: 3 }}>
            <FormLabel>Completeness</FormLabel>
            <RadioGroup row value={completeness} onChange={(e) => setCompleteness(e.target.value as 'all' | 'complete_only')}>
              <FormControlLabel value="all" control={<Radio size="small" />} label="All records" />
              <FormControlLabel value="complete_only" control={<Radio size="small" />} label="Complete records only" />
            </RadioGroup>
          </FormControl>

          {/* Diagnosis filter */}
          <FormControl size="small" sx={{ mb: 3, minWidth: 200, display: 'block' }}>
            <InputLabel>Diagnosis Filter</InputLabel>
            <Select value={diagnosisFilter} label="Diagnosis Filter" onChange={(e) => setDiagnosisFilter(e.target.value)} sx={{ minWidth: 200 }}>
              <MenuItem value="">All diagnoses</MenuItem>
              <MenuItem value="CF">CF</MenuItem>
              <MenuItem value="CFTR_related">CFTR-related</MenuItem>
              <MenuItem value="CRMS_CFSPID">CRMS/CFSPID</MenuItem>
            </Select>
          </FormControl>

          {/* Output format */}
          <FormControl sx={{ mb: 3 }}>
            <FormLabel>Output Format</FormLabel>
            <RadioGroup row value={outputFormat} onChange={(e) => setOutputFormat(e.target.value as 'coded' | 'descriptive')}>
              <FormControlLabel value="coded" control={<Radio size="small" />} label="As codes (numeric/coded values)" />
              <FormControlLabel value="descriptive" control={<Radio size="small" />} label="As descriptives (human-readable labels)" />
            </RadioGroup>
          </FormControl>

          {/* Actions */}
          <Stack direction="row" spacing={2}>
            <Button variant="contained" startIcon={exportMutation.isPending ? <CircularProgress size={16} /> : <PlayArrowIcon />} onClick={() => exportMutation.mutate()} disabled={exportMutation.isPending || selectedForms.length === 0}>
              Download
            </Button>
            <Button variant="outlined" startIcon={<SaveIcon />} onClick={() => setSaveDialogOpen(true)}>
              Save Definition
            </Button>
          </Stack>

          {exportMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>{(exportMutation.error as Error)?.message || 'Export failed.'}</Alert>
          )}
          {exportMutation.isSuccess && (
            <Alert severity="success" sx={{ mt: 2 }}>Download started.</Alert>
          )}
        </CardContent>
      </Card>

      {/* My Downloads library */}
      {savedDefs.length > 0 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>My Downloads</Typography>
            {savedDefs.map((def) => (
              <Box key={def.id} sx={{ display: 'flex', alignItems: 'center', py: 1, borderBottom: '1px solid', borderColor: 'divider' }}>
                <Box sx={{ flexGrow: 1 }}>
                  <Typography variant="body1" fontWeight={500}>{def.name}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {def.description ?? 'No description'} | Last run: {def.lastExecutedAt ? new Date(def.lastExecutedAt).toLocaleDateString() : 'Never'}
                  </Typography>
                </Box>
                <Button size="small" onClick={() => loadDefinition(def)}>Load</Button>
                <IconButton size="small" color="error" onClick={() => { if (window.confirm('Delete?')) deleteMutation.mutate(def.id); }}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Box>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Save dialog */}
      <Dialog open={saveDialogOpen} onClose={() => setSaveDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Save Download Definition</DialogTitle>
        <Divider />
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField label="Name" value={saveName} onChange={(e) => setSaveName(e.target.value)} required size="small" fullWidth />
            <TextField label="Description" value={saveDesc} onChange={(e) => setSaveDesc(e.target.value)} multiline rows={2} size="small" fullWidth />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setSaveDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={() => saveMutation.mutate()} disabled={!saveName || saveMutation.isPending}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
