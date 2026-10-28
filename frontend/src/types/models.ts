export type Urgencia = 'SinPrisa' | 'Urgente';
export type Tipo = 'Extra' | 'Alineado';
export type Estatus =
  | 'Recibido'
  | 'SolicitudDevuelta'
  | 'EnEsperaAsignacionEjecutor'
  | 'ResolviendoSolucion'
  | 'Finalizado'
  | 'Cancelado';

export type Rol =
  | 'Administrador'
  | 'Gestor'
  | 'AsignadorEjecutor'
  | 'Ejecutor'
  | 'Usuario';

export interface Me {
  id: string;
  email: string;
  nombreCompleto: string;
  roles: Rol[];
}

export interface SolicitudResumen {
  id: string;
  folio: string;
  fechaCreacion: string;
  fechaActualizacion: string;
  urgencia: Urgencia;
  tipo: Tipo;
  estatus: Estatus;
  tema: string;
  solicitante: string;
  ejecutorAsignado: string | null;
}

export interface DocumentoApi {
  id: string;
  titulo: string;
  descripcion: string;
  nombreArchivo: string;
  contentType: string;
  tamanoBytes: number;
  subidoEn: string;
  urlDescarga: string | null;
  subidoPor: string;
}

export interface Mensaje {
  id: string;
  contenido: string;
  creadoEn: string;
  autor: string;
}

export interface HistorialItem {
  id: string;
  estatusAnterior: Estatus;
  estatusNuevo: Estatus;
  comentario: string | null;
  cambiadoEn: string;
  cambiadoPor: string;
}

export interface SolicitudDetalle {
  id: string;
  folio: string;
  fechaCreacion: string;
  fechaActualizacion: string;
  urgencia: Urgencia;
  tipo: Tipo;
  estatus: Estatus;
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
  solicitante: string;
  solicitanteEmail: string;
  solicitanteId: string;
  ejecutorAsignado: string | null;
  ejecutorAsignadoId: string | null;
  documentos: DocumentoApi[];
  mensajes: Mensaje[];
  historial: HistorialItem[];
}

export interface UsuarioDto {
  id: string;
  nombreCompleto: string;
  email: string;
}
