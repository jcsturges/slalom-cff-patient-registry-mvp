import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Stack,
  Tab,
  Tabs,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import CloseIcon from '@mui/icons-material/Close';

export interface SubFormDefinition {
  id: string;
  label: string;
  /** Whether this sub-form is always required (e.g., General Encounter Start) */
  required?: boolean;
}

interface SubFormTabsProps {
  /** Available sub-form definitions */
  availableSubForms: SubFormDefinition[];
  /** Currently selected sub-form IDs */
  selectedSubForms: string[];
  /** Current active tab */
  activeTab: string;
  /** Sub-form completion status map */
  completionStatus: Record<string, 'complete' | 'incomplete' | 'not_created'>;
  onTabChange: (tabId: string) => void;
  onAddSubForm: (subFormId: string) => void;
  onRemoveSubForm: (subFormId: string) => void;
  disabled?: boolean;
  children: React.ReactNode;
}

export function SubFormTabs({
  availableSubForms,
  selectedSubForms,
  activeTab,
  completionStatus,
  onTabChange,
  onAddSubForm,
  onRemoveSubForm,
  disabled = false,
  children,
}: SubFormTabsProps) {
  const [removeConfirm, setRemoveConfirm] = useState<string | null>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);

  const selectedDefs = availableSubForms.filter((sf) => selectedSubForms.includes(sf.id));
  const unselectedDefs = availableSubForms.filter(
    (sf) => !selectedSubForms.includes(sf.id) && !sf.required,
  );

  const handleRemove = (subFormId: string) => {
    const status = completionStatus[subFormId];
    if (status === 'incomplete' || status === 'complete') {
      // Has data — warn first
      setRemoveConfirm(subFormId);
    } else {
      onRemoveSubForm(subFormId);
    }
  };

  const confirmRemove = () => {
    if (removeConfirm) {
      onRemoveSubForm(removeConfirm);
      setRemoveConfirm(null);
      // Switch to first available tab
      const remaining = selectedSubForms.filter((id) => id !== removeConfirm);
      if (remaining.length > 0) onTabChange(remaining[0]);
    }
  };

  const getStatusColor = (status: string): 'success' | 'error' | 'default' => {
    if (status === 'complete') return 'success';
    if (status === 'incomplete') return 'error';
    return 'default';
  };

  return (
    <Box>
      {/* ── Tab bar ───────────────────────────────────────────── */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', display: 'flex', alignItems: 'center' }}>
        <Tabs
          value={activeTab}
          onChange={(_, val) => onTabChange(val)}
          variant="scrollable"
          scrollButtons="auto"
          sx={{ flexGrow: 1 }}
        >
          {selectedDefs.map((sf) => (
            <Tab
              key={sf.id}
              value={sf.id}
              label={
                <Stack direction="row" alignItems="center" spacing={0.5}>
                  <span>{sf.label}</span>
                  <Chip
                    size="small"
                    variant="filled"
                    color={getStatusColor(completionStatus[sf.id] ?? 'not_created')}
                    sx={{ width: 10, height: 10, minWidth: 10, '& .MuiChip-label': { display: 'none' } }}
                  />
                  {!sf.required && !disabled && (
                    <CloseIcon
                      sx={{ fontSize: 14, opacity: 0.5, '&:hover': { opacity: 1 } }}
                      onClick={(e) => { e.stopPropagation(); handleRemove(sf.id); }}
                    />
                  )}
                </Stack>
              }
            />
          ))}
        </Tabs>
        {!disabled && unselectedDefs.length > 0 && (
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setAddDialogOpen(true)}
            sx={{ ml: 1, whiteSpace: 'nowrap' }}
          >
            Add Tab
          </Button>
        )}
      </Box>

      {/* ── Tab content ───────────────────────────────────────── */}
      <Box sx={{ pt: 2 }}>
        {children}
      </Box>

      {/* ── Add sub-form dialog ───────────────────────────────── */}
      <Dialog open={addDialogOpen} onClose={() => setAddDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Add Sub-Form</DialogTitle>
        <Divider />
        <DialogContent>
          {unselectedDefs.map((sf) => (
            <Button
              key={sf.id}
              fullWidth
              variant="outlined"
              sx={{ mb: 1, justifyContent: 'flex-start' }}
              onClick={() => {
                onAddSubForm(sf.id);
                onTabChange(sf.id);
                setAddDialogOpen(false);
              }}
            >
              {sf.label}
            </Button>
          ))}
        </DialogContent>
      </Dialog>

      {/* ── Remove confirmation dialog ────────────────────────── */}
      <Dialog open={removeConfirm !== null} onClose={() => setRemoveConfirm(null)}>
        <DialogTitle>Remove Sub-Form</DialogTitle>
        <Divider />
        <DialogContent>
          <Alert severity="warning">
            This sub-form contains data that will be permanently deleted. Are you sure?
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRemoveConfirm(null)}>Cancel</Button>
          <Button variant="contained" color="error" onClick={confirmRemove}>
            Remove &amp; Delete Data
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
