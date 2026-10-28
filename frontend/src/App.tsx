import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import { Route, Routes, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import Layout from './components/Layout';
import SolicitudesListPage from './pages/SolicitudesListPage';
import SolicitudDetallePage from './pages/SolicitudDetallePage';
import CrearSolicitudPage from './pages/CrearSolicitudPage';

export default function App() {
  return (
    <>
      <UnauthenticatedTemplate>
        <LoginPage />
      </UnauthenticatedTemplate>

      <AuthenticatedTemplate>
        <Layout>
          <Routes>
            <Route path="/" element={<Navigate to="/solicitudes" replace />} />
            <Route path="/solicitudes" element={<SolicitudesListPage />} />
            <Route path="/solicitudes/nueva" element={<CrearSolicitudPage />} />
            <Route path="/solicitudes/:id" element={<SolicitudDetallePage />} />
            <Route path="*" element={<Navigate to="/solicitudes" replace />} />
          </Routes>
        </Layout>
      </AuthenticatedTemplate>
    </>
  );
}
