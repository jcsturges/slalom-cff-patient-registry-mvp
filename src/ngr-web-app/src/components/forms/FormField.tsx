import { useState } from 'react';
import {
  Autocomplete,
  Box,
  Checkbox,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormHelperText,
  FormLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Radio,
  RadioGroup,
  Select,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import type { ValidationMessageDto } from '../../types';

/** JSON schema field definition */
export interface FieldSchema {
  id: string;
  type: 'text' | 'textarea' | 'number' | 'date' | 'datetime' | 'radio' | 'checkbox' | 'multiselect' | 'dropdown' | 'mutation_search';
  label: string;
  required?: boolean;
  tooltip?: string;
  maxLength?: number;
  min?: number;
  max?: number;
  options?: { value: string; label: string }[];
  /** For mutation_search: all searchable options */
  searchOptions?: string[];
  /** Dependency: parent field ID */
  dependsOn?: string;
  /** Dependency: required parent value to show this field */
  dependsOnValue?: string;
  /** Whether field is calculated (read-only) */
  calculated?: boolean;
}

interface FormFieldProps {
  schema: FieldSchema;
  value: unknown;
  onChange: (fieldId: string, value: unknown) => void;
  disabled?: boolean;
  errors?: ValidationMessageDto[];
}

export function FormField({ schema, value, onChange, disabled = false, errors = [] }: FormFieldProps) {
  const [searchInput, setSearchInput] = useState('');

  const fieldErrors = errors.filter((e) => e.fieldId === schema.id);
  const hasError = fieldErrors.some((e) => e.severity === 'SaveBlocking' || e.severity === 'CompletionBlocking');
  const hasWarning = fieldErrors.some((e) => e.severity === 'Warning');
  const helperText = fieldErrors[0]?.message;

  const labelWithTooltip = (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
      <span>{schema.label}{schema.required ? ' *' : ''}</span>
      {schema.tooltip && (
        <Tooltip title={schema.tooltip} arrow>
          <IconButton size="small" tabIndex={-1}>
            <HelpOutlineIcon sx={{ fontSize: 16 }} />
          </IconButton>
        </Tooltip>
      )}
    </Box>
  );

  const isReadOnly = disabled || schema.calculated;

  switch (schema.type) {
    case 'text':
      return (
        <TextField
          label={labelWithTooltip}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(schema.id, e.target.value)}
          disabled={isReadOnly}
          error={hasError}
          helperText={helperText}
          fullWidth
          size="small"
          required={schema.required}
          inputProps={{ maxLength: schema.maxLength }}
          color={hasWarning ? 'warning' : undefined}
        />
      );

    case 'textarea':
      return (
        <TextField
          label={labelWithTooltip}
          value={(value as string) ?? ''}
          onChange={(e) => onChange(schema.id, e.target.value)}
          disabled={isReadOnly}
          error={hasError}
          helperText={helperText}
          fullWidth
          multiline
          rows={3}
          required={schema.required}
          inputProps={{ maxLength: schema.maxLength }}
          color={hasWarning ? 'warning' : undefined}
        />
      );

    case 'number':
      return (
        <TextField
          label={labelWithTooltip}
          type="number"
          value={value ?? ''}
          onChange={(e) => onChange(schema.id, e.target.value ? Number(e.target.value) : null)}
          disabled={isReadOnly}
          error={hasError}
          helperText={helperText}
          fullWidth
          size="small"
          required={schema.required}
          inputProps={{ min: schema.min, max: schema.max }}
          color={hasWarning ? 'warning' : undefined}
        />
      );

    case 'date':
      return (
        <TextField
          label={labelWithTooltip}
          type="date"
          value={(value as string) ?? ''}
          onChange={(e) => onChange(schema.id, e.target.value)}
          disabled={isReadOnly}
          error={hasError}
          helperText={helperText}
          fullWidth
          size="small"
          required={schema.required}
          InputLabelProps={{ shrink: true }}
          color={hasWarning ? 'warning' : undefined}
        />
      );

    case 'datetime':
      return (
        <TextField
          label={labelWithTooltip}
          type="datetime-local"
          value={(value as string) ?? ''}
          onChange={(e) => onChange(schema.id, e.target.value)}
          disabled={isReadOnly}
          error={hasError}
          helperText={helperText}
          fullWidth
          size="small"
          required={schema.required}
          InputLabelProps={{ shrink: true }}
          color={hasWarning ? 'warning' : undefined}
        />
      );

    case 'radio':
      return (
        <FormControl error={hasError} disabled={isReadOnly} required={schema.required}>
          <FormLabel>{labelWithTooltip}</FormLabel>
          <RadioGroup
            value={(value as string) ?? ''}
            onChange={(e) => onChange(schema.id, e.target.value)}
            row
          >
            {schema.options?.map((opt) => (
              <FormControlLabel key={opt.value} value={opt.value} control={<Radio size="small" />} label={opt.label} />
            ))}
          </RadioGroup>
          {helperText && <FormHelperText>{helperText}</FormHelperText>}
        </FormControl>
      );

    case 'checkbox':
      return (
        <FormControl error={hasError} disabled={isReadOnly}>
          <FormLabel>{labelWithTooltip}</FormLabel>
          <FormControlLabel
            control={
              <Checkbox
                checked={Boolean(value)}
                onChange={(e) => onChange(schema.id, e.target.checked)}
                size="small"
              />
            }
            label={schema.options?.[0]?.label ?? schema.label}
          />
          {helperText && <FormHelperText>{helperText}</FormHelperText>}
        </FormControl>
      );

    case 'multiselect':
      return (
        <FormControl error={hasError} disabled={isReadOnly}>
          <FormLabel>{labelWithTooltip}</FormLabel>
          <FormGroup row>
            {schema.options?.map((opt) => {
              const selected = Array.isArray(value) ? (value as string[]).includes(opt.value) : false;
              return (
                <FormControlLabel
                  key={opt.value}
                  control={
                    <Checkbox
                      checked={selected}
                      onChange={(e) => {
                        const current = Array.isArray(value) ? (value as string[]) : [];
                        const next = e.target.checked
                          ? [...current, opt.value]
                          : current.filter((v) => v !== opt.value);
                        onChange(schema.id, next);
                      }}
                      size="small"
                    />
                  }
                  label={opt.label}
                />
              );
            })}
          </FormGroup>
          {helperText && <FormHelperText>{helperText}</FormHelperText>}
        </FormControl>
      );

    case 'dropdown':
      return (
        <FormControl fullWidth size="small" error={hasError} disabled={isReadOnly} required={schema.required}>
          <InputLabel>{schema.label}</InputLabel>
          <Select
            value={(value as string) ?? ''}
            label={schema.label}
            onChange={(e) => onChange(schema.id, e.target.value)}
          >
            <MenuItem value="">— Select —</MenuItem>
            {schema.options?.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
            ))}
          </Select>
          {helperText && <FormHelperText>{helperText}</FormHelperText>}
        </FormControl>
      );

    case 'mutation_search':
      return (
        <Autocomplete
          freeSolo
          options={schema.searchOptions ?? []}
          inputValue={searchInput}
          onInputChange={(_, val) => setSearchInput(val)}
          value={(value as string) ?? ''}
          onChange={(_, val) => onChange(schema.id, val)}
          disabled={isReadOnly}
          filterOptions={(opts, { inputValue }) =>
            opts.filter((o) => o.toLowerCase().includes(inputValue.toLowerCase())).slice(0, 50)
          }
          renderInput={(params) => (
            <TextField
              {...params}
              label={labelWithTooltip}
              size="small"
              required={schema.required}
              error={hasError}
              helperText={helperText}
            />
          )}
        />
      );

    default:
      return (
        <Typography variant="body2" color="error">
          Unknown field type: {schema.type}
        </Typography>
      );
  }
}
