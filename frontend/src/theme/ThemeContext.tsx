import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { CssBaseline, ThemeProvider } from '@mui/material';
import { buildTheme } from './theme';

type Mode = 'light' | 'dark';
interface ColorModeCtx {
  mode: Mode;
  toggle: () => void;
}

const Ctx = createContext<ColorModeCtx>({ mode: 'light', toggle: () => {} });
export const useColorMode = () => useContext(Ctx);

const STORAGE_KEY = 'solicitudes.colorMode';

export function ColorModeProvider({ children }: { children: ReactNode }) {
  const [mode, setMode] = useState<Mode>(() => {
    const saved = localStorage.getItem(STORAGE_KEY) as Mode | null;
    if (saved === 'light' || saved === 'dark') return saved;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  });

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, mode);
  }, [mode]);

  const value = useMemo<ColorModeCtx>(
    () => ({ mode, toggle: () => setMode((m) => (m === 'light' ? 'dark' : 'light')) }),
    [mode]
  );
  const theme = useMemo(() => buildTheme(mode), [mode]);

  return (
    <Ctx.Provider value={value}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </Ctx.Provider>
  );
}
