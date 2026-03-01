import '@fontsource/montserrat/400.css';
import '@fontsource/montserrat/600.css';
import '@fontsource/montserrat/700.css';
import { createTheme } from '@mui/material/styles';

const fontFamily = "'Montserrat', sans-serif";

export const theme = createTheme({
  palette: {
    primary: {
      main: '#003B71',
      light: '#005DA8',
    },
    secondary: {
      main: '#F5A623',
    },
    error: {
      main: '#D32F2F',
    },
    success: {
      main: '#2E7D32',
    },
    background: {
      default: '#FFFFFF',
      paper: '#F4F7FA',
    },
    text: {
      primary: '#1A1A2E',
      secondary: '#4A5568',
    },
  },
  typography: {
    fontFamily,
    fontSize: 16,
    h1: { fontFamily, fontWeight: 700 },
    h2: { fontFamily, fontWeight: 700 },
    h3: { fontFamily, fontWeight: 700 },
    h4: { fontFamily, fontWeight: 600 },
    h5: { fontFamily, fontWeight: 600 },
    h6: { fontFamily, fontWeight: 600 },
    body1: { fontFamily, fontWeight: 400, lineHeight: 1.6 },
    body2: { fontFamily, fontWeight: 400, lineHeight: 1.6 },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 6,
          textTransform: 'none',
        },
        containedPrimary: {
          backgroundColor: '#003B71',
          '&:hover': {
            backgroundColor: '#005DA8',
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: '#003B71',
          boxShadow: 'none',
        },
      },
    },
    MuiChip: {
      defaultProps: {
        variant: 'outlined',
        color: 'primary',
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          border: '1px solid #E2E8F0',
          borderRadius: 8,
          boxShadow: 'none',
        },
      },
    },
    // NOTE: When @mui/x-data-grid is added, uncomment MuiDataGrid overrides below
    // and add `sx` on the DataGrid component's columnHeaders slot:
    //   sx={{ '& .MuiDataGrid-columnHeaders': { bgcolor: '#003B71', color: '#fff' } }}
    //
    // MuiDataGrid: {
    //   styleOverrides: {
    //     root: { border: '1px solid #E2E8F0', borderRadius: 8 },
    //     columnHeaders: {
    //       backgroundColor: '#003B71', color: '#FFFFFF',
    //       '& .MuiDataGrid-columnHeaderTitle': { fontWeight: 600 },
    //     },
    //     row: { '&:hover': { backgroundColor: '#EBF2FA' } },
    //   },
    // },

    // Table overrides (current table components use MUI Table, not DataGrid)
    MuiTableHead: {
      styleOverrides: {
        root: {
          backgroundColor: '#003B71',
          '& .MuiTableCell-head': {
            color: '#FFFFFF',
            fontWeight: 600,
          },
        },
      },
    },
    MuiTableRow: {
      styleOverrides: {
        root: {
          '&.MuiTableRow-hover:hover': {
            backgroundColor: '#EBF2FA',
          },
        },
      },
    },
  },
});
