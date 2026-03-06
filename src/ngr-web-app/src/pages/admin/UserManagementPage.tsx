import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import PersonIcon from '@mui/icons-material/Person';
import { useRoles } from '../../hooks/useRoles';
import { useImpersonation } from '../../contexts/ImpersonationContext';
import { apiGet } from '../../services/api';

interface UserListItem {
  id: number;
  oktaId: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  programRoles: { programName: string; roleName: string }[];
}

function fetchUsers(search: string): Promise<UserListItem[]> {
  return apiGet<UserListItem[]>('/api/users', search ? { search } : {});
}

export function UserManagementPage() {
  const { isFoundationAdmin } = useRoles();
  const { isImpersonating, startImpersonation } = useImpersonation();

  const [search,     setSearch]     = useState('');
  const [query,      setQuery]      = useState('');
  const [targetUser, setTargetUser] = useState<UserListItem | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [impersonateError, setImpersonateError] = useState<string | null>(null);
  const [impersonating, setImpersonating] = useState(false);

  if (!isFoundationAdmin) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        Access denied. This page requires the FoundationAnalyst role or higher.
      </Alert>
    );
  }

  const { data: users = [], isLoading } = useQuery<UserListItem[]>({
    queryKey: ['users', query],
    queryFn:  () => fetchUsers(query),
  });

  const handleSearch = () => setQuery(search.trim());

  const handleImpersonateClick = (user: UserListItem) => {
    setTargetUser(user);
    setImpersonateError(null);
    setConfirmOpen(true);
  };

  const handleConfirmImpersonate = async () => {
    if (!targetUser) return;
    setImpersonating(true);
    setImpersonateError(null);
    try {
      await startImpersonation(targetUser.oktaId || targetUser.email);
      setConfirmOpen(false);
    } catch (err) {
      setImpersonateError((err as Error).message ?? 'Failed to start impersonation.');
    } finally {
      setImpersonating(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>User Management</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        View and manage registry users. Foundation Admins can impersonate any active user to
        investigate issues or verify user experiences.
      </Typography>

      {/* Search bar */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <TextField
            label="Search by name or email"
            size="small"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            sx={{ flexGrow: 1, maxWidth: 400 }}
          />
          <Button variant="contained" onClick={handleSearch}>Search</Button>
        </Box>
      </Paper>

      {/* User table */}
      {isLoading ? (
        <CircularProgress size={24} />
      ) : users.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">No users found.</Typography>
        </Paper>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Programs / Roles</TableCell>
                {isFoundationAdmin && <TableCell align="center">Actions</TableCell>}
              </TableRow>
            </TableHead>
            <TableBody>
              {users.map((user) => (
                <TableRow key={user.id} hover>
                  <TableCell>{user.firstName} {user.lastName}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>
                    <Chip
                      label={user.isActive ? 'Active' : 'Inactive'}
                      color={user.isActive ? 'success' : 'default'}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>
                    {user.programRoles?.map((pr, i) => (
                      <Typography key={i} variant="caption" display="block">
                        {pr.programName} — {pr.roleName}
                      </Typography>
                    ))}
                  </TableCell>
                  {isFoundationAdmin && (
                    <TableCell align="center">
                      <Tooltip
                        title={
                          isImpersonating ? 'Exit current impersonation before starting a new one' :
                          !user.isActive ? 'Cannot impersonate an inactive user' :
                          'Impersonate this user'
                        }
                      >
                        <span>
                          <Button
                            size="small"
                            variant="outlined"
                            color="warning"
                            startIcon={<PersonIcon />}
                            disabled={isImpersonating || !user.isActive}
                            onClick={() => handleImpersonateClick(user)}
                          >
                            Impersonate
                          </Button>
                        </span>
                      </Tooltip>
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Impersonation confirmation dialog */}
      <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Impersonate User?</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            You are about to impersonate{' '}
            <strong>{targetUser?.firstName} {targetUser?.lastName}</strong>{' '}
            ({targetUser?.email}). Your Foundation Admin privileges will be suspended for the
            duration. The session expires in 60 minutes.
          </Alert>
          <Typography variant="body2">
            All actions taken during this session will be attributed to both you (acting admin)
            and the impersonated user in the audit trail.
          </Typography>
          {impersonateError && (
            <Alert severity="error" sx={{ mt: 2 }}>{impersonateError}</Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="warning"
            disabled={impersonating}
            onClick={handleConfirmImpersonate}
            startIcon={impersonating ? <CircularProgress size={16} color="inherit" /> : <PersonIcon />}
          >
            {impersonating ? 'Starting…' : 'Continue'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
