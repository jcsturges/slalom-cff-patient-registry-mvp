import {
  Alert,
  Box,
  Button,
  IconButton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';

export interface CareEpisodeSegment {
  startDateTime: string;
  endDateTime: string;
}

interface CareEpisodeSegmentEditorProps {
  segments: CareEpisodeSegment[];
  onChange: (segments: CareEpisodeSegment[]) => void;
  disabled?: boolean;
  validationErrors?: string[];
}

export function CareEpisodeSegmentEditor({
  segments,
  onChange,
  disabled = false,
  validationErrors = [],
}: CareEpisodeSegmentEditorProps) {
  const handleAdd = () => {
    onChange([...segments, { startDateTime: '', endDateTime: '' }]);
  };

  const handleRemove = (index: number) => {
    if (!window.confirm('Delete this segment?')) return;
    onChange(segments.filter((_, i) => i !== index));
  };

  const handleChange = (index: number, field: keyof CareEpisodeSegment, value: string) => {
    const updated = [...segments];
    updated[index] = { ...updated[index], [field]: value };
    onChange(updated);
  };

  // Calculate overall episode dates
  const starts = segments
    .map((s) => s.startDateTime)
    .filter(Boolean)
    .map((d) => new Date(d).getTime());
  const ends = segments
    .map((s) => s.endDateTime)
    .filter(Boolean)
    .map((d) => new Date(d).getTime());
  const hasOpenEnded = segments.some((s) => s.startDateTime && !s.endDateTime);

  const episodeStart = starts.length > 0 ? new Date(Math.min(...starts)).toLocaleString() : '—';
  const episodeEnd = hasOpenEnded ? 'Open' : ends.length > 0 ? new Date(Math.max(...ends)).toLocaleString() : '—';

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <Box>
          <Typography variant="h6" fontWeight={600}>Care Episode Segments</Typography>
          <Typography variant="body2" color="text.secondary">
            Episode: {episodeStart} — {episodeEnd}
          </Typography>
        </Box>
        {!disabled && (
          <Button size="small" startIcon={<AddIcon />} onClick={handleAdd}>
            Add Segment
          </Button>
        )}
      </Stack>

      {validationErrors.length > 0 && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {validationErrors.map((err, i) => (
            <Typography key={i} variant="body2">{err}</Typography>
          ))}
        </Alert>
      )}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>#</TableCell>
              <TableCell>Start Date/Time</TableCell>
              <TableCell>End Date/Time</TableCell>
              <TableCell>Status</TableCell>
              {!disabled && <TableCell align="right">Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {segments.length === 0 ? (
              <TableRow>
                <TableCell colSpan={disabled ? 4 : 5} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    No segments. Add a segment to begin.
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              segments.map((seg, idx) => (
                <TableRow key={idx}>
                  <TableCell>{idx + 1}</TableCell>
                  <TableCell>
                    <TextField
                      type="datetime-local"
                      value={seg.startDateTime}
                      onChange={(e) => handleChange(idx, 'startDateTime', e.target.value)}
                      disabled={disabled}
                      size="small"
                      required
                      InputLabelProps={{ shrink: true }}
                      sx={{ minWidth: 200 }}
                    />
                  </TableCell>
                  <TableCell>
                    <TextField
                      type="datetime-local"
                      value={seg.endDateTime}
                      onChange={(e) => handleChange(idx, 'endDateTime', e.target.value)}
                      disabled={disabled}
                      size="small"
                      InputLabelProps={{ shrink: true }}
                      sx={{ minWidth: 200 }}
                      helperText={!seg.endDateTime ? 'Open-ended' : ''}
                    />
                  </TableCell>
                  <TableCell>
                    {seg.endDateTime ? (
                      <Typography variant="body2" color="success.main">Closed</Typography>
                    ) : (
                      <Typography variant="body2" color="warning.main">Open</Typography>
                    )}
                  </TableCell>
                  {!disabled && (
                    <TableCell align="right">
                      <IconButton size="small" color="error" onClick={() => handleRemove(idx)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  )}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
