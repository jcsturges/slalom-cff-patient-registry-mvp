import { useMemo, useCallback, useState } from 'react';
import { Box, Dialog, DialogActions, DialogContent, DialogTitle, Button, Divider, Grid, Typography } from '@mui/material';
import { FormField, type FieldSchema } from './FormField';
import { RepeatBlock, type RepeatBlockSchema } from './RepeatBlock';
import type { ValidationMessageDto } from '../../types';

/** Top-level form schema parsed from JSON */
export interface FormSchema {
  fields: FieldSchema[];
  repeatBlocks?: RepeatBlockSchema[];
  sections?: { title: string; fieldIds: string[] }[];
  dependencies?: DependencyRule[];
}

export interface DependencyRule {
  parentFieldId: string;
  parentValue: string;
  childFieldIds: string[];
}

interface FormRendererProps {
  schema: FormSchema;
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
  disabled?: boolean;
  errors?: ValidationMessageDto[];
}

export function FormRenderer({ schema, data, onChange, disabled = false, errors = [] }: FormRendererProps) {
  const [depConfirm, setDepConfirm] = useState<{
    parentFieldId: string;
    newValue: unknown;
    childFieldIds: string[];
  } | null>(null);

  // Evaluate field visibility based on dependencies
  const visibleFieldIds = useMemo(() => {
    const visible = new Set(schema.fields.map((f) => f.id));

    // Check dependsOn visibility
    for (const field of schema.fields) {
      if (field.dependsOn) {
        const parentVal = data[field.dependsOn];
        if (field.dependsOnValue && String(parentVal) !== field.dependsOnValue) {
          visible.delete(field.id);
        }
      }
    }

    return visible;
  }, [schema.fields, data]);

  const handleFieldChange = useCallback(
    (fieldId: string, value: unknown) => {
      // Check if this field has dependency rules
      if (schema.dependencies) {
        const depRules = schema.dependencies.filter((d) => d.parentFieldId === fieldId);
        for (const rule of depRules) {
          // If the parent value changed away from the trigger value,
          // warn about clearing dependent data
          const oldValue = data[fieldId];
          if (String(oldValue) === rule.parentValue && String(value) !== rule.parentValue) {
            const hasData = rule.childFieldIds.some((cId) => data[cId] != null && data[cId] !== '');
            if (hasData) {
              setDepConfirm({ parentFieldId: fieldId, newValue: value, childFieldIds: rule.childFieldIds });
              return;
            }
          }
        }
      }

      onChange({ ...data, [fieldId]: value });
    },
    [data, onChange, schema.dependencies],
  );

  const confirmDependencyChange = () => {
    if (!depConfirm) return;
    const updated = { ...data, [depConfirm.parentFieldId]: depConfirm.newValue };
    // Clear dependent fields
    for (const childId of depConfirm.childFieldIds) {
      updated[childId] = null;
    }
    onChange(updated);
    setDepConfirm(null);
  };

  const cancelDependencyChange = () => {
    setDepConfirm(null);
  };

  const handleRepeatBlockChange = useCallback(
    (blockId: string, blocks: Record<string, unknown>[]) => {
      onChange({ ...data, [blockId]: blocks });
    },
    [data, onChange],
  );

  // Render sections if defined, otherwise render all fields in a grid
  const renderFields = (fieldIds?: string[]) => {
    const fields = fieldIds
      ? schema.fields.filter((f) => fieldIds.includes(f.id))
      : schema.fields;

    return (
      <Grid container spacing={2}>
        {fields
          .filter((f) => visibleFieldIds.has(f.id))
          .map((field) => (
            <Grid item xs={12} sm={6} md={4} key={field.id}>
              <FormField
                schema={field}
                value={data[field.id]}
                onChange={handleFieldChange}
                disabled={disabled}
                errors={errors}
              />
            </Grid>
          ))}
      </Grid>
    );
  };

  return (
    <Box>
      {schema.sections ? (
        schema.sections.map((section) => (
          <Box key={section.title} sx={{ mb: 3 }}>
            <Typography variant="h6" fontWeight={600} sx={{ mb: 1.5 }}>
              {section.title}
            </Typography>
            {renderFields(section.fieldIds)}
          </Box>
        ))
      ) : (
        renderFields()
      )}

      {/* Repeat blocks */}
      {schema.repeatBlocks?.map((rb) => (
        <RepeatBlock
          key={rb.id}
          schema={rb}
          blocks={(data[rb.id] as Record<string, unknown>[]) ?? []}
          onChange={handleRepeatBlockChange}
          disabled={disabled}
          errors={errors}
        />
      ))}

      {/* Dependency change confirmation (06-011) */}
      <Dialog open={depConfirm !== null} onClose={cancelDependencyChange}>
        <DialogTitle>Dependent Data Will Be Cleared</DialogTitle>
        <Divider />
        <DialogContent>
          <Typography>
            Changing this field will clear the following dependent data:
          </Typography>
          <Box component="ul" sx={{ mt: 1 }}>
            {depConfirm?.childFieldIds.map((cId) => {
              const field = schema.fields.find((f) => f.id === cId);
              return <li key={cId}>{field?.label ?? cId}</li>;
            })}
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            This cannot be undone. Do you want to proceed?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={cancelDependencyChange}>Cancel (keep current value)</Button>
          <Button variant="contained" color="warning" onClick={confirmDependencyChange}>
            Proceed &amp; Clear Dependent Data
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
