import { useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
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
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { FormField, type FieldSchema } from './FormField';
import type { ValidationMessageDto } from '../../types';

export interface RepeatBlockSchema {
  id: string;
  label: string;
  fields: FieldSchema[];
  /** Column IDs to show in collapsed summary table */
  summaryColumns?: string[];
}

interface RepeatBlockProps {
  schema: RepeatBlockSchema;
  /** Array of block data objects */
  blocks: Record<string, unknown>[];
  onChange: (blockId: string, blocks: Record<string, unknown>[]) => void;
  disabled?: boolean;
  errors?: ValidationMessageDto[];
}

export function RepeatBlock({ schema, blocks, onChange, disabled = false, errors = [] }: RepeatBlockProps) {
  const [expanded, setExpanded] = useState<number | null>(blocks.length === 0 ? 0 : null);

  const handleFieldChange = (blockIndex: number, fieldId: string, value: unknown) => {
    const updated = [...blocks];
    updated[blockIndex] = { ...updated[blockIndex], [fieldId]: value };
    onChange(schema.id, updated);
  };

  const handleAddBlock = () => {
    const newBlock: Record<string, unknown> = {};
    schema.fields.forEach((f) => { newBlock[f.id] = null; });
    const updated = [...blocks, newBlock];
    onChange(schema.id, updated);
    setExpanded(updated.length - 1);
  };

  const handleDeleteBlock = (index: number) => {
    if (!window.confirm('Delete this entry? This cannot be undone.')) return;
    const updated = blocks.filter((_, i) => i !== index);
    onChange(schema.id, updated);
    setExpanded(null);
  };

  const summaryFields = schema.summaryColumns
    ? schema.fields.filter((f) => schema.summaryColumns!.includes(f.id))
    : schema.fields.slice(0, 3);

  return (
    <Box sx={{ mb: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
        <Typography variant="subtitle2" fontWeight={600}>{schema.label}</Typography>
        {!disabled && (
          <Button size="small" startIcon={<AddIcon />} onClick={handleAddBlock}>
            Add Entry
          </Button>
        )}
      </Stack>

      {/* Collapsed summary table */}
      {blocks.length > 0 && expanded === null && (
        <TableContainer sx={{ mb: 1 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>#</TableCell>
                {summaryFields.map((f) => (
                  <TableCell key={f.id}>{f.label}</TableCell>
                ))}
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {blocks.map((block, idx) => (
                <TableRow
                  key={idx}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => setExpanded(idx)}
                >
                  <TableCell>{idx + 1}</TableCell>
                  {summaryFields.map((f) => (
                    <TableCell key={f.id}>{String(block[f.id] ?? '—')}</TableCell>
                  ))}
                  <TableCell align="right">
                    {!disabled && (
                      <IconButton
                        size="small"
                        color="error"
                        onClick={(e) => { e.stopPropagation(); handleDeleteBlock(idx); }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Accordion blocks */}
      {blocks.map((block, idx) => (
        <Accordion
          key={idx}
          expanded={expanded === idx}
          onChange={(_, isExpanded) => setExpanded(isExpanded ? idx : null)}
          sx={{ mb: 0.5 }}
        >
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="body2">
              {schema.label} #{idx + 1}
              {summaryFields.length > 0 && expanded !== idx && (
                <Typography component="span" variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                  ({summaryFields.map((f) => `${f.label}: ${block[f.id] ?? '—'}`).join(', ')})
                </Typography>
              )}
            </Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {schema.fields.map((field) => (
                <FormField
                  key={field.id}
                  schema={field}
                  value={block[field.id]}
                  onChange={(fId, val) => handleFieldChange(idx, fId, val)}
                  disabled={disabled}
                  errors={errors}
                />
              ))}
              {!disabled && (
                <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                  <Button size="small" color="error" onClick={() => handleDeleteBlock(idx)}>
                    Delete Entry
                  </Button>
                </Box>
              )}
            </Box>
          </AccordionDetails>
        </Accordion>
      ))}

      {blocks.length === 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
          No entries yet. Click "Add Entry" to begin.
        </Typography>
      )}
    </Box>
  );
}
