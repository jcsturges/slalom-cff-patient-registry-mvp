import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  CircularProgress,
  Typography,
} from '@mui/material';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import { useProgram } from '../contexts/ProgramContext';

export function ProgramSelectionPage() {
  const navigate = useNavigate();
  const { programs, selectedProgram, selectProgram, loading } = useProgram();

  const handleSelect = (programId: number) => {
    selectProgram(programId);
    navigate('/');
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box maxWidth={720} mx="auto" py={6} px={2}>
      <Box display="flex" alignItems="center" gap={1.5} mb={1}>
        <AccountBalanceIcon color="primary" fontSize="large" />
        <Typography variant="h4" fontWeight={700}>
          Select a Care Program
        </Typography>
      </Box>
      <Typography variant="body1" color="text.secondary" mb={4}>
        Choose the program you want to work in. You can switch programs at any time using the
        selector in the header.
      </Typography>

      <Box display="flex" flexDirection="column" gap={2}>
        {programs.map((program) => {
          const isSelected = selectedProgram?.programId === program.programId;
          return (
            <Card
              key={program.programId}
              variant="outlined"
              sx={{
                borderColor: isSelected ? 'primary.main' : 'divider',
                borderWidth: isSelected ? 2 : 1,
              }}
            >
              <CardActionArea onClick={() => handleSelect(program.programId)}>
                <CardContent>
                  <Box display="flex" alignItems="flex-start" justifyContent="space-between" gap={2}>
                    <Box>
                      <Typography variant="h6" fontWeight={600}>
                        {program.name}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" mt={0.5}>
                        Program ID: {program.programId}
                      </Typography>
                      {(program.city || program.state) && (
                        <Typography variant="body2" color="text.secondary">
                          {[program.city, program.state].filter(Boolean).join(', ')}
                        </Typography>
                      )}
                    </Box>
                    <Box display="flex" flexDirection="column" alignItems="flex-end" gap={1} flexShrink={0}>
                      <Chip
                        label={program.programType}
                        size="small"
                        variant="outlined"
                        color="primary"
                      />
                      {program.isTrainingProgram && (
                        <Chip label="Training" size="small" variant="outlined" color="warning" />
                      )}
                      {isSelected && (
                        <Chip label="Current" size="small" color="primary" />
                      )}
                    </Box>
                  </Box>
                </CardContent>
              </CardActionArea>
            </Card>
          );
        })}

        {programs.length === 0 && (
          <Typography color="text.secondary" textAlign="center" py={4}>
            No care programs are assigned to your account. Contact your administrator.
          </Typography>
        )}
      </Box>
    </Box>
  );
}
