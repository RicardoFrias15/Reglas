import { Button, MenuItem, Paper, Stack, TextField, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { solicitudesApi } from '../api/solicitudes';
import { useNotifier } from '../components/NotificationProvider';

const schema = z.object({
  urgencia: z.enum(['SinPrisa', 'Urgente']),
  tipo: z.enum(['Extra', 'Alineado']),
  administracionBeneficiada: z.string().min(1, 'Requerido'),
  personaBeneficiada: z.string().min(1, 'Requerido'),
  nombreAdministrador: z.string().min(1, 'Requerido'),
  quienAutoriza: z.string().min(1, 'Requerido'),
  tema: z.string().min(1, 'Requerido'),
  subtema: z.string().min(1, 'Requerido'),
  objetivo: z.string().min(1, 'Requerido'),
  justificacion: z.string().min(1, 'Requerido'),
  resultadosEsperados: z.string().min(1, 'Requerido'),
  descripcionDetallada: z.string().min(1, 'Requerido')
});
type FormValues = z.infer<typeof schema>;

export default function CrearSolicitudPage() {
  const nav = useNavigate();
  const qc = useQueryClient();
  const { notify } = useNotifier();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { urgencia: 'SinPrisa', tipo: 'Alineado' }
  });

  const mut = useMutation({
    mutationFn: (data: FormValues) => solicitudesApi.crear(data),
    onSuccess: (id) => {
      notify('Solicitud creada correctamente', 'success');
      qc.invalidateQueries({ queryKey: ['solicitudes'] });
      nav(`/solicitudes/${id}`);
    },
    onError: (err) => notify(`No se pudo crear la solicitud: ${(err as Error).message}`, 'error')
  });

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h5" sx={{ mb: 2 }}>
        Nueva solicitud
      </Typography>
      <form onSubmit={handleSubmit((d) => mut.mutate(d))} noValidate>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField select fullWidth label="Urgencia" defaultValue="SinPrisa" {...register('urgencia')}>
              <MenuItem value="SinPrisa">Sin prisa</MenuItem>
              <MenuItem value="Urgente">Urgente</MenuItem>
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField select fullWidth label="Tipo" defaultValue="Alineado" {...register('tipo')}>
              <MenuItem value="Alineado">Alineado</MenuItem>
              <MenuItem value="Extra">Extra</MenuItem>
            </TextField>
          </Grid>

          {(
            [
              ['administracionBeneficiada', 'Administración beneficiada'],
              ['personaBeneficiada', 'Persona beneficiada'],
              ['nombreAdministrador', 'Nombre del administrador'],
              ['quienAutoriza', 'Quién autoriza'],
              ['tema', 'Tema'],
              ['subtema', 'Subtema']
            ] as const
          ).map(([name, label]) => (
            <Grid key={name} size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                label={label}
                error={!!errors[name]}
                helperText={errors[name]?.message}
                {...register(name)}
              />
            </Grid>
          ))}

          <Grid size={12}>
            <TextField
              fullWidth
              label="Objetivo"
              multiline
              minRows={2}
              error={!!errors.objetivo}
              helperText={errors.objetivo?.message}
              {...register('objetivo')}
            />
          </Grid>
          <Grid size={12}>
            <TextField
              fullWidth
              label="Justificación"
              multiline
              minRows={3}
              error={!!errors.justificacion}
              helperText={errors.justificacion?.message}
              {...register('justificacion')}
            />
          </Grid>
          <Grid size={12}>
            <TextField
              fullWidth
              label="Resultados o productos esperados"
              multiline
              minRows={2}
              error={!!errors.resultadosEsperados}
              helperText={errors.resultadosEsperados?.message}
              {...register('resultadosEsperados')}
            />
          </Grid>
          <Grid size={12}>
            <TextField
              fullWidth
              label="Descripción detallada"
              multiline
              minRows={4}
              error={!!errors.descripcionDetallada}
              helperText={errors.descripcionDetallada?.message}
              {...register('descripcionDetallada')}
            />
          </Grid>

          <Grid size={12}>
            <Stack direction="row" spacing={1} justifyContent="flex-end">
              <Button onClick={() => nav('/solicitudes')}>Cancelar</Button>
              <Button type="submit" variant="contained" disabled={isSubmitting || mut.isPending}>
                Crear solicitud
              </Button>
            </Stack>
          </Grid>
        </Grid>
      </form>
      <Typography variant="caption" color="text.secondary">
        Después de crearla podrás adjuntar tus archivos (PDF / Word) desde el detalle de la solicitud.
      </Typography>
    </Paper>
  );
}
