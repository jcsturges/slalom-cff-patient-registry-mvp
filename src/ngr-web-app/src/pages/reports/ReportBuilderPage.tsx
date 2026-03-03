import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import SaveIcon from '@mui/icons-material/Save';
import { reportsService } from '../../services/reports';
import { useProgram } from '../../contexts/ProgramContext';
import { ReportResultsView } from '../../components/ReportResultsView';
import type { ReportResultDto, SavedReportDto } from '../../types';

interface Condition {
  field: string;
  operator: string;
  value: string;
}

interface Block {
  type: 'include' | 'exclude';
  conditions: Condition[];
}

const OPERATORS = ['equals', 'not equals', 'contains', 'greater than', 'less than', 'between', 'is empty', 'is not empty'];

export function ReportBuilderPage() {
  const { selectedProgram } = useProgram();
  const [blocks, setBlocks] = useState<Block[]>([{ type: 'include', conditions: [{ field: '', operator: 'equals', value: '' }] }]);
  const [result, setResult] = useState<ReportResultDto | null>(null);
  const [saveDialogOpen, setSaveDialogOpen] = useState(false);
  const [saveTitle, setSaveTitle] = useState('');
  const [saveDesc, setSaveDesc] = useState('');
  const [saveScope, setSaveScope] = useState('my');

  // Load saved reports
  const { data: savedReports = [] } = useQuery({
    queryKey: ['saved-reports'],
    queryFn: () => reportsService.getSavedReports(),
  });

  const executeMutation = useMutation({
    mutationFn: () =>
      reportsService.executeReport({
        programId: selectedProgram?.id,
        queryDefinitionJson: JSON.stringify({ blocks }),
      }),
    onSuccess: (data) => setResult(data),
  });

  const saveMutation = useMutation({
    mutationFn: () =>
      reportsService.createSavedReport({
        title: saveTitle,
        description: saveDesc || undefined,
        scope: saveScope,
        queryDefinitionJson: JSON.stringify({ blocks }),
        programId: selectedProgram?.id,
      }),
    onSuccess: () => {
      setSaveDialogOpen(false);
      setSaveTitle('');
      setSaveDesc('');
    },
  });

  const addBlock = (type: 'include' | 'exclude') => {
    setBlocks([...blocks, { type, conditions: [{ field: '', operator: 'equals', value: '' }] }]);
  };

  const removeBlock = (idx: number) => {
    setBlocks(blocks.filter((_, i) => i !== idx));
  };

  const addCondition = (blockIdx: number) => {
    const updated = [...blocks];
    updated[blockIdx].conditions.push({ field: '', operator: 'equals', value: '' });
    setBlocks(updated);
  };

  const removeCondition = (blockIdx: number, condIdx: number) => {
    const updated = [...blocks];
    updated[blockIdx].conditions = updated[blockIdx].conditions.filter((_, i) => i !== condIdx);
    setBlocks(updated);
  };

  const updateCondition = (blockIdx: number, condIdx: number, field: keyof Condition, value: string) => {
    const updated = [...blocks];
    updated[blockIdx].conditions[condIdx][field] = value;
    setBlocks(updated);
  };

  const loadSavedReport = (_report: SavedReportDto) => {
    try {
      // Would load the query definition JSON — for now just execute
      executeMutation.mutate();
    } catch {
      // Parse error
    }
  };

  const handleDownload = (format: 'csv' | 'excel') => {
    if (result) {
      reportsService.downloadReport({ executionId: result.executionId, format });
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Report Builder</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Build custom queries to select patients and choose which data fields to include in your report.
      </Typography>

      {/* ── Saved Reports List ────────────────────────────────── */}
      {savedReports.length > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" gutterBottom>Saved Reports</Typography>
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
            {savedReports.map((r) => (
              <Button key={r.id} size="small" variant="outlined" onClick={() => loadSavedReport(r)}>
                {r.title}
              </Button>
            ))}
          </Stack>
        </Box>
      )}

      {/* ── Query Blocks ──────────────────────────────────────── */}
      {blocks.map((block, blockIdx) => (
        <Card key={blockIdx} sx={{ mb: 2, borderLeft: `4px solid ${block.type === 'include' ? '#2E7D32' : '#D32F2F'}` }}>
          <CardContent>
            <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
              <Typography variant="subtitle2" color={block.type === 'include' ? 'success.main' : 'error.main'}>
                {block.type === 'include' ? 'INCLUDE patients where' : 'EXCLUDE patients where (AND NOT)'}
              </Typography>
              {blocks.length > 1 && (
                <IconButton size="small" onClick={() => removeBlock(blockIdx)} color="error">
                  <DeleteIcon fontSize="small" />
                </IconButton>
              )}
            </Stack>

            {block.conditions.map((cond, condIdx) => (
              <Stack key={condIdx} direction="row" spacing={1} alignItems="center" sx={{ mb: 1 }}>
                {condIdx > 0 && <Typography variant="caption" sx={{ minWidth: 30 }}>AND</Typography>}
                <TextField
                  size="small"
                  value={cond.field}
                  onChange={(e) => updateCondition(blockIdx, condIdx, 'field', e.target.value)}
                  placeholder="Field name..."
                  sx={{ flex: 2 }}
                />
                <FormControl size="small" sx={{ flex: 1 }}>
                  <Select
                    value={cond.operator}
                    onChange={(e) => updateCondition(blockIdx, condIdx, 'operator', e.target.value)}
                  >
                    {OPERATORS.map((op) => <MenuItem key={op} value={op}>{op}</MenuItem>)}
                  </Select>
                </FormControl>
                <TextField
                  size="small"
                  value={cond.value}
                  onChange={(e) => updateCondition(blockIdx, condIdx, 'value', e.target.value)}
                  placeholder="Value..."
                  sx={{ flex: 2 }}
                />
                {block.conditions.length > 1 && (
                  <IconButton size="small" onClick={() => removeCondition(blockIdx, condIdx)}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                )}
              </Stack>
            ))}

            <Button size="small" startIcon={<AddIcon />} onClick={() => addCondition(blockIdx)}>
              Add Condition
            </Button>
          </CardContent>
        </Card>
      ))}

      {/* ── Block Actions ─────────────────────────────────────── */}
      <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
        <Button variant="outlined" color="success" startIcon={<AddIcon />} onClick={() => addBlock('include')}>
          Add Include Block
        </Button>
        <Button variant="outlined" color="error" startIcon={<AddIcon />} onClick={() => addBlock('exclude')}>
          Add Exclude Block
        </Button>
      </Stack>

      {/* ── Execute / Save ────────────────────────────────────── */}
      <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
        <Button
          variant="contained"
          startIcon={executeMutation.isPending ? <CircularProgress size={16} /> : <PlayArrowIcon />}
          onClick={() => executeMutation.mutate()}
          disabled={executeMutation.isPending}
        >
          Run Report
        </Button>
        <Button
          variant="outlined"
          startIcon={<SaveIcon />}
          onClick={() => setSaveDialogOpen(true)}
        >
          Save Query
        </Button>
      </Stack>

      {executeMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(executeMutation.error as Error)?.message || 'Failed to execute report.'}
        </Alert>
      )}

      {/* ── Results ───────────────────────────────────────────── */}
      {result && (
        <ReportResultsView result={result} onDownload={handleDownload} />
      )}

      {/* ── Save Dialog ───────────────────────────────────────── */}
      <Dialog open={saveDialogOpen} onClose={() => setSaveDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Save Report Query</DialogTitle>
        <Divider />
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField label="Report Title" value={saveTitle} onChange={(e) => setSaveTitle(e.target.value)} required size="small" fullWidth />
            <TextField label="Description" value={saveDesc} onChange={(e) => setSaveDesc(e.target.value)} multiline rows={2} size="small" fullWidth />
            <FormControl size="small">
              <InputLabel>Scope</InputLabel>
              <Select value={saveScope} label="Scope" onChange={(e) => setSaveScope(e.target.value)}>
                <MenuItem value="my">My Reports</MenuItem>
                <MenuItem value="program">My Program's Reports</MenuItem>
                <MenuItem value="global">Global Reports</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setSaveDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={() => saveMutation.mutate()} disabled={!saveTitle || saveMutation.isPending}>
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
