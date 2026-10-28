import { Box, Button, Paper, Stack, Typography } from '@mui/material';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { useQuery } from '@tanstack/react-query';
import AddIcon from '@mui/icons-material/Add';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { solicitudesApi } from '@/api/solicitudes';
import { EstatusChip } from '@/components/EstatusChip';
import type { SolicitudResumen } from '@/types/models';

export default function SolicitudesListPage() {
  const nav = useNavigate();
  const { data, isLoading } = useQuery({
    queryKey: ['solicitudes'],
    queryFn: () => solicitudesApi.listar()
  });

  const cols: GridColDef<SolicitudResumen>[] = [
    { field: 'folio', headerName: 'Folio', width: 160 },
    {
      field: 'fechaCreacion',
      headerName: 'Creada',
      width: 170,
      valueFormatter: (v: string) => new Date(v).toLocaleString()
    },
    { field: 'tema', headerName: 'Tema', flex: 1, minWidth: 220 },
    { field: 'urgencia', headerName: 'Urgencia', width: 110 },
    { field: 'tipo', headerName: 'Tipo', width: 110 },
    {
      field: 'estatus',
      headerName: 'Estatus',
      width: 200,
      renderCell: (p) => <EstatusChip value={p.row.estatus} />
    },
    { field: 'solicitante', headerName: 'Solicitante', width: 180 },
    { field: 'ejecutorAsignado', headerName: 'Ejecutor', width: 180 }
  ];

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5">Solicitudes</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          component={RouterLink}
          to="/solicitudes/nueva"
        >
          Nueva solicitud
        </Button>
      </Stack>

      <Paper sx={{ p: 0 }}>
        <Box sx={{ height: 'calc(100dvh - 220px)', width: '100%' }}>
          <DataGrid
            rows={data ?? []}
            columns={cols}
            loading={isLoading}
            disableRowSelectionOnClick
            onRowClick={(p) => nav(`/solicitudes/${p.row.id}`)}
            pageSizeOptions={[10, 25, 50]}
            initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
            sx={{ border: 0 }}
          />
        </Box>
      </Paper>
    </Stack>
  );
}
