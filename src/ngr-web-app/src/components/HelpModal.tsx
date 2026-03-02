import { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  CircularProgress,
  Dialog,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { helpPagesService } from '../services/helpPages';
import { useRoles } from '../hooks/useRoles';
import type { HelpPageDto } from '../types';

// ── Map URL paths to help context keys ──────────────────────────

const PATH_TO_CONTEXT: Record<string, string> = {
  '/': 'dashboard',
  '/patients': 'patient-roster',
  '/programs': 'care-programs',
  '/reports': 'reporting',
  '/import': 'emr-upload',
  '/user-management': 'user-management',
  '/admin/announcements': 'announcements',
  '/admin/help-pages': 'help-pages',
};

function getContextKey(pathname: string): string {
  // Try exact match first
  if (PATH_TO_CONTEXT[pathname]) return PATH_TO_CONTEXT[pathname];
  // Try prefix match
  for (const [path, key] of Object.entries(PATH_TO_CONTEXT)) {
    if (path !== '/' && pathname.startsWith(path)) return key;
  }
  return 'general';
}

interface HelpModalProps {
  open: boolean;
  onClose: () => void;
}

export function HelpModal({ open, onClose }: HelpModalProps) {
  const location = useLocation();
  const { isFoundationAdmin } = useRoles();
  const [selectedPage, setSelectedPage] = useState<HelpPageDto | null>(null);

  const contextKey = getContextKey(location.pathname);

  // Fetch help page tree
  const { data: helpTree = [], isLoading: treeLoading } = useQuery({
    queryKey: ['help-pages', 'tree', isFoundationAdmin],
    queryFn: () => helpPagesService.getTree(isFoundationAdmin),
    enabled: open,
  });

  // Fetch context-specific help page
  const { data: contextPage } = useQuery({
    queryKey: ['help-pages', 'context', contextKey],
    queryFn: () => helpPagesService.getByContextKey(contextKey).catch(() => null),
    enabled: open,
  });

  // Auto-select context-relevant page when opening
  useEffect(() => {
    if (open && contextPage) {
      setSelectedPage(contextPage);
    }
  }, [open, contextPage]);

  const renderHelpTree = (pages: HelpPageDto[], depth = 0) => (
    <List component="nav" disablePadding>
      {pages.map((page) => (
        <Box key={page.id}>
          <ListItemButton
            selected={selectedPage?.id === page.id}
            onClick={() => setSelectedPage(page)}
            sx={{ pl: 2 + depth * 2 }}
          >
            <ListItemText
              primary={page.title}
              primaryTypographyProps={{
                variant: 'body2',
                fontWeight: selectedPage?.id === page.id ? 600 : 400,
              }}
            />
            {!page.isPublished && (
              <Typography variant="caption" color="warning.main" sx={{ ml: 1 }}>
                Draft
              </Typography>
            )}
          </ListItemButton>
          {page.children && page.children.length > 0 && renderHelpTree(page.children, depth + 1)}
        </Box>
      ))}
    </List>
  );

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="lg"
      fullWidth
      aria-labelledby="help-dialog-title"
      PaperProps={{ sx: { height: '80vh' } }}
    >
      <DialogTitle id="help-dialog-title" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Typography variant="h6" sx={{ flexGrow: 1 }}>
          Help
        </Typography>
        <IconButton onClick={onClose} aria-label="Close help" size="small">
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <Divider />
      <DialogContent sx={{ display: 'flex', p: 0, overflow: 'hidden' }}>
        {/* ── Help navigation tree ─────────────────────────────── */}
        <Box
          sx={{
            width: 260,
            minWidth: 260,
            borderRight: '1px solid',
            borderColor: 'divider',
            overflowY: 'auto',
          }}
        >
          {treeLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress size={24} />
            </Box>
          ) : helpTree.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
              No help pages available.
            </Typography>
          ) : (
            renderHelpTree(helpTree)
          )}
        </Box>

        {/* ── Help content ─────────────────────────────────────── */}
        <Box sx={{ flexGrow: 1, overflowY: 'auto', p: 3 }}>
          {selectedPage ? (
            <Box>
              <Typography variant="h5" gutterBottom fontWeight={600}>
                {selectedPage.title}
              </Typography>
              <Box
                dangerouslySetInnerHTML={{ __html: selectedPage.content }}
                sx={{
                  '& img': { maxWidth: '100%', height: 'auto' },
                  '& video': { maxWidth: '100%', height: 'auto' },
                  '& a': { color: 'primary.light' },
                  '& h2, & h3, & h4': { mt: 3, mb: 1 },
                  '& p': { mb: 1.5 },
                  '& ul, & ol': { pl: 3 },
                }}
              />
              {selectedPage.attachments && selectedPage.attachments.length > 0 && (
                <Box sx={{ mt: 3 }}>
                  <Typography variant="subtitle2" gutterBottom>
                    Attachments
                  </Typography>
                  {selectedPage.attachments.map((att) => (
                    <Typography key={att.id} variant="body2">
                      <a href={att.downloadUrl} target="_blank" rel="noopener noreferrer">
                        {att.fileName}
                      </a>{' '}
                      ({(att.fileSize / 1024 / 1024).toFixed(1)} MB)
                    </Typography>
                  ))}
                </Box>
              )}
            </Box>
          ) : (
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%' }}>
              <Typography color="text.secondary">
                Select a help topic from the menu to view its content.
              </Typography>
            </Box>
          )}
        </Box>
      </DialogContent>
    </Dialog>
  );
}
