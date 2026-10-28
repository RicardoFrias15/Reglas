import { Alert, Snackbar, type AlertColor } from '@mui/material';
import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode
} from 'react';

interface NotifierContext {
  notify: (message: string, severity?: AlertColor) => void;
}

const Ctx = createContext<NotifierContext>({ notify: () => {} });

export const useNotifier = () => useContext(Ctx);

interface NotificationState {
  message: string;
  severity: AlertColor;
  key: number;
}

export function NotificationProvider({ children }: { children: ReactNode }) {
  const [current, setCurrent] = useState<NotificationState | null>(null);

  const notify = useCallback((message: string, severity: AlertColor = 'success') => {
    setCurrent({ message, severity, key: Date.now() });
  }, []);

  const value = useMemo(() => ({ notify }), [notify]);

  return (
    <Ctx.Provider value={value}>
      {children}
      <Snackbar
        key={current?.key}
        open={!!current}
        autoHideDuration={5000}
        onClose={() => setCurrent(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        {current ? (
          <Alert
            severity={current.severity}
            variant="filled"
            onClose={() => setCurrent(null)}
            sx={{ width: '100%' }}
          >
            {current.message}
          </Alert>
        ) : undefined}
      </Snackbar>
    </Ctx.Provider>
  );
}
