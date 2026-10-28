import {
  PublicClientApplication,
  type Configuration,
  type RedirectRequest,
  LogLevel
} from '@azure/msal-browser';

const tenantId = import.meta.env.VITE_AZURE_TENANT_ID as string;
const clientId = import.meta.env.VITE_AZURE_CLIENT_ID as string;
const apiScope = import.meta.env.VITE_AZURE_API_SCOPE as string;

if (!tenantId || !clientId || !apiScope) {
  console.warn('Faltan variables VITE_AZURE_* en .env.local');
}

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
      loggerCallback: (level, message) => {
        if (level === LogLevel.Error) console.error(message);
      }
    }
  }
};

export const msalInstance = new PublicClientApplication(msalConfig);

export const loginRequest: RedirectRequest = {
  scopes: ['openid', 'profile', 'email', apiScope]
};

export const apiTokenRequest = {
  scopes: [apiScope]
};
