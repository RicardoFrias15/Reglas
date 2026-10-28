import { createTheme, type ThemeOptions } from '@mui/material/styles';

const carmesi = {
  50: '#fdecec',
  100: '#fac9c9',
  200: '#f29d9d',
  300: '#e96f6f',
  400: '#dd4545',
  500: '#c81f1f',
  600: '#a71414',
  700: '#990F02',
  800: '#7a0a01',
  900: '#5a0500'
};

const sharedComponents: ThemeOptions['components'] = {
  MuiButton: {
    defaultProps: { disableElevation: true },
    styleOverrides: { root: { textTransform: 'none', borderRadius: 8 } }
  },
  MuiAppBar: { styleOverrides: { root: { backgroundImage: 'none' } } },
  MuiPaper: { styleOverrides: { root: { backgroundImage: 'none' } } },
  MuiCard: { styleOverrides: { root: { borderRadius: 12 } } }
};

export const buildTheme = (mode: 'light' | 'dark') =>
  createTheme({
    palette: {
      mode,
      primary: { main: carmesi[700], light: carmesi[500], dark: carmesi[900], contrastText: '#fff' },
      secondary: { main: '#1f2937' },
      ...(mode === 'light'
        ? {
            background: { default: '#faf6f6', paper: '#ffffff' }
          }
        : {
            background: { default: '#161112', paper: '#1f1818' }
          })
    },
    shape: { borderRadius: 8 },
    typography: {
      fontFamily: 'Segoe UI, system-ui, -apple-system, Roboto, Arial, sans-serif',
      h5: { fontWeight: 600 },
      h6: { fontWeight: 600 }
    },
    components: sharedComponents
  });
