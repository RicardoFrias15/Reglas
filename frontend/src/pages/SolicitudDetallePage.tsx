import { useEffect, useRef, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  IconButton,
  Link,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import SendIcon from '@mui/icons-material/Send';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import InsertDriveFileIcon from '@mui/icons-material/InsertDriveFile';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import UndoIcon from '@mui/icons-material/Undo';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import CancelIcon from '@mui/icons-material/Cancel';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { solicitudesApi } from '@/api/solicitudes';
import { EstatusChip } from '@/components/EstatusChip';
import { useNotifier } from '@/components/NotificationProvider';
import { useMe } from '@/hooks/useMe';

export default function SolicitudDetallePage() {
  const { id = '' } = useParams();
  const qc = useQueryClient();
  const me = useMe();
  const { notify } = useNotifier();

  const { data: s, isLoading } = useQuery({
    queryKey: ['solicitud', id],
    queryFn: () => solicitudesApi.obtener(id),
    enabled: !!id
  });

  const ejecutoresQ = useQuery({
    queryKey: ['ejecutores'],
    queryFn: solicitudesApi.ejecutores,
    enabled: me.esAsignadorEjecutor
  });

  const [mensaje, setMensaje] = useState('');
  const [comentarioAccion, setComentarioAccion] = useState('');
  const [ejecutorSel, setEjecutorSel] = useState('');

  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [archivoSel, setArchivoSel] = useState<File | null>(null);
  const [tituloDoc, setTituloDoc] = useState('');
  const [descDoc, setDescDoc] = useState('');

  const [edicion, setEdicion] = useState({ descripcionDetallada: '', justificacion: '' });

  useEffect(() => {
    if (s && s.estatus === 'SolicitudDevuelta') {
      setEdicion({
        descripcionDetallada: s.descripcionDetallada ?? '',
        justificacion: s.justificacion ?? ''
      });
    }
  }, [s?.id, s?.estatus, s?.descripcionDetallada, s?.justificacion]);

  const invalidar = () => qc.invalidateQueries({ queryKey: ['solicitud', id] });

  const enviarMensaje = useMutation({
    mutationFn: () => solicitudesApi.enviarMensaje(id, mensaje),
    onSuccess: () => {
      setMensaje('');
      notify('Mensaje enviado', 'success');
      invalidar();
    },
    onError: (err) => notify(`No se pudo enviar el mensaje: ${(err as Error).message}`, 'error')
  });

  const aprobar = useMutation({
    mutationFn: () => solicitudesApi.aprobar(id, comentarioAccion),
    onSuccess: () => {
      setComentarioAccion('');
      notify('Solicitud aprobada. Estatus → En espera de asignación', 'success');
      invalidar();
    },
    onError: (err) => notify(`Error al aprobar: ${(err as Error).message}`, 'error')
  });

  const devolver = useMutation({
    mutationFn: () => solicitudesApi.devolver(id, comentarioAccion),
    onSuccess: () => {
      setComentarioAccion('');
      notify('Solicitud devuelta al usuario', 'success');
      invalidar();
    },
    onError: (err) => notify(`Error al devolver: ${(err as Error).message}`, 'error')
  });

  const finalizar = useMutation({
    mutationFn: () => solicitudesApi.finalizar(id, comentarioAccion),
    onSuccess: () => {
      setComentarioAccion('');
      notify('Solicitud finalizada', 'success');
      invalidar();
    },
    onError: (err) => notify(`Error al finalizar: ${(err as Error).message}`, 'error')
  });

  const cancelar = useMutation({
    mutationFn: () => solicitudesApi.cancelar(id, comentarioAccion),
    onSuccess: () => {
      setComentarioAccion('');
      notify('Solicitud cancelada', 'success');
      invalidar();
    },
    onError: (err) => notify(`Error al cancelar: ${(err as Error).message}`, 'error')
  });

  const asignar = useMutation({
    mutationFn: () => solicitudesApi.asignarEjecutor(id, ejecutorSel),
    onSuccess: () => {
      const nombre = ejecutoresQ.data?.find((u) => u.id === ejecutorSel)?.nombreCompleto ?? 'el ejecutor';
      notify(`Asignada a ${nombre}. Estatus → Resolviendo solución`, 'success');
      setEjecutorSel('');
      invalidar();
    },
    onError: (err) => notify(`Error al asignar: ${(err as Error).message}`, 'error')
  });

  const subirDoc = useMutation({
    mutationFn: (fd: FormData) => solicitudesApi.subirDocumento(id, fd),
    onSuccess: () => {
      setArchivoSel(null);
      setTituloDoc('');
      setDescDoc('');
      if (fileInputRef.current) fileInputRef.current.value = '';
      notify('Documento subido', 'success');
      invalidar();
    },
    onError: (err) => notify(`No se pudo subir el archivo: ${(err as Error).message}`, 'error')
  });

  const reenviar = useMutation({
    mutationFn: () => solicitudesApi.actualizarContenido(id, edicion),
    onSuccess: () => {
      notify('Solicitud re-enviada. Vuelve a estatus Recibido.', 'success');
      invalidar();
    },
    onError: (err) => notify(`No se pudo re-enviar: ${(err as Error).message}`, 'error')
  });

  if (isLoading || !s) return <Typography>Cargando…</Typography>;

  const esMiSolicitud = me.data?.id === s.solicitanteId;
  const soyEjecutorAsignado = me.data?.id === s.ejecutorAsignadoId;

  const mostrarPanelGestor =
    (me.esGestor || me.esAdmin) &&
    (s.estatus === 'Recibido' || s.estatus === 'SolicitudDevuelta');

  const mostrarPanelAsignador =
    (me.esAsignadorEjecutor || me.esAdmin) &&
    s.estatus === 'EnEsperaAsignacionEjecutor';

  const mostrarPanelEjecutor =
    (soyEjecutorAsignado || me.esAdmin) &&
    s.estatus === 'ResolviendoSolucion';

  const mostrarPanelUsuario = esMiSolicitud && s.estatus === 'SolicitudDevuelta';

  const puedoSubirArchivo =
    esMiSolicitud ||
    soyEjecutorAsignado ||
    me.esAdmin;

  const handleSubir = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!archivoSel) {
      notify('Selecciona un archivo antes de subir', 'warning');
      return;
    }
    if (!tituloDoc.trim() || !descDoc.trim()) {
      notify('Título y descripción son obligatorios', 'warning');
      return;
    }
    const fd = new FormData();
    fd.set('titulo', tituloDoc);
    fd.set('descripcion', descDoc);
    fd.set('archivo', archivoSel);
    subirDoc.mutate(fd);
  };

  return (
    <Stack spacing={2}>
      <Paper sx={{ p: 3 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" flexWrap="wrap" gap={2}>
          <Box>
            <Typography variant="overline" color="text.secondary">
              {s.folio}
            </Typography>
            <Typography variant="h5">{s.tema}</Typography>
            <Typography variant="body2" color="text.secondary">
              Subtema: {s.subtema}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} alignItems="center">
            <EstatusChip value={s.estatus} />
            <Chip
              size="small"
              color={s.urgencia === 'Urgente' ? 'error' : 'default'}
              label={s.urgencia === 'Urgente' ? 'Urgente' : 'Sin prisa'}
            />
            <Chip size="small" variant="outlined" label={s.tipo} />
          </Stack>
        </Stack>
        <Divider sx={{ my: 2 }} />
        <Grid container spacing={2}>
          <Info label="Solicitante" value={`${s.solicitante} (${s.solicitanteEmail})`} />
          <Info label="Ejecutor asignado" value={s.ejecutorAsignado ?? '— Sin asignar —'} />
          <Info label="Administración beneficiada" value={s.administracionBeneficiada} />
          <Info label="Persona beneficiada" value={s.personaBeneficiada} />
          <Info label="Administrador" value={s.nombreAdministrador} />
          <Info label="Autoriza" value={s.quienAutoriza} />
          <Info label="Creada" value={new Date(s.fechaCreacion).toLocaleString()} />
          <Info label="Última actualización" value={new Date(s.fechaActualizacion).toLocaleString()} />
        </Grid>
        <Divider sx={{ my: 2 }} />
        <Grid container spacing={2}>
          <Block label="Objetivo" value={s.objetivo} />
          <Block label="Justificación" value={s.justificacion} />
          <Block label="Resultados esperados" value={s.resultadosEsperados} />
          <Block label="Descripción detallada" value={s.descripcionDetallada} />
        </Grid>
      </Paper>

      {mostrarPanelUsuario && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Re-enviar solicitud
            </Typography>
            <Alert severity="warning" sx={{ mb: 2 }}>
              El gestor te devolvió la solicitud. Revisa los comentarios en el historial,
              corrige descripción y/o justificación, y vuelve a enviar.
            </Alert>
            <Stack spacing={2}>
              <TextField
                label="Justificación"
                multiline
                minRows={3}
                value={edicion.justificacion}
                onChange={(e) => setEdicion((v) => ({ ...v, justificacion: e.target.value }))}
              />
              <TextField
                label="Descripción detallada"
                multiline
                minRows={4}
                value={edicion.descripcionDetallada}
                onChange={(e) => setEdicion((v) => ({ ...v, descripcionDetallada: e.target.value }))}
              />
              <Stack direction="row" justifyContent="flex-end">
                <Button
                  variant="contained"
                  onClick={() => reenviar.mutate()}
                  disabled={reenviar.isPending}
                >
                  Re-enviar solicitud
                </Button>
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      )}

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card sx={{ mb: 2 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Documentos
              </Typography>
              {s.documentos.length === 0 && (
                <Typography color="text.secondary">Aún no hay documentos.</Typography>
              )}
              <Stack spacing={1}>
                {s.documentos.map((d) => (
                  <Paper key={d.id} variant="outlined" sx={{ p: 1.5 }}>
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <InsertDriveFileIcon color="primary" />
                      <Box sx={{ flex: 1 }}>
                        <Typography variant="subtitle2">{d.titulo}</Typography>
                        <Typography variant="body2" color="text.secondary">
                          {d.descripcion}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {d.nombreArchivo} · {(d.tamanoBytes / 1024).toFixed(1)} KB · subido por {d.subidoPor}
                        </Typography>
                      </Box>
                      {d.urlDescarga && (
                        <Link href={d.urlDescarga} target="_blank" rel="noopener">
                          Descargar
                        </Link>
                      )}
                    </Stack>
                  </Paper>
                ))}
              </Stack>

              {puedoSubirArchivo && (
                <Box component="form" onSubmit={handleSubir} sx={{ mt: 2 }}>
                  <Stack spacing={1}>
                    <TextField
                      label="Título"
                      size="small"
                      value={tituloDoc}
                      onChange={(e) => setTituloDoc(e.target.value)}
                      required
                    />
                    <TextField
                      label="Descripción"
                      size="small"
                      multiline
                      minRows={2}
                      value={descDoc}
                      onChange={(e) => setDescDoc(e.target.value)}
                      required
                    />
                    <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap">
                      <Button variant="outlined" component="label" startIcon={<UploadFileIcon />}>
                        Elegir archivo
                        <input
                          ref={fileInputRef}
                          type="file"
                          accept=".pdf,.doc,.docx"
                          hidden
                          onChange={(e) => setArchivoSel(e.target.files?.[0] ?? null)}
                        />
                      </Button>
                      {archivoSel && (
                        <Chip
                          icon={<InsertDriveFileIcon />}
                          label={`${archivoSel.name} (${(archivoSel.size / 1024).toFixed(1)} KB)`}
                          onDelete={() => {
                            setArchivoSel(null);
                            if (fileInputRef.current) fileInputRef.current.value = '';
                          }}
                          color="primary"
                          variant="outlined"
                        />
                      )}
                      <Button type="submit" variant="contained" disabled={subirDoc.isPending || !archivoSel}>
                        Subir
                      </Button>
                    </Stack>
                    {soyEjecutorAsignado && (
                      <Typography variant="caption" color="text.secondary">
                        Como ejecutor asignado, los archivos que subas serán los entregables de la solicitud.
                      </Typography>
                    )}
                  </Stack>
                </Box>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Conversación
              </Typography>
              {s.mensajes.length === 0 && (
                <Typography color="text.secondary">Sin mensajes todavía.</Typography>
              )}
              <Stack spacing={1.5} sx={{ maxHeight: 400, overflowY: 'auto', pr: 1 }}>
                {s.mensajes.map((m) => (
                  <Paper key={m.id} variant="outlined" sx={{ p: 1.5 }}>
                    <Typography variant="caption" color="text.secondary">
                      {m.autor} · {new Date(m.creadoEn).toLocaleString()}
                    </Typography>
                    <Typography sx={{ whiteSpace: 'pre-wrap' }}>{m.contenido}</Typography>
                  </Paper>
                ))}
              </Stack>
              <Divider sx={{ my: 2 }} />
              <Stack direction="row" spacing={1}>
                <TextField
                  fullWidth
                  size="small"
                  placeholder="Escribe una actualización…"
                  value={mensaje}
                  onChange={(e) => setMensaje(e.target.value)}
                  multiline
                  maxRows={4}
                />
                <IconButton
                  color="primary"
                  disabled={!mensaje.trim() || enviarMensaje.isPending}
                  onClick={() => enviarMensaje.mutate()}
                >
                  <SendIcon />
                </IconButton>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          {mostrarPanelGestor && (
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Revisión del gestor
                </Typography>
                <Stack spacing={1.5}>
                  <TextField
                    label="Comentario"
                    placeholder="Obligatorio si vas a devolver"
                    multiline
                    minRows={2}
                    size="small"
                    value={comentarioAccion}
                    onChange={(e) => setComentarioAccion(e.target.value)}
                  />
                  <Button
                    variant="contained"
                    color="success"
                    startIcon={<CheckCircleIcon />}
                    onClick={() => aprobar.mutate()}
                    disabled={aprobar.isPending}
                  >
                    Aprobar y enviar a asignador
                  </Button>
                  <Button
                    variant="outlined"
                    color="warning"
                    startIcon={<UndoIcon />}
                    onClick={() => {
                      if (!comentarioAccion.trim()) {
                        notify('Indica el motivo de la devolución en el comentario', 'warning');
                        return;
                      }
                      devolver.mutate();
                    }}
                    disabled={devolver.isPending}
                  >
                    Devolver Solicitud
                  </Button>
                </Stack>
              </CardContent>
            </Card>
          )}

          {mostrarPanelAsignador && (
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Asignar ejecutor
                </Typography>
                <Stack spacing={1.5}>
                  <TextField
                    select
                    size="small"
                    label="Ejecutor"
                    value={ejecutorSel}
                    onChange={(e) => setEjecutorSel(e.target.value)}
                    helperText="Solo se muestran usuarios con rol Ejecutor"
                  >
                    {(ejecutoresQ.data ?? []).length === 0 && (
                      <MenuItem disabled value="">
                        No hay ejecutores registrados
                      </MenuItem>
                    )}
                    {(ejecutoresQ.data ?? []).map((u) => (
                      <MenuItem key={u.id} value={u.id}>
                        {u.nombreCompleto}
                      </MenuItem>
                    ))}
                  </TextField>
                  <Button
                    variant="contained"
                    disabled={!ejecutorSel || asignar.isPending}
                    onClick={() => asignar.mutate()}
                  >
                    Asignar (→ Resolviendo solución)
                  </Button>
                </Stack>
              </CardContent>
            </Card>
          )}

          {mostrarPanelEjecutor && (
            <Card sx={{ mb: 2 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Acción del ejecutor
                </Typography>
                <Stack spacing={1.5}>
                  <TextField
                    label="Comentario"
                    placeholder="Obligatorio si vas a cancelar"
                    multiline
                    minRows={2}
                    size="small"
                    value={comentarioAccion}
                    onChange={(e) => setComentarioAccion(e.target.value)}
                  />
                  <Button
                    variant="contained"
                    color="success"
                    startIcon={<DoneAllIcon />}
                    onClick={() => finalizar.mutate()}
                    disabled={finalizar.isPending}
                  >
                    Finalizar solicitud
                  </Button>
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<CancelIcon />}
                    onClick={() => {
                      if (!comentarioAccion.trim()) {
                        notify('Indica el motivo de la cancelación en el comentario', 'warning');
                        return;
                      }
                      cancelar.mutate();
                    }}
                    disabled={cancelar.isPending}
                  >
                    Cancelar
                  </Button>
                </Stack>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Historial
              </Typography>
              <Stack spacing={1.5}>
                {s.historial.map((h) => (
                  <Box key={h.id}>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {h.estatusAnterior} → {h.estatusNuevo}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {h.cambiadoPor} · {new Date(h.cambiadoEn).toLocaleString()}
                    </Typography>
                    {h.comentario && (
                      <Typography variant="body2" sx={{ mt: 0.5 }}>
                        {h.comentario}
                      </Typography>
                    )}
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <Grid size={{ xs: 12, sm: 6, md: 4 }}>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography>{value}</Typography>
    </Grid>
  );
}

function Block({ label, value }: { label: string; value: string }) {
  return (
    <Grid size={12}>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography sx={{ whiteSpace: 'pre-wrap' }}>{value}</Typography>
    </Grid>
  );
}
