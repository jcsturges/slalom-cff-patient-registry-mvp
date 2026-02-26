import { Box, Paper, Typography } from '@mui/material';

interface Props {
  title: string;
}

export function PlaceholderPage({ title }: Props) {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {title}
      </Typography>
      <Paper sx={{ p: 6, mt: 2, textAlign: 'center' }}>
        <Typography variant="h6" color="text.secondary" gutterBottom>
          Coming Soon
        </Typography>
        <Typography variant="body2" color="text.disabled">
          This feature is planned for a future release.
        </Typography>
      </Paper>
    </Box>
  );
}
