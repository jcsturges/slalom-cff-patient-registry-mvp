import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PreviewIcon from '@mui/icons-material/Preview';
import CloseIcon from '@mui/icons-material/Close';
import { announcementsService } from '../../services/announcements';
import { useRoles } from '../../hooks/useRoles';
import { RichTextEditor } from '../../components/RichTextEditor';
import type { AnnouncementDto, CreateAnnouncementDto, UpdateAnnouncementDto } from '../../types';

function formatET(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    timeZone: 'America/New_York',
  });
}

export function AnnouncementManagerPage() {
  const { isFoundationAdmin } = useRoles();
  const queryClient = useQueryClient();

  const [editing, setEditing] = useState<AnnouncementDto | null>(null);
  const [isNew, setIsNew] = useState(false);
  const [previewHtml, setPreviewHtml] = useState<string | null>(null);

  // Form state
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [effectiveDate, setEffectiveDate] = useState('');
  const [expirationDate, setExpirationDate] = useState('');

  const { data: announcements = [], isLoading, error } = useQuery({
    queryKey: ['announcements', 'all'],
    queryFn: () => announcementsService.getAll(true),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateAnnouncementDto) => announcementsService.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['announcements'] });
      handleCloseEditor();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateAnnouncementDto }) =>
      announcementsService.update(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['announcements'] });
      handleCloseEditor();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => announcementsService.delete(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['announcements'] }),
  });

  const handleCreate = () => {
    setIsNew(true);
    setEditing(null);
    setTitle('');
    setContent('');
    setEffectiveDate(new Date().toISOString().slice(0, 10));
    setExpirationDate('');
  };

  const handleEdit = (ann: AnnouncementDto) => {
    setIsNew(false);
    setEditing(ann);
    setTitle(ann.title);
    setContent(ann.content);
    setEffectiveDate(ann.effectiveDate.slice(0, 10));
    setExpirationDate(ann.expirationDate?.slice(0, 10) ?? '');
  };

  const handleCloseEditor = () => {
    setEditing(null);
    setIsNew(false);
  };

  const handleSave = () => {
    if (isNew) {
      createMutation.mutate({
        title,
        content,
        effectiveDate,
        expirationDate: expirationDate || undefined,
      });
    } else if (editing) {
      updateMutation.mutate({
        id: editing.id,
        data: {
          title,
          content,
          effectiveDate,
          expirationDate: expirationDate || undefined,
          isActive: editing.isActive,
        },
      });
    }
  };

  const handleDelete = (ann: AnnouncementDto) => {
    if (window.confirm(`Delete announcement "${ann.title}"? This cannot be undone.`)) {
      deleteMutation.mutate(ann.id);
    }
  };

  const handleToggleActive = (ann: AnnouncementDto) => {
    updateMutation.mutate({
      id: ann.id,
      data: {
        title: ann.title,
        content: ann.content,
        effectiveDate: ann.effectiveDate,
        expirationDate: ann.expirationDate ?? undefined,
        isActive: !ann.isActive,
      },
    });
  };

  if (!isFoundationAdmin) {
    return (
      <Alert severity="error">
        You need the Foundation Administrator role to manage announcements.
      </Alert>
    );
  }

  const showEditor = isNew || editing !== null;

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Announcements</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreate}>
          Create Announcement
        </Button>
      </Stack>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        All times in Eastern Time (ET)
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(error as Error).message}
        </Alert>
      )}

      {isLoading ? (
        <Typography color="text.secondary">Loading...</Typography>
      ) : announcements.length === 0 ? (
        <Typography color="text.secondary">No announcements yet.</Typography>
      ) : (
        announcements.map((ann) => (
          <Card key={ann.id} sx={{ mb: 2 }}>
            <CardContent>
              <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                <Box sx={{ flexGrow: 1 }}>
                  <Stack direction="row" alignItems="center" gap={1} mb={1}>
                    <Typography variant="h6" fontWeight={600}>
                      {ann.title}
                    </Typography>
                    <Chip
                      label={ann.isActive ? 'Active' : 'Inactive'}
                      color={ann.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </Stack>
                  <Typography variant="caption" color="text.secondary">
                    Effective: {formatET(ann.effectiveDate)}
                    {ann.expirationDate && ` — Expires: ${formatET(ann.expirationDate)}`}
                  </Typography>
                </Box>
                <Stack direction="row" spacing={0.5}>
                  <IconButton
                    size="small"
                    onClick={() => setPreviewHtml(ann.content)}
                    aria-label="Preview"
                  >
                    <PreviewIcon fontSize="small" />
                  </IconButton>
                  <IconButton size="small" onClick={() => handleEdit(ann)} aria-label="Edit">
                    <EditIcon fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => handleDelete(ann)}
                    aria-label="Delete"
                    color="error"
                  >
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Stack>
              </Stack>
              <Button
                size="small"
                sx={{ mt: 1 }}
                onClick={() => handleToggleActive(ann)}
              >
                {ann.isActive ? 'Deactivate' : 'Activate'}
              </Button>
            </CardContent>
          </Card>
        ))
      )}

      {/* ── Editor Dialog ─────────────────────────────────────── */}
      <Dialog
        open={showEditor}
        onClose={handleCloseEditor}
        maxWidth="md"
        fullWidth
        aria-labelledby="announcement-editor-title"
      >
        <DialogTitle id="announcement-editor-title">
          {isNew ? 'Create Announcement' : 'Edit Announcement'}
        </DialogTitle>
        <Divider />
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              fullWidth
              size="small"
            />
            <Stack direction="row" spacing={2}>
              <TextField
                label="Effective Date"
                type="date"
                value={effectiveDate}
                onChange={(e) => setEffectiveDate(e.target.value)}
                required
                size="small"
                InputLabelProps={{ shrink: true }}
                sx={{ flex: 1 }}
              />
              <TextField
                label="Expiration Date (optional)"
                type="date"
                value={expirationDate}
                onChange={(e) => setExpirationDate(e.target.value)}
                size="small"
                InputLabelProps={{ shrink: true }}
                sx={{ flex: 1 }}
                helperText="Auto-deactivates at 11:59 PM ET"
              />
            </Stack>
            <Typography variant="subtitle2">Content</Typography>
            <RichTextEditor
              content={content}
              onChange={setContent}
              placeholder="Write your announcement..."
              maxLength={5000}
              minHeight={250}
            />
          </Box>
          {(createMutation.isError || updateMutation.isError) && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {((createMutation.error || updateMutation.error) as Error)?.message || 'Failed to save.'}
            </Alert>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setPreviewHtml(content)} startIcon={<PreviewIcon />}>
            Preview
          </Button>
          <Box sx={{ flexGrow: 1 }} />
          <Button onClick={handleCloseEditor}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={!title.trim() || !content.trim() || !effectiveDate || createMutation.isPending || updateMutation.isPending}
          >
            {isNew ? 'Create' : 'Save Changes'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* ── Preview Dialog ────────────────────────────────────── */}
      <Dialog
        open={previewHtml !== null}
        onClose={() => setPreviewHtml(null)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center' }}>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Preview
          </Typography>
          <IconButton onClick={() => setPreviewHtml(null)} size="small" aria-label="Close preview">
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <Divider />
        <DialogContent>
          {previewHtml && (
            <Box
              dangerouslySetInnerHTML={{ __html: previewHtml }}
              sx={{
                '& img': { maxWidth: '100%' },
                '& a': { color: 'primary.light' },
              }}
            />
          )}
        </DialogContent>
      </Dialog>
    </Box>
  );
}
