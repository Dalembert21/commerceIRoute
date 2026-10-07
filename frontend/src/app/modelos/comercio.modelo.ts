export interface Comercio {
  id?: number;
  fechaProceso: string;
  codigoComercio?: string;
  nombreComercial?: string;
  numeroDocumento?: string;
  tipoDocumento?: string;
  estado?: string;
  fechaRegistro?: string;
}

export interface ComercioCuarentena {
  id: number;
  idComercioOrigen?: number;
  fechaProceso: string;
  codigoComercio?: string;
  nombreComercial?: string;
  numeroDocumento?: string;
  tipoDocumento?: string;
  estado?: string;
  motivo: string;
  fechaCuarentena: string;
}

export interface RespuestaApi<T> {
  exito: boolean;
  mensaje: string;
  datos: T;
  totalRegistros?: number;
}

export interface SolicitudProcesoFecha {
  fechaProceso: string;
}

export interface ResultadoProcesoFecha {
  fechaProceso: string;
  registrosEnCuarentena: number;
  mensaje: string;
}
