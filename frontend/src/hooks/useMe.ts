import { useQuery } from '@tanstack/react-query';
import { solicitudesApi } from '@/api/solicitudes';
import type { Rol } from '@/types/models';

export function useMe() {
  const q = useQuery({ queryKey: ['me'], queryFn: solicitudesApi.me });

  const has = (...roles: Rol[]) => !!q.data && q.data.roles.some((r) => roles.includes(r));
  const esAdmin = has('Administrador');
  const esGestor = esAdmin || has('Gestor');
  const esAsignadorEjecutor = esAdmin || has('AsignadorEjecutor');
  const esEjecutor = esAdmin || has('Ejecutor');
  const esUsuario = !!q.data;

  return { ...q, esAdmin, esGestor, esAsignadorEjecutor, esEjecutor, esUsuario };
}
