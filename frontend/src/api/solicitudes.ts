import { api } from './client';
import type {
  Me,
  SolicitudDetalle,
  SolicitudResumen,
  UsuarioDto,
  Estatus,
  Urgencia,
  Tipo
} from '@/types/models';

export const solicitudesApi = {
  me: () => api<Me>('/api/usuarios/me'),

  ejecutores: () => api<UsuarioDto[]>('/api/usuarios/ejecutores'),

  listar: (params?: { estatus?: Estatus; urgencia?: Urgencia }) => {
    const q = new URLSearchParams();
    if (params?.estatus) q.set('estatus', params.estatus);
    if (params?.urgencia) q.set('urgencia', params.urgencia);
    const qs = q.toString();
    return api<SolicitudResumen[]>(`/api/solicitudes${qs ? `?${qs}` : ''}`);
  },

  obtener: (id: string) => api<SolicitudDetalle>(`/api/solicitudes/${id}`),

  crear: (data: {
    urgencia: Urgencia;
    tipo: Tipo;
    administracionBeneficiada: string;
    personaBeneficiada: string;
    nombreAdministrador: string;
    quienAutoriza: string;
    tema: string;
    subtema: string;
    objetivo: string;
    justificacion: string;
    resultadosEsperados: string;
    descripcionDetallada: string;
  }) => api<string>('/api/solicitudes', { method: 'POST', body: data }),

  actualizarContenido: (id: string, data: { descripcionDetallada?: string; justificacion?: string }) =>
    api<void>(`/api/solicitudes/${id}/contenido`, { method: 'PUT', body: data }),

  aprobar: (id: string, comentario?: string) =>
    api<void>(`/api/solicitudes/${id}/aprobar`, { method: 'POST', body: { nuevoEstatus: 'EnEsperaAsignacionEjecutor', comentario } }),

  devolver: (id: string, comentario: string) =>
    api<void>(`/api/solicitudes/${id}/devolver`, { method: 'POST', body: { nuevoEstatus: 'SolicitudDevuelta', comentario } }),

  finalizar: (id: string, comentario?: string) =>
    api<void>(`/api/solicitudes/${id}/finalizar`, { method: 'POST', body: { nuevoEstatus: 'Finalizado', comentario } }),

  cancelar: (id: string, comentario: string) =>
    api<void>(`/api/solicitudes/${id}/cancelar`, { method: 'POST', body: { nuevoEstatus: 'Cancelado', comentario } }),

  asignarEjecutor: (id: string, ejecutorId: string) =>
    api<void>(`/api/solicitudes/${id}/asignar`, { method: 'POST', body: { ejecutorId } }),

  enviarMensaje: (id: string, contenido: string) =>
    api<{ id: string }>(`/api/solicitudes/${id}/mensajes`, { method: 'POST', body: { contenido } }),

  subirDocumento: (id: string, fd: FormData) =>
    api<{ id: string }>(`/api/solicitudes/${id}/documentos`, { method: 'POST', body: fd, isFormData: true })
};
