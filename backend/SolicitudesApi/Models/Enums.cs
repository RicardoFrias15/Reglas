namespace SolicitudesApi.Models;

public enum UrgenciaSolicitud
{
    SinPrisa = 0,
    Urgente = 1
}

public enum TipoSolicitud
{
    Extra = 0,
    Alineado = 1
}

public enum EstatusSolicitud
{
    Recibido = 0,
    SolicitudDevuelta = 1,
    EnEsperaAsignacionEjecutor = 2,
    ResolviendoSolucion = 3,
    Finalizado = 4,
    Cancelado = 5
}
