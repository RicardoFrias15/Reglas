import { Chip } from '@mui/material';
import type { Estatus } from '@/types/models';

const labels: Record<Estatus, string> = {
  Recibido: 'Recibido',
  SolicitudDevuelta: 'Solicitud devuelta',
  EnEsperaAsignacionEjecutor: 'En espera de asignación',
  ResolviendoSolucion: 'Resolviendo solución',
  Finalizado: 'Finalizado',
  Cancelado: 'Cancelado'
};

const colors: Record<Estatus, 'default' | 'primary' | 'success' | 'warning' | 'info' | 'error'> = {
  Recibido: 'info',
  SolicitudDevuelta: 'warning',
  EnEsperaAsignacionEjecutor: 'warning',
  ResolviendoSolucion: 'primary',
  Finalizado: 'success',
  Cancelado: 'error'
};

export function EstatusChip({ value }: { value: Estatus }) {
  return <Chip size="small" label={labels[value]} color={colors[value]} />;
}
