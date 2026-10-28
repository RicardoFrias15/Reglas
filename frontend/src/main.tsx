import React from 'react';
import ReactDOM from 'react-dom/client';
import { MsalProvider } from '@azure/msal-react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { msalInstance } from './auth/authConfig';
import { ColorModeProvider } from './theme/ThemeContext';
import { NotificationProvider } from './components/NotificationProvider';
import App from './App';

await msalInstance.initialize();
const accounts = msalInstance.getAllAccounts();
if (accounts.length > 0) msalInstance.setActiveAccount(accounts[0]);

msalInstance.addEventCallback((event) => {
  if (event.eventType === 'msal:loginSuccess' && event.payload && 'account' in event.payload) {
    msalInstance.setActiveAccount((event.payload as { account: any }).account);
  }
});

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, retry: 1 } }
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <MsalProvider instance={msalInstance}>
      <QueryClientProvider client={queryClient}>
        <ColorModeProvider>
          <NotificationProvider>
            <BrowserRouter>
              <App />
            </BrowserRouter>
          </NotificationProvider>
        </ColorModeProvider>
      </QueryClientProvider>
    </MsalProvider>
  </React.StrictMode>
);
