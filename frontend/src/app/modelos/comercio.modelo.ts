export interface Comercio {
  id?: number;
  pcProcessdate: string;
  pcCodcom?: string;
  pcNomcomred?: string;
  pcNumdoc?: string;
  pcTipdoc?: string;
  pcEstado?: string;
  fechaRegistro?: string;
}

export interface ComercioCuarentena {
  id: number;
  idComercioOrigen?: number;
  pcProcessdate: string;
  pcCodcom?: string;
  pcNomcomred?: string;
  pcNumdoc?: string;
  pcTipdoc?: string;
  pcEstado?: string;
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
