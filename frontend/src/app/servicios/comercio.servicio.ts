import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Comercio,
  ComercioCuarentena,
  RespuestaApi,
  ResultadoProcesoFecha,
  SolicitudProcesoFecha
} from '../modelos/comercio.modelo';

@Injectable({
  providedIn: 'root'
})
export class ComercioServicio {
  private http = inject(HttpClient);
  private urlBase = 'http://localhost:5225/api/comercios';

  /**
   * Envía el archivo CSV al backend para registrarlo en la tabla commerce
   */
  cargarArchivoCsv(archivo: File): Observable<RespuestaApi<number>> {
    const formulario = new FormData();
    formulario.append('archivo', archivo, archivo.name);
    return this.http.post<RespuestaApi<number>>(`${this.urlBase}/cargar-archivo`, formulario);
  }

  /**
   * Invoca el procesamiento y validación de comercios por fecha
   */
  procesarPorFecha(fechaProceso: string): Observable<RespuestaApi<ResultadoProcesoFecha>> {
    const solicitud: SolicitudProcesoFecha = { fechaProceso };
    return this.http.post<RespuestaApi<ResultadoProcesoFecha>>(`${this.urlBase}/procesar-fecha`, solicitud);
  }

  /**
   * Obtiene la lista de registros en la tabla commerce_quarantine
   */
  obtenerComerciosCuarentena(): Observable<RespuestaApi<ComercioCuarentena[]>> {
    return this.http.get<RespuestaApi<ComercioCuarentena[]>>(`${this.urlBase}/cuarentena`);
  }

  /**
   * Obtiene la lista de comercios registrados
   */
  obtenerComercios(fechaProceso?: string): Observable<RespuestaApi<Comercio[]>> {
    const params = fechaProceso ? `?fechaProceso=${encodeURIComponent(fechaProceso)}` : '';
    return this.http.get<RespuestaApi<Comercio[]>>(`${this.urlBase}${params}`);
  }
}
