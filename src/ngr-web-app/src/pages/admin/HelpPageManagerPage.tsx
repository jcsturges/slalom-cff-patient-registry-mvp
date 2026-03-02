import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
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
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  type SelectChangeEvent,
  Stack,
  Switch,
  FormControlLabel,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PreviewIcon from '@mui/icons-material/Preview';
import CloseIcon from '@mui/icons-material/Close';
import { helpPagesService } from '../../services/helpPages';
import { useRoles } from '../../hooks/useRoles';
import { RichTextEditor } from '../../components/RichTextEditor';
import type { HelpPageDto, CreateHelpPageDto, UpdateHelpPageDto } from '../../types';

function slugify(text: string): string {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');
}

export function HelpPageManagerPage() {
  const { isFoundationAdmin } = useRoles();
  const queryClient = useQueryClient();

  const [editing, setEditing] = useState<HelpPageDto | null>(null);
  const [isNew, setIsNew] = useState(false);
  const [previewHtml, setPreviewHtml] = useState<string | null>(null);

  // Form state
  const [title, setTitle] = useState('');
  const [slug, setSlug] = useState('');
  const [content, setContent] = useState('');
  const [parentId, setParentId] = useState<number | ''>('');
  const [sortOrder, setSortOrder] = useState(0);
  const [isPublished, setIsPublished] = useState(false);
  const [contextKey, setContextKey] = useState('');

  const { data: helpPages = [], isLoading, error } = useQuery({
    queryKey: ['help-pages', 'all'],
    queryFn: () => helpPagesService.getAll(true),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateHelpPageDto) => helpPagesService.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['help-pages'] });
      handleCloseEditor();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateHelpPageDto }) =>
      helpPagesService.update(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['help-pages'] });
      handleCloseEditor();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => helpPagesService.delete(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['help-pages'] }),
  });

  const handleCreate = () => {
    setIsNew(true);
    setEditing(null);
    setTitle('');
    setSlug('');
    setContent('');
    setParentId('');
    setSortOrder(0);
    setIsPublished(false);
    setContextKey('');
  };

  const handleEdit = (page: HelpPageDto) => {
    setIsNew(false);
    setEditing(page);
    setTitle(page.title);
    setSlug(page.slug);
    setContent(page.content);
    setParentId(page.parentId ?? '');
    setSortOrder(page.sortOrder);
    setIsPublished(page.isPublished);
    setContextKey(page.contextKey ?? '');
  };

  const handleCloseEditor = () => {
    setEditing(null);
    setIsNew(false);
  };

  const handleTitleChange = (val: string) => {
    setTitle(val);
    if (isNew) setSlug(slugify(val));
  };

  const handleSave = () => {
    const data = {
      title,
      slug,
      content,
      parentId: parentId === '' ? undefined : (parentId as number),
      sortOrder,
      isPublished,
      contextKey: contextKey || undefined,
    };

    if (isNew) {
      createMutation.mutate(data);
    } else if (editing) {
      updateMutation.mutate({ id: editing.id, data });
    }
  };

  const handleDelete = (page: HelpPageDto) => {
    if (window.confirm(`Delete help page "${page.title}"? This cannot be undone.`)) {
      deleteMutation.mutate(page.id);
    }
  };

  const handleTogglePublish = (page: HelpPageDto) => {
    updateMutation.mutate({
      id: page.id,
      data: {
        title: page.title,
        slug: page.slug,
        content: page.content,
        parentId: page.parentId ?? undefined,
        sortOrder: page.sortOrder,
        isPublished: !page.isPublished,
        contextKey: page.contextKey ?? undefined,
      },
    });
  };

  if (!isFoundationAdmin) {
    return (
      <Alert severity="error">
        You need the Foundation Administrator role to manage help pages.
      </Alert>
    );
  }

  // Build flat list with parent info for the parent dropdown
  const topLevelPages = helpPages.filter((p) => !p.parentId);

  const showEditor = isNew || editing !== null;

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Help Pages</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreate}>
          Create Help Page
        </Button>
      </Stack>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(error as Error).message}
        </Alert>
      )}

      {isLoading ? (
        <Typography color="text.secondary">Loading...</Typography>
      ) : helpPages.length === 0 ? (
        <Typography color="text.secondary">No help pages yet.</Typography>
      ) : (
        helpPages.map((page) => (
          <Box
            key={page.id}
            sx={{
              display: 'flex',
              alignItems: 'center',
              py: 1.5,
              px: 2,
              borderBottom: '1px solid',
              borderColor: 'divider',
              pl: page.parentId ? 5 : 2,
              '&:hover': { bgcolor: '#EBF2FA' },
            }}
          >
            <Box sx={{ flexGrow: 1 }}>
              <Stack direction="row" alignItems="center" gap={1}>
                <Typography variant="body1" fontWeight={500}>
                  {page.title}
                </Typography>
                <Chip
                  label={page.isPublished ? 'Published' : 'Draft'}
                  color={page.isPublished ? 'success' : 'default'}
                  size="small"
                  variant="outlined"
                />
                {page.contextKey && (
                  <Chip label={`ctx: ${page.contextKey}`} size="small" variant="outlined" />
                )}
              </Stack>
              <Typography variant="caption" color="text.secondary">
                /{page.slug} — Sort: {page.sortOrder}
              </Typography>
            </Box>
            <Stack direction="row" spacing={0.5}>
              <IconButton
                size="small"
                onClick={() => setPreviewHtml(page.content)}
                aria-label="Preview"
              >
                <PreviewIcon fontSize="small" />
              </IconButton>
              <IconButton size="small" onClick={() => handleEdit(page)} aria-label="Edit">
                <EditIcon fontSize="small" />
              </IconButton>
              <Button size="small" onClick={() => handleTogglePublish(page)}>
                {page.isPublished ? 'Unpublish' : 'Publish'}
              </Button>
              <IconButton
                size="small"
                onClick={() => handleDelete(page)}
                aria-label="Delete"
                color="error"
              >
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Stack>
          </Box>
        ))
      )}

      {/* ── Editor Dialog ─────────────────────────────────────── */}
      <Dialog
        open={showEditor}
        onClose={handleCloseEditor}
        maxWidth="md"
        fullWidth
        aria-labelledby="help-editor-title"
      >
        <DialogTitle id="help-editor-title">
          {isNew ? 'Create Help Page' : 'Edit Help Page'}
        </DialogTitle>
        <Divider />
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Title"
              value={title}
              onChange={(e) => handleTitleChange(e.target.value)}
              required
              fullWidth
              size="small"
            />
            <TextField
              label="Slug (URL path)"
              value={slug}
              onChange={(e) => setSlug(e.target.value)}
              required
              fullWidth
              size="small"
              helperText="URL-friendly identifier"
            />
            <Stack direction="row" spacing={2}>
              <FormControl size="small" sx={{ flex: 1 }}>
                <InputLabel>Parent Page</InputLabel>
                <Select
                  value={parentId}
                  label="Parent Page"
                  onChange={(e: SelectChangeEvent<number | ''>) => setParentId(e.target.value as number | '')}
                >
                  <MenuItem value="">None (Top Level)</MenuItem>
                  {topLevelPages
                    .filter((p) => p.id !== editing?.id)
                    .map((p) => (
                      <MenuItem key={p.id} value={p.id}>
                        {p.title}
                      </MenuItem>
                    ))}
                </Select>
              </FormControl>
              <TextField
                label="Sort Order"
                type="number"
                value={sortOrder}
                onChange={(e) => setSortOrder(Number(e.target.value))}
                size="small"
                sx={{ width: 120 }}
              />
            </Stack>
            <Stack direction="row" spacing={2} alignItems="center">
              <TextField
                label="Context Key (optional)"
                value={contextKey}
                onChange={(e) => setContextKey(e.target.value)}
                size="small"
                sx={{ flex: 1 }}
                helperText="Maps this page to a specific application screen"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={isPublished}
                    onChange={(e) => setIsPublished(e.target.checked)}
                  />
                }
                label="Published"
              />
            </Stack>
            <Typography variant="subtitle2">Content</Typography>
            <RichTextEditor
              content={content}
              onChange={setContent}
              placeholder="Write your help page content..."
              maxLength={50000}
              minHeight={300}
            />
            <Typography variant="caption" color="text.secondary">
              File attachments: Use the API to upload files (max 50 MB, videos max 300 MB).
              Reference uploaded files via their URL in the content.
            </Typography>
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
            disabled={!title.trim() || !slug.trim() || !content.trim() || createMutation.isPending || updateMutation.isPending}
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
                '& video': { maxWidth: '100%' },
                '& a': { color: 'primary.light' },
              }}
            />
          )}
        </DialogContent>
      </Dialog>
    </Box>
  );
}
